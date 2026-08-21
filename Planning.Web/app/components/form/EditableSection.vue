<script setup lang="ts">
import type { EditInstance } from '~/lib/form/useEdit';

/**
 * A section showing one entity's fields: the live form when you may edit them, the read-only view
 * when you may not. No Edit button and no mode switch — being allowed to change something is not a
 * reason to make you ask for it first.
 *
 * `entity` must already be loaded; render this behind a `v-if`.
 */
const props = withDefaults(defineProps<{
  edit: EditInstance
  entity: object
  title?: string
  description?: string
  canEdit?: boolean
}>(), {
  title: undefined,
  description: undefined,
  canEdit: true,
});

// Seeded once, deliberately: a refetch that lands mid-edit must not overwrite what is being typed.
// A page that shows a different entity under the same section keys this component on the entity id.
props.edit.load(props.entity);

// `edit` is a plain prop object, so its nested refs are not unwrapped in the template.
const controls = computed(() => props.edit.form.controls.value);
const values = computed(() => props.edit.toState(props.entity));
</script>

<template>
	<LayoutSection
		:title="title"
		:description="description"
	>
		<component
			:is="edit.form.render"
			v-if="canEdit"
		>
			<template #footer>
				<component :is="edit.form.renderFooter" />
			</template>
		</component>

		<FormDisplay
			v-else
			:controls="controls"
			:values="values"
		/>
	</LayoutSection>
</template>
