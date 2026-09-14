<template>

	<!--
		The ground, same as the app shell: the card is `bg-default` and has to float on something.
		This was `bg-default` too, so a white card sat on a white page and only its border said
		where it began.
	-->
	<div class="min-h-screen flex flex-col items-center justify-center p-4 bg-elevated">

		<div class="w-full max-w-md mb-6 flex flex-col items-center gap-4">

			<NuxtLink
				to="/"
				class="text-xl font-semibold text-highlighted"
			>
				Planning
			</NuxtLink>
			<ControlsColorModeSwitch class="w-full max-w-xs" />
		</div>
		<slot />
	</div>
</template>

