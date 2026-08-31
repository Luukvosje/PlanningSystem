/**
 * Page size for planning list queries. The API clamps to 2000 (PlanningService.MaxPageSize);
 * this used to be spelled out separately in four places.
 */
export const PLANNING_PAGE_SIZE = 2000;

/**
 * Timeline rows are keyed by the entity they group, so records without one need a reserved id:
 * a shift without an employee is an open shift, a booking without a customer is unassigned.
 */
export const OPEN_SHIFT_ROW_ID = '__open__';

/**
 * Value of the "open shift" option in the employee select. A select item may not carry an
 * empty string (Reka UI reserves that for clearing the selection), so the form holds this
 * sentinel and translates it back to null when saving.
 */
export const OPEN_SHIFT_SELECT_VALUE = '__open_shift__';
export const UNASSIGNED_CUSTOMER_ROW_ID = '__unassigned__';
