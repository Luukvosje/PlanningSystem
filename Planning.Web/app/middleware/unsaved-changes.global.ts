import { getDirtyForms } from '~/lib/form/dirty-registry';

export default defineNuxtRouteMiddleware(async (to, from) => {
  if (import.meta.server) {
    return;
  }

  if (to.fullPath === from.fullPath) {
    return;
  }

  const dirtyForms = getDirtyForms();
  if (!dirtyForms.length) {
    return;
  }

  const { confirm } = useUnsavedChangesConfirm();
  const choice = await confirm();

  if (choice === 'cancel') {
    return false;
  }

  if (choice === 'save') {
    const results = await Promise.all(dirtyForms.map((form) => form.submit()));
    if (results.some((succeeded) => !succeeded)) {
      return false;
    }
  }
});
