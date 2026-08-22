import type { BuiltInFormControl, FormControl } from './control-types';
import { isBuiltInControl } from './control-types';

interface SelectItem {
  label?: unknown
  value?: unknown
}

/**
 * The text for a control's value in a read-only view, or null when there is nothing to show — the
 * caller decides what "empty" looks like.
 *
 * A control whose `props` is a function can only be resolved against a live form, so a select
 * declared that way falls back to its raw value; give it an explicit `display` if that matters.
 */
export function formatControlValue(control: FormControl, value: unknown): string | null {
  if (control.display) {
    return control.display(value);
  }

  if (isBuiltInControl(control) && control.type === 'password') {
    return null;
  }

  if (value === null || value === undefined || value === '') {
    return null;
  }

  if (isBuiltInControl(control) && control.type === 'select') {
    const label = findSelectLabel(control, value);

    if (label !== null) {
      return label;
    }
  }

  if (Array.isArray(value)) {
    return value.length ? value.map(String).join(', ') : null;
  }

  return String(value);
}

function findSelectLabel(control: BuiltInFormControl, value: unknown): string | null {
  const props = typeof control.props === 'function' ? undefined : control.props;
  const items = props?.items;

  if (!Array.isArray(items)) {
    return null;
  }

  const match = items.find((item): item is SelectItem =>
    typeof item === 'object' && item !== null && (item as SelectItem).value === value);

  return typeof match?.label === 'string' ? match.label : null;
}
