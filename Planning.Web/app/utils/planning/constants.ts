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
export const UNASSIGNED_CUSTOMER_ROW_ID = '__unassigned__';
