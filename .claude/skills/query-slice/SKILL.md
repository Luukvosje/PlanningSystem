---
name: query-slice
description: Wire a backend endpoint into Planning.Web — an api composable method, a query key, a TanStack useQuery composable and cache invalidation. Use whenever the frontend needs to read or write something it does not read or write yet, after regenerating the orval client.
---

# Wiring an endpoint into the frontend

The chain, in order. Every step has exactly one home; skipping one is how cache invalidation
silently stops working.

```
app/generated/api/*        orval output — never hand-edited
composables/api/use*Api.ts thin wrapper around those functions
utils/queryKeys.ts         the key, and the only place a key is written
composables/queries/use*.ts  useQuery
page / component           consumes data, isLoading, error
```

## 1. Regenerate

```bash
dotnet run --project Planning.Api      # in one terminal
cd Planning.Web && pnpm generate:api
```

If a generated type is wrong — an unexpected `| null`, a missing field — fix the C#. The
nullability annotations and `[ProducesResponseType]` on the API are what produce it. Never
hand-edit `app/generated/**`.

## 2. `composables/api/use<Feature>Api.ts`

Add a method. Name it for what it does, not for the generated function's shape.

```ts
export function useCustomersApi() {
  return {
    getAll: () => getApiCustomers(),
    getById: (id: string) => getApiCustomersId(id),
    create: (request: CreateCustomerRequest) => postApiCustomers(request),
  };
}
```

A mutation that always invalidates the same keys may expose a `use*Mutation()` here.
Everything else stays a plain call, invalidated by whoever owns the flow.

## 3. `utils/queryKeys.ts`

```ts
customers: {
  all: ['customers'] as const,
  detail: (id: string) => ['customers', id] as const,
},
```

Parameterised keys are functions, and the parameters belong **in** the key — a range query
keyed on its start and end re-fetches when the range changes and needs no watcher. `as const`
everywhere.

A literal key array anywhere outside this file is a bug.

## 4. `composables/queries/use<Thing>.ts`

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

- `enabled` guards on session state so the query does not fire before there is an
  organization. Nearly every org-scoped query needs this.
- A query taking a parameter takes a `Ref`/`ComputedRef` and uses it in both the key and the
  `queryFn` — reactive by construction, not by watcher.
- One `useQuery` per composable. A screen needing three reads calls three composables.

## 5. Invalidation

Where the mutation succeeds — the `onSubmit` of an edit/create composable, or `onSuccess` of
the mutation. Invalidate the **list and the detail** key:

```ts
await queryClient.invalidateQueries({ queryKey: queryKeys.customers.all });
await queryClient.invalidateQueries({ queryKey: queryKeys.customers.detail(id) });
```

`utils/queryInvalidation.ts` has `invalidateOrgScopedQueries` for switching organizations —
it sweeps everything except `auth` and `organizations`.

## 6. Consume

```vue
const { data, isLoading, error } = useCustomers();
```

```vue
<UiQueryState :error="error" :loading="isLoading" :loading-label="t('customers.loading')">
  ...
</UiQueryState>
```

`<UiQueryState>` does error → loading → content in that order; do not hand-roll it. Errors
carry a translated message via `useApiError(err)`; an untranslated backend string needs an
entry in `utils/backendMessages.ts`.

## 7. If it is a new page

Add the route prefix to `ROUTE_MODULE_MAP` in `utils/modules.ts`. Hiding the menu item leaves
the URL reachable — that is how `/timeline` was once open without the Planning module.

## Checklist

- [ ] nothing hand-edited under `app/generated/`
- [ ] no `$fetch` / `useFetch` against the API
- [ ] the key exists in `utils/queryKeys.ts` and nowhere else
- [ ] `enabled` guards on `auth.isAuthenticated && auth.hasOrganization` where org-scoped
- [ ] list and detail both invalidated after a mutation
- [ ] new route added to `ROUTE_MODULE_MAP`
- [ ] `pnpm lint` on the files you touched
