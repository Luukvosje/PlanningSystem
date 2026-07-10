<script setup lang="ts">
import type { AvailabilityRule } from '~/types/availability'
import { formatOneTimeRuleLabel, formatWeeklyRuleLabel } from '~/utils/planning/availabilityMath'

const props = defineProps<{
  employeeId: string
  canSelectUser?: boolean
}>()

const { data: users } = useUsers()
const api = useAvailabilityApi()

const selectedEmployeeId = ref(props.employeeId)

watch(() => props.employeeId, (value) => {
  selectedEmployeeId.value = value
})

const { data: rulesData, isLoading } = useAvailabilityRules({
  employeeId: selectedEmployeeId,
})

const weeklyRules = computed(() =>
  (rulesData.value?.items ?? []).filter(rule => rule.type === 'Weekly'),
)

const oneTimeRules = computed(() =>
  (rulesData.value?.items ?? [])
    .filter(rule => rule.type === 'OneTime')
    .sort((a, b) => (a.date ?? '').localeCompare(b.date ?? '')),
)

const employeeOptions = computed(() =>
  (users.value ?? [])
    .filter(user => user.isActive !== false)
    .map(user => ({
      label: `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim() || 'Medewerker',
      value: user.id!,
    })),
)

const weeklySheetOpen = ref(false)
const oneTimeSheetOpen = ref(false)
const editingRule = ref<AvailabilityRule | null>(null)

function openWeeklyCreate() {
  editingRule.value = null
  weeklySheetOpen.value = true
}

function openWeeklyEdit(rule: AvailabilityRule) {
  editingRule.value = rule
  weeklySheetOpen.value = true
}

function openOneTimeCreate() {
  editingRule.value = null
  oneTimeSheetOpen.value = true
}

function openOneTimeEdit(rule: AvailabilityRule) {
  editingRule.value = rule
  oneTimeSheetOpen.value = true
}

async function removeRule(rule: AvailabilityRule) {
  await api.remove.mutateAsync(rule.id)
}
</script>

<template>
  <div class="space-y-8">
    <UAlert
      v-if="canSelectUser"
      color="info"
      variant="subtle"
      title="Je beheert de beschikbaarheid van een medewerker."
    />

    <UFormField v-if="canSelectUser" label="Medewerker">
      <USelect
        v-model="selectedEmployeeId"
        :items="employeeOptions"
        value-key="value"
        label-key="label"
        class="w-full max-w-md"
      />
    </UFormField>

    <section class="space-y-3">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h2 class="text-lg font-medium">
            Wekelijkse blokkades
          </h2>
          <p class="text-sm text-muted">
            Vaste momenten waarop je niet beschikbaar bent.
          </p>
        </div>
        <UButton icon="i-lucide-plus" label="Toevoegen" @click="openWeeklyCreate" />
      </div>

      <div v-if="isLoading" class="space-y-2">
        <USkeleton class="h-14 w-full" />
        <USkeleton class="h-14 w-full" />
      </div>

      <UCard v-else-if="weeklyRules.length === 0">
        <p class="text-sm text-muted text-center py-2">
          Geen vaste blokkades. Je bent standaard beschikbaar.
        </p>
      </UCard>

      <div v-else class="space-y-2">
        <UCard
          v-for="rule in weeklyRules"
          :key="rule.id"
          :ui="{ body: 'p-3 sm:p-4' }"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="min-w-0">
              <p class="font-medium">
                {{ formatWeeklyRuleLabel(rule.weekday!, rule.startTime, rule.endTime) }}
              </p>
              <p v-if="rule.reason" class="text-sm text-muted mt-0.5">
                {{ rule.reason }}
              </p>
            </div>
            <div class="flex shrink-0 gap-1">
              <UButton
                icon="i-lucide-pencil"
                variant="ghost"
                color="neutral"
                size="sm"
                @click="openWeeklyEdit(rule)"
              />
              <UButton
                icon="i-lucide-trash-2"
                variant="ghost"
                color="error"
                size="sm"
                :loading="api.remove.isPending.value"
                @click="removeRule(rule)"
              />
            </div>
          </div>
        </UCard>
      </div>
    </section>

    <section class="space-y-3">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h2 class="text-lg font-medium">
            Eenmalige uitzonderingen
          </h2>
          <p class="text-sm text-muted">
            Vakantie, afspraken of andere uitzonderingen op een datum.
          </p>
        </div>
        <UButton icon="i-lucide-plus" label="Toevoegen" @click="openOneTimeCreate" />
      </div>

      <div v-if="isLoading" class="space-y-2">
        <USkeleton class="h-14 w-full" />
      </div>

      <UCard v-else-if="oneTimeRules.length === 0">
        <p class="text-sm text-muted text-center py-2">
          Geen eenmalige uitzonderingen.
        </p>
      </UCard>

      <div v-else class="space-y-2">
        <UCard
          v-for="rule in oneTimeRules"
          :key="rule.id"
          :ui="{ body: 'p-3 sm:p-4' }"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="min-w-0">
              <p class="font-medium">
                {{ formatOneTimeRuleLabel(rule.date!, rule.startTime, rule.endTime, rule.reason) }}
              </p>
            </div>
            <div class="flex shrink-0 gap-1">
              <UButton
                icon="i-lucide-pencil"
                variant="ghost"
                color="neutral"
                size="sm"
                @click="openOneTimeEdit(rule)"
              />
              <UButton
                icon="i-lucide-trash-2"
                variant="ghost"
                color="error"
                size="sm"
                :loading="api.remove.isPending.value"
                @click="removeRule(rule)"
              />
            </div>
          </div>
        </UCard>
      </div>
    </section>

    <AvailabilityRuleSheet
      v-model:open="weeklySheetOpen"
      :employee-id="selectedEmployeeId"
      type="Weekly"
      :rule="editingRule?.type === 'Weekly' ? editingRule : null"
    />

    <AvailabilityRuleSheet
      v-model:open="oneTimeSheetOpen"
      :employee-id="selectedEmployeeId"
      type="OneTime"
      :rule="editingRule?.type === 'OneTime' ? editingRule : null"
    />
  </div>
</template>
