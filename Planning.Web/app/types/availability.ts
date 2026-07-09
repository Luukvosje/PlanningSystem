export type AvailabilityType = 'DayPart' | 'TimeBlock'
export type DayPart = 'Morning' | 'Afternoon' | 'Evening'
export type AvailabilitySource = 'Employee' | 'Manager'

export interface AvailabilityEntry {
  id: string
  userId: string
  date: string
  type: AvailabilityType
  dayPart?: DayPart | null
  startTime?: string | null
  endTime?: string | null
  isAvailable: boolean
  source: AvailabilitySource
  lastModifiedByUserId: string
  lastModifiedByName: string
  note?: string | null
  createdAtUtc?: string
  updatedAtUtc?: string
}

export interface WeekAvailabilityResponse {
  items: AvailabilityEntry[]
  rangeStart: string
  rangeEnd: string
}

export interface UpsertDayPartRequest {
  userId: string
  date: string
  dayPart: DayPart
  isAvailable: boolean
  note?: string | null
}

export interface UpsertTimeBlockRequest {
  userId: string
  date: string
  startTime: string
  endTime: string
  note?: string | null
  id?: string | null
}
