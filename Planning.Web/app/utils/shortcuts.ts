/**
 * Anything that means "the keystroke belongs to what the user is busy with, not to the page".
 *
 * The first two are what `defineShortcuts` already stands down for by itself; they are repeated
 * here so this guard is correct on its own rather than only as a top-up. Everything below them
 * is the gap: a select trigger, a switch and a date segment are buttons and divs as far as the
 * DOM is concerned, so a bare letter sails straight past them into a page-level shortcut. The
 * overlay roles close the same hole one level up — they trap the focus, which is why matching on
 * the focused element catches an open slideover or menu without reading Reka's state attributes.
 */
const EDITING_SELECTOR = [
  'input',
  'textarea',
  '[contenteditable="true"]',
  '[contenteditable="plaintext-only"]',
  '[role="combobox"]',
  '[role="spinbutton"]',
  '[role="switch"]',
  '[role="textbox"]',
  '[role="dialog"]',
  '[role="alertdialog"]',
  '[role="menu"]',
  '[role="listbox"]',
  'form',
].join(',');

/**
 * Wraps a bare-letter shortcut so it does nothing while the user is filling something in.
 *
 * Only for unmodified single keys — a `meta_k` style shortcut is unambiguous and should keep
 * working from inside a form.
 */
export function unlessEditing(handler: () => void) {
  return () => {
    if (import.meta.client && document.activeElement?.closest(EDITING_SELECTOR)) {
      return;
    }

    handler();
  };
}
