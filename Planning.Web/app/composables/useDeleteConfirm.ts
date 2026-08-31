export interface DeleteConfirmRequest {
  title: string
  description?: string
  confirmLabel?: string
}

/**
 * Only ever set from a click handler in the browser, so it is never touched during SSR - which is
 * why a module-level variable is safe here where shared state normally would not be.
 */
let resolveAnswer: ((confirmed: boolean) => void) | null = null;

/**
 * The one dialog every delete in the app goes through. Nothing is deleted without asking first
 * (see CLAUDE.md), and keeping the guard to a single line is what makes that rule cheap enough to
 * actually follow:
 *
 * ```ts
 * if (!await confirmDelete({ title: t('customers.deleteTitle') })) {
 *   return;
 * }
 * await api.remove.mutateAsync(id);
 * ```
 *
 * The dialog itself is mounted once in `app.vue` (`UiDeleteConfirm`); this composable only carries
 * the question and hands back the answer.
 */
export function useDeleteConfirm() {
  // Open is tracked apart from the question so the dialog keeps its text while it animates out;
  // clearing both at once left an empty box on screen for the length of the transition.
  const isOpen = useState('delete-confirm-open', () => false);
  const request = useState<DeleteConfirmRequest | null>('delete-confirm', () => null);

  function confirmDelete(next: DeleteConfirmRequest): Promise<boolean> {
    // A second question while one is open would strand the first caller's promise forever.
    resolveAnswer?.(false);

    request.value = next;
    isOpen.value = true;

    return new Promise<boolean>((resolve) => {
      resolveAnswer = resolve;
    });
  }

  /** Closing the dialog any other way (escape, backdrop, cancel) answers no. */
  function answer(confirmed: boolean) {
    isOpen.value = false;
    resolveAnswer?.(confirmed);
    resolveAnswer = null;
  }

  return { isOpen, request, confirmDelete, answer };
}
