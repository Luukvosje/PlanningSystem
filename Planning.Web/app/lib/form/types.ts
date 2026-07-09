export interface ServerErrorPayload {
  errors?: Record<string, string | string[]>
  message?: string
  title?: string
}

/** Maps ASP.NET PascalCase property names to camelCase form field names. */
export function mapServerFieldName(field: string): string {
  if (!field) {
    return field
  }
  return field.charAt(0).toLowerCase() + field.slice(1)
}

export function toFormErrors(errors: Record<string, string | string[]>) {
  return Object.entries(errors).map(([field, message]) => ({
    name: mapServerFieldName(field),
    message: Array.isArray(message) ? message[0]! : message,
  }))
}
