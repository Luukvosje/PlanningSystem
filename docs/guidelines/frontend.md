# Frontend conventions

For `Planning.Web`. Read this before writing Vue or TypeScript here. The *visual* language —
tokens, radii, density, page anatomy, which shared component to reach for — lives in
[design.md](design.md); this document is about architecture and code.

Where this document and the code disagree, the code that most recently touched the same area
wins — but say so instead of silently picking one.

## Stack

Nuxt 4 (`app/` directory), Vue 3 Composition API, Nuxt UI 4 on Tailwind 4, Pinia, TanStack
Vue Query, zod 4, `@nuxtjs/i18n`, orval. pnpm 10.33.0 / Node 22. `typescript.strict: true`.

`app/composables/**` is auto-imported (`imports.dirs` in `nuxt.config.ts`), as are
`app/utils/**` and components. So `useCustomers()`, `canManagePlanning()` and `<LayoutCard>`
need no import; anything from `~/generated/`, `~/schemas/`, `~/lib/` or a type does.

Component names come from the folder: `components/form/EditableSection.vue` is
`<FormEditableSection>`, `components/layout/Card.vue` is `<LayoutCard>`,
`components/ui/DataTable.vue` is `<UiDataTable>`.

## Data flow

```
app/generated/api/*     orval-generated fetch functions  (never hand-edited)
        ↓
app/utils/apiClient.ts  customFetch: base URL, auth header, org header, refresh, errors
        ↓
composables/api/use*Api.ts       thin wrapper: one object of callable methods
        ↓
composables/queries/use*.ts      TanStack useQuery, keyed from utils/queryKeys.ts
        ↓
page / component
```

**Every HTTP call goes through this.** There is no `$fetch` or `useFetch` against the API
anywhere in `app/`; keep it that way. The old hand-written client with its PascalCase
normalizer is gone — do not reintroduce that shape.

### Regenerating the client

```bash
pnpm dev                 # in one terminal, API on :5264
pnpm generate:api        # orval, reads /swagger/v1/swagger.json, rewrites app/generated/**
```

`app/generated/**` is globally ignored by ESLint and is regenerated wholesale. If a generated
type is wrong, fix the C# — nullability and `[ProducesResponseType]` on the API are what
produce it.

### `composables/api/use*Api.ts`

One function per feature returning a plain object. Methods are the generated functions,
renamed to something readable. Mutations that always invalidate the same keys can expose a
`use*Mutation()` here; anything else stays a plain call.

```ts
export function useCustomersApi() {
  const queryClient = useQueryClient();

  return {
    getAll: () => getApiCustomers(),
    create: (request: CreateCustomerRequest) => postApiCustomers(request),
    delete: (id: string) => deleteApiCustomersId(id),
    useDeleteMutation: () => useMutation({ ... }),
  };
}
```

### `composables/queries/use*.ts`

One `useQuery` per read, key from `utils/queryKeys.ts`, and `enabled` guarding on session
state so a query does not fire before there is an organization:

```ts
export function useCustomers() {
  const auth = useAuthStore();

  return useQuery({
    queryKey: queryKeys.customers.all,
    queryFn: () => getApiCustomers(),
    enabled: computed(() => auth.isAuthenticated && auth.hasOrganization),
  });
}
```

**Query keys live only in `utils/queryKeys.ts`.** A literal array in a component is how
invalidation silently stops working. Parameterised keys are functions
(`queryKeys.planning.detail(id)`); the parameters are part of the key, so a query over a
date range re-fetches when the range changes and needs no watcher.

Invalidation happens where the mutation succeeds — in the `onSubmit` of the edit/create
composable, or `onSuccess` of the mutation. Invalidate both the list and the detail key.
`utils/queryInvalidation.ts` has the org-wide sweep for switching organizations.

### Errors

`apiClient.ts` throws a typed `ApiError` (`types/api-error.ts`). Read it with
`useApiError(err)`, which gives a translated `message` and `validationErrors`. Backend
message strings are translated by exact match in `utils/backendMessages.ts` — a new backend
message falls through untranslated (English) rather than breaking, so adding the entry is a
follow-up, not a blocker.

`apiClient.ts` also owns the refresh-token dance, including deduplication of concurrent
refreshes and a short memory of already-spent tokens. Read the comments there before
touching it; every branch is there because of a bug that logged people out.

## Forms

There is one form system, in `app/lib/form/`, surfaced through three composables. **A
`useForm(...)` definition never lives in a `.vue` file.**

