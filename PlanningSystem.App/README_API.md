# API Service Factory - Gebruik in Nuxt App

De ServiceFactory is nu beschikbaar in de hele Nuxt app via `useNuxtApp()` of de `useApi()` composable.

## Installatie

De plugin wordt automatisch geladen bij het starten van de app. Zorg ervoor dat:

1. De `API_URL` environment variabele is ingesteld (in `.env.dev` of `.env`)
2. De TypeScript types zijn correct geconfigureerd (al gedaan in `types/nuxt.d.ts`)

## Gebruik

### Optie 1: Via `useApi()` composable (aanbevolen)

```vue
<script setup lang="ts">
const { userService, organizationService, shiftService, serviceFactory } = useApi()

// Login
const handleLogin = async () => {
  const result = await userService.login({
    email: 'user@example.com',
    password: 'Password123!'
  })
  
  if (result.success) {
    console.log('Logged in:', result.data.user)
    // Token is automatisch opgeslagen
  }
}

// Organisaties ophalen
const loadOrganizations = async () => {
  const result = await organizationService.getUserOrganizations()
  if (result.success) {
    console.log('Organizations:', result.data)
  }
}
</script>
```

### Optie 2: Via `useNuxtApp()` direct

```vue
<script setup lang="ts">
const nuxtApp = useNuxtApp()

// ServiceFactory
const serviceFactory = nuxtApp.$serviceFactory

// Individuele services
const userService = nuxtApp.$userService
const organizationService = nuxtApp.$organizationService
const shiftService = nuxtApp.$shiftService

// Gebruik
const result = await userService.login({ email: '...', password: '...' })
</script>
```

### Optie 3: In composables

```typescript
// composables/useAuth.ts
export const useAuth = () => {
  const { userService } = useApi()
  
  const login = async (email: string, password: string) => {
    const result = await userService.login({ email, password })
    return result
  }
  
  const logout = () => {
    const { serviceFactory } = useApi()
    serviceFactory.clearToken()
  }
  
  return { login, logout }
}
```

## Configuratie

De API base URL wordt geconfigureerd via:

1. **Environment variabele**: `API_URL` in `.env.dev` of `.env`
2. **Runtime config**: `runtimeConfig.public.apiBaseUrl` in `nuxt.config.ts`
3. **Default**: `http://localhost:5157`

## Token Management

De token wordt automatisch opgeslagen na login. Je kunt ook handmatig beheren:

```typescript
const { serviceFactory } = useApi()

// Token instellen
serviceFactory.setToken('your-jwt-token')

// Token wissen
serviceFactory.clearToken()

// Base URL wijzigen
serviceFactory.setBaseUrl('https://api.example.com')
```

## Voorbeelden

### Login pagina

```vue
<template>
  <div>
    <UInput v-model="email" placeholder="Email" />
    <UInput v-model="password" type="password" placeholder="Password" />
    <UButton @click="handleLogin">Login</UButton>
  </div>
</template>

<script setup lang="ts">
const email = ref('')
const password = ref('')
const { userService } = useApi()

const handleLogin = async () => {
  const result = await userService.login({
    email: email.value,
    password: password.value
  })
  
  if (result.success) {
    // Redirect naar dashboard
    await navigateTo('/dashboard')
  } else {
    // Toon error
    console.error(result.message)
  }
}
</script>
```

### Organisaties lijst

```vue
<template>
  <div>
    <div v-for="org in organizations" :key="org.id">
      {{ org.name }}
    </div>
  </div>
</template>

<script setup lang="ts">
const organizations = ref([])
const { organizationService } = useApi()

onMounted(async () => {
  const result = await organizationService.getUserOrganizations()
  if (result.success) {
    organizations.value = result.data
  }
})
</script>
```

## TypeScript Support

Alle types zijn beschikbaar via de imports:

```typescript
import type { 
  User, 
  Organization, 
  Shift,
  ResultObject,
  UserAuthRequest 
} from '../../PlaningSystem.Typescript/modules'
```
