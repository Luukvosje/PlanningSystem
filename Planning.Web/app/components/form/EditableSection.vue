<script setup lang="ts">
import type { EditInstance } from '~/lib/form/useEdit';

/**
 * A card showing one entity's fields: the live form when you may edit them, the read-only view when
 * you may not. No Edit button and no mode switch — being allowed to change something is not a
 * reason to make you ask for it first.
 *
 * This is the standard for every entity's editable fields (customer, employee, organisation,
 * profile): a `LayoutCard`, not a plain `LayoutSection`, so the form reads as one thing you are
 * filling in rather than as loose fields on the page. Add a field to the edit composable's
 * `controls` and both the form and the read-only view pick it up.
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
	<LayoutCard
		:title="title"
		:description="description"
	>
		<component
			:is="edit.form.render"
			v-if="canEdit"
		/>

		<FormDisplay
			v-else
			:controls="controls"
			:values="values"
		/>

		<template
			v-if="canEdit"
			#footer
		>
			<component :is="edit.form.renderFooter" />
		</template>
	</LayoutCard>
</template>