| Composable | Where the definition lives | Renders as |
|---|---|---|
| `useForm` | `composables/forms/use*Form.ts` | whatever the page does with `form.render` |
| `useEdit` | `composables/edit/use*Edit.ts` | `<FormEditableSection>` — a card that is always the form |
| `useCreate` | `composables/create/use*Create.ts` | opens `FormCreateModal` through the Nuxt UI overlay |

A definition is: the zod schema (from `app/schemas/`), the `controls` array, `onSubmit`
including API call, cache invalidation, toast and navigation, and for `useEdit` a `toState`
that maps the entity onto the form. The component decides only *when* the form is visible.

```ts
const CUSTOMER_EDIT_KEY = Symbol('customer-edit');

export function useCustomerEdit() {
  return useEdit(CUSTOMER_EDIT_KEY, {
    schema: createCustomerSchema(t),
    controls: computed(() => [
      { name: 'name', label: t('customers.fields.name'), type: 'input', required: true },
      ...
    ]),
    toState: (customer) => ({ name: customer.name ?? '', ... }),
    onSubmit: async (customer, data) => { ...api, invalidate, toast... },
  });
}
```

The `Symbol` key matters: `useEdit`/`useCreate` keep one instance per key in a registry, so
the page's "open create modal" button and the modal itself share state without prop drilling.

`controls` is a `computed` because the labels come from `t()` and must re-render on a locale
switch. Control types are in `lib/form/control-types.ts`: `input | email | password |
textarea | select | switch`, or `component` for anything custom. Add a field there and both
the form and the read-only `FormDisplay` pick it up.

### Editable vs. read-only

**If a user may edit a field, they see the field — no read-mode with an Edit button, no edit
modal.** `<FormEditableSection>` renders the live form when `canEdit`, and `<FormDisplay>`
with the same controls when not. One Save for the whole card, in the form's own `#footer`
slot (only there does `type="submit"` work).

Fields the API genuinely cannot update stay read-only: declare them as a display-only control
list, e.g. `composables/display/useUserFields.ts`, and render `<FormDisplay>` directly. See
`components/users/Details.vue`, which does both — an identity card that is read-only and an
access card that is a form.

Permission comes from `utils/userRole.ts` (`canManagePlanning`, `canManageCustomers`,
`canEditUserRole`, …) — never an inline role comparison. Showing an input that returns 403 on
save is worse than not showing it.

### Validation

zod schemas live in `app/schemas/<entity>.schema.ts` and are **factories taking `t`**, so the
messages are translated:

```ts
export function createCustomerSchema(t: Translate) {
  return z.object({ name: z.string().min(1, t('validation.name.required')) });
}
```

They are UX only. The FluentValidation validator on the API is the security boundary, and the
limits must match it — a `max(200)` here mirrors `MaximumLength(200)` there.

## Deleting

**Nothing is deleted without asking first.** The default is the one-line guard:

```ts
if (!await confirmDelete({ title: t('customers.deleteTitle') })) {
  return;
}
await customersApi.delete(id);
```

`composables/useDeleteConfirm.ts` drives the single `<UiDeleteConfirm>` mounted in `app.vue`.
A component that needs more — progress while the delete runs, a dialog inside a slideover —
may own its own `<UiConfirmModal>` instead; the customer detail page does. The requirement is
the confirm, not the helper.

## Routing and access

Three global middlewares in `app/middleware/`, in this order:

- `auth.global.ts` — validates the session against `/me` (with a silent refresh) before any
  routing decision, then handles public routes, bootstrap routes (no organization yet) and
  redirects.
- `module.global.ts` — blocks a route whose module the user does not have.
- `unsaved-changes.global.ts` — the route-leave guard, fed by the form dirty registry.

**Route access is a table, not a per-page flag.** `ROUTE_MODULE_MAP` in `utils/modules.ts`
maps a path prefix to an `AppModule`; `PLANNER_ONLY_ROUTE_PREFIXES` marks the routes that
also need a planner role. A new page under a module-gated area must be added there — hiding
the menu item leaves the URL open, which is how `/timeline` was once reachable without the
Planning module at all.

`stores/auth.ts` is the only session state: `planning_access_token`,
`planning_refresh_token` and `planning_organization_id` as `useCookie`. Do not add a second
mechanism (localStorage, another store, a plugin) — extend the store. Note the tokens are
JS-readable by design, so any XSS is a full account takeover: treat `v-html` and dynamically
built URLs carrying user content as high severity.

## Pages

