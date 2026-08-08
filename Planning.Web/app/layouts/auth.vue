<template>

  <div class="min-h-screen flex flex-col items-center justify-center p-4 bg-default">

    <div class="w-full max-w-md mb-6 flex flex-col items-center gap-4">

      <NuxtLink to="/" class="text-xl font-semibold text-highlighted">
        Planning
      </NuxtLink>
      <ColorModeSwitch class="w-full max-w-xs" />
    </div>
    <slot />
  </div>
</template>

