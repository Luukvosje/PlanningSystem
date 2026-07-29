<script setup lang="ts">
const {
  weekLabel,
  weekRangeLabel,
  isCurrentWeek,
  days,
  weekStart,
  isLoading,
  navigatePrevious,
  navigateNext,
  goToToday,
  selectWeekContaining,
} = useMyPlanningView()

const workDayCount = computed(() => days.value.filter(day => day.hasShifts).length)
</script>

<template>
  <LayoutPageContainer>
    <div class="mx-auto w-full  space-y-6">
      <LayoutPageHeader
        title="Mijn planning"
        :subtitle="workDayCount > 0
          ? `${workDayCount} ${workDayCount === 1 ? 'werkdag' : 'werkdagen'} deze week`
          : 'Geen diensten deze week'"
      />

      <USeparator />

      <PlanningMyWeekNav
        :week-label="weekLabel"
        :week-range-label="weekRangeLabel"
        :is-current-week="isCurrentWeek"
        :days="days"
        :week-start="weekStart"
        @previous="navigatePrevious"
        @next="navigateNext"
        @today="goToToday"
        @select-week="selectWeekContaining"
      />

      <div v-if="isLoading" class="space-y-3">
        <UCard
          v-for="index in 3"
          :key="index"
          :ui="{ body: 'p-5' }"
        >
          <div class="space-y-3">
            <USkeleton class="h-4 w-28" />
            <USkeleton class="h-4 w-40" />
            <USkeleton class="h-4 w-52" />
          </div>
        </UCard>
      </div>

      <div v-else class="space-y-3">
        <PlanningMyDayCard
          v-for="day in days"
          :key="day.dateKey"
          :day="day"
        />
      </div>
    </div>
  </LayoutPageContainer>
</template>
