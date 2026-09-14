<template>

	<div class="min-h-screen flex flex-col items-center justify-center p-4 bg-default">

		<div class="w-full max-w-md mb-6 flex flex-col items-center">

			<NuxtLink
				to="/"
				class="text-xl font-semibold text-highlighted"
			>
				Planning
			</NuxtLink>
		</div>
		<slot />

		<ControlsLanguageSwitch class="fixed bottom-4 left-4" />
	</div>
</template>
