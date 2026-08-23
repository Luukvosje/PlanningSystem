export function usePlanningPermissions() {
  const auth = useAuthStore();

  const canManage = computed(() => canManagePlanning(auth.currentUser?.role));

  return { canManage };
}

export function usePlanning() {
  const store = usePlanningStore();
  const api = usePlanningApi();
  const toast = useToast();
  const { records, isLoading, isInitialLoading, error, refetch } = usePlanningRange();
  const { canManage } = usePlanningPermissions();

  const selectedRecord = computed(() =>
    records.value.find((r) => r.id === store.selectedPlanningId) ?? null,
  );

  async function createRecord(request: Parameters<typeof api.create>[0]) {
    try {
      const created = await api.create(request);
      api.invalidatePlanning();
      toast.add({ title: 'Planning aangemaakt', color: 'success' });
      return created;
    } catch {
      toast.add({ title: 'Aanmaken mislukt', color: 'error' });
      throw new Error('create failed');
    }
  }

  async function deleteRecord(id: string) {
    try {
      await api.delete(id);
      if (store.selectedPlanningId === id) {
store.clearSelection();
}
      api.invalidatePlanning();
      toast.add({ title: 'Planning verwijderd', color: 'success' });
    } catch {
      toast.add({ title: 'Verwijderen mislukt', color: 'error' });
    }
  }

  async function duplicateRecord(id: string) {
    try {
      const created = await api.duplicate(id);
      api.invalidatePlanning();
      // The copy lands on top of the original, so select it: without the highlight two identical
      // blocks side by side leave you guessing which one is new.
      if (created?.id) {
        store.selectPlanning(created.id, { openSidebar: false });
      }
      toast.add({ title: 'Planning gedupliceerd', color: 'success' });
    } catch {
      toast.add({ title: 'Dupliceren mislukt', color: 'error' });
    }
  }

  return {
    store,
    records,
    isLoading,
    isInitialLoading,
    error,
    refetch,
    canManage,
    selectedRecord,
    createRecord,
    deleteRecord,
    duplicateRecord,
  };
}
