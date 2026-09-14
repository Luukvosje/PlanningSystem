---
name: form-card
description: Add or change an editable entity card in Planning.Web — a zod schema, a useEdit or useCreate composable and a FormEditableSection or create modal. Use whenever an entity gains an editable field, a new entity needs a detail card, or a create flow is added. Covers the always-editable card rule, permissions and translations.
---

# Editable entity card

The rule: **if a user may edit a field, they see the field.** No read-mode with an Edit
button, no edit modal. `FormEditableSection` shows the live form to anyone with the right and
`FormDisplay` to everyone else. One Save for the whole card.

A `useForm(...)` definition never lives in a `.vue` file.

## Editing an existing entity

### 1. Schema — `app/schemas/<entity>.schema.ts`

A factory taking `t`, so messages are translated. Limits must match the FluentValidation rule
on the API; that validator is the security boundary, this is UX.

```ts
export function createCustomerSchema(t: Translate) {
  return z.object({
    name: z.string().min(1, t('validation.name.required')).max(200, t('validation.name.tooLong')),
  });
}
```

### 2. Definition — `app/composables/edit/use<Entity>Edit.ts`

```ts
const CUSTOMER_EDIT_KEY = Symbol('customer-edit');

export function useCustomerEdit() {
  const customersApi = useCustomersApi();
  const queryClient = useQueryClient();
  const toast = useToast();
  const { t } = useI18n();

  return useEdit(CUSTOMER_EDIT_KEY, {
    schema: createCustomerSchema(t),
    controls: computed(() => [
      { name: 'name', label: t('customers.fields.name'), type: 'input', required: true },
    ]),
    toState: (customer) => ({ name: customer.name ?? '' }),
    onSubmit: async (customer, data) => {
      await customersApi.update(customer.id, { name: data.name });
      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });
      await queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(customer.id) });
      toast.add({ title: t('customers.updated'), color: 'success' });
    },
  });
}
```

- The `Symbol` is module-level and stable — it keys the shared instance registry.
- `controls` is a `computed` because the labels come from `t()`.
- `toState` maps the API's nullable fields onto non-null form values (`?? ''`).
- `onSubmit` owns the API call, the invalidation of **both** list and detail keys, and the
  toast. The component owns none of it.

### 3. Render

```vue
<FormEditableSection
  :key="customer.id"
  :edit="customerEdit"
  :entity="customer"
  :title="t('customers.details')"
  :can-edit="canManage"
/>
```

`:key` on the entity id is what reloads the form for a different entity — not a watcher.
Render it behind a `v-if` / `UiQueryState`; `entity` must already be loaded.

`can-edit` comes from `utils/userRole.ts` (`canManageCustomers`, `canEditUserRole`, …), never
an inline role comparison.

## Creating a new entity

Same shape in `app/composables/create/use<Entity>Create.ts` with `useCreate`, plus
`initialState`, `title`, `description` and `submitLabel`. It opens `FormCreateModal` through
the Nuxt UI overlay; the page only calls `entityCreate.open()` from its `#actions` button.

`onSubmit` invalidates the list key and usually navigates to the new detail page.

## Adding a field to an existing card

1. Field on the API first (see the `db-schema-change` skill) and `pnpm generate:api`.
2. Add it to the zod schema.
3. Add it to `controls` — it appears in both the form and the read-only `FormDisplay`.
4. Map it in `toState` (edit) or `initialState` (create), and send it in `onSubmit`.
5. `i18n/locales/nl.json` **and** `en.json` for the label and any validation message.

## Fields the API cannot update

Do not put them in the form. Declare a display-only control list in
`app/composables/display/use<Entity>Fields.ts` and render `<FormDisplay>` directly — see
`components/users/Details.vue`, which shows a read-only identity card next to an editable
access card.

## Checklist

- [ ] no `useForm`/`useEdit`/`useCreate` call inside a `.vue` file
- [ ] limits match the backend validator
- [ ] both locale files updated
- [ ] `can-edit` from `utils/userRole.ts`
- [ ] list **and** detail query keys invalidated
- [ ] no `watch` — `:key`, `computed` or the handler instead
- [ ] `pnpm lint` on the files you touched
