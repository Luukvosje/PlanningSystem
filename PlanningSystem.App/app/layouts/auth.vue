<template>
  <div class="min-h-screen flex items-center justify-center p-6">
    <div class="w-full max-w-md">
      <div class="flex items-center justify-center mb-6">
        <NuxtLink to="/" class="flex items-center gap-3">
          <AppLogo class="w-auto h-7 shrink-0" />
        </NuxtLink>
      </div>

      <slot />
    </div>
  </div>
</template>

