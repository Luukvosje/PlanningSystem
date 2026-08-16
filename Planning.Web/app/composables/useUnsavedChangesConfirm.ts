import { LazyFormUnsavedChangesModal } from '#components';

export type UnsavedChangesChoice = 'save' | 'discard' | 'cancel';

let modal: ReturnType<ReturnType<typeof useOverlay>['create']> | undefined;

/** Shared singleton dialog, reused by the route-leave guard and modal close-guards alike. */
export function useUnsavedChangesConfirm() {
  const overlay = useOverlay();

  if (!modal) {
    modal = overlay.create(LazyFormUnsavedChangesModal);
  }

  async function confirm(): Promise<UnsavedChangesChoice> {
    const choice = await modal!.open();
    return (choice as UnsavedChangesChoice | undefined) ?? 'cancel';
  }

  return { confirm };
}