```vue
<script setup lang="ts">
definePageMeta({ layout: false });     // only when the page fills a layout slot

const auth = useAuthStore();
const canManage = computed(() => canManageCustomers(auth.currentUser?.role));
const { data: customers, isLoading, error } = useCustomers();
const { t } = useI18n();
</script>

<template>
	<NuxtLayout name="default">
		<template #actions> ... </template>

		<LayoutPageContainer fill>
			<UiQueryState :error="error" :loading="isLoading" :loading-label="t('customers.loading')">
				...
			</UiQueryState>
		</LayoutPageContainer>
	</NuxtLayout>
</template>
```

A page loads data, resolves permissions, and decides what is visible. Business logic goes in
a composable, a form definition in `composables/*`, a calculation in `utils/planning/*`.

`<UiQueryState>` handles error → loading → content in that order; do not hand-roll it.

## Composables — where does it go?

| Folder | Contents |
|---|---|
| `composables/api/` | one wrapper per API area |
| `composables/queries/` | one `useQuery` per read |
| `composables/forms/` | standalone form definitions (login, register, settings) |
| `composables/edit/` | `useEdit` definitions for entity cards |
| `composables/create/` | `useCreate` definitions for create modals |
| `composables/display/` | read-only control lists for `FormDisplay` |
| `composables/planning/` | the planning board's own state: filters, drag, resize, timeline |
| `composables/entities/` | shared entity helpers (recent entities, entity source) |
| `composables/` (root) | cross-cutting: `useApiError`, `useDeleteConfirm`, `useEntityTabs`, … |

Pure functions with no Vue reactivity go in `app/utils/` — `utils/planning/availabilityMath.ts`,
`overlapLayout.ts`, `timelineMath.ts`. That is what makes them testable, and
`test/timelineMath.spec.ts` is the example.

## i18n

Every user-facing string goes through `t()` / `$t()`, with an entry in **both**
`i18n/locales/nl.json` and `i18n/locales/en.json`. `nl` is the default and the fallback;
`strategy: 'no_prefix'` so there is no locale segment in URLs.

Route paths are Dutch where the feature is Dutch (`/beschikbaarheid`, `/aanvragen`) and
English elsewhere (`/customers`, `/users`). Follow the surrounding area rather than
normalising; renaming a route means touching `ROUTE_MODULE_MAP` too.

`apiClient.ts` sends the active locale as `Accept-Language`, which is what localizes
FluentValidation's default messages server-side.

## State: no watchers

Derived state is a `computed`. Something that must react to an event belongs in the handler,
in a lifecycle hook, or in a `:key` that rebuilds the component — see `<FormEditableSection
:key="customer.id">`, which reloads the form for a different entity without a watcher.

A `watch`/`watchEffect` is the answer only when there is genuinely no other route, and then
it carries a comment saying why. Watchers hide what caused a change, create a second source
of truth, and are where ordering bugs start.

## Style

ESLint (`eslint.config.mjs`) is the authority; run `pnpm lint` rather than guessing. The
rules that bite:

- `@typescript-eslint/no-explicit-any: 'error'` — fix the type, never `as any`.
- Single quotes, semicolons always, `curly: all`, trailing commas always-multiline.
- **Tabs** for Vue template indentation (`vue/html-indent`), spaces in `<script>`.
- One attribute per line on multiline elements, closing bracket on its own line.
- `no-nested-ternary`, `prefer-const`, `operator-linebreak: after`.
- `no-console` warns except `info`/`warn`/`error`.

`<script setup lang="ts">` only. Type-only `defineProps`/`defineEmits` with
`withDefaults(...)` — `vue/require-default-prop` is on, so every optional prop needs a
default, including `undefined`.

Tailwind utility classes only; semantic tokens rather than raw palette colours. No `<style>`
block. See [design.md](design.md).

## Tests

Vitest, in `Planning.Web/test/`. Per `CLAUDE.md` we do not write tests by default; the
exception is a pure calculation in `utils/planning/` — overlap layout, timeline maths,
availability expansion. Those are cheap, deterministic and the place where a silent
off-by-one costs a whole afternoon.

```bash
pnpm test
pnpm exec vitest run test/timelineMath.spec.ts
```

## Open questions

Places where the codebase is inconsistent. Pick an existing variant, do not invent a third,
and settle it in `docs/decisions/`.

- **`onMounted(() => auth.fetchMe())` in pages.** Several pages re-fetch the session on mount
  even though `auth.global.ts` already validated it via `ensureSession`. Harmless but
  redundant; new pages should not copy it.
- **Two launch configs.** `.claude/launch.json` starts the dev server on port 3000,
  `Planning.Web/.claude/launch.json` on 3100. Both ports are allowed by the development CORS
  config, so either works.
