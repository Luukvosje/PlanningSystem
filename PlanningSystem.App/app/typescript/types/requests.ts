import { OrganizationRole } from './models';

export interface UserAddRequest {
  guid: string;
  email: string;
  password: string;
  name: string;
}

export interface UserAuthRequest {
  email: string;
  password: string;
}

export interface OrganizationCreateRequest {
  name: string;
}

export interface OrganizationUpdateRequest {
  name: string;
}

export interface OrganizationAddUserRequest {
  userId: number;
  role: OrganizationRole | string;
}

export interface ShiftCreateRequest {
  startTime: string; 
  endTime: string; // ISO 8601 date string
  location: string;
  organizationId: number;
  workerId: number;
  status?: string; // Default: 'scheduled'
}

export interface ShiftUpdateRequest {
  startTime?: string;
  endTime?: string;
  location?: string;
  workerId?: number;
  status?: string;
}

export interface ShiftStatusUpdateRequest {
  status: string; // 'scheduled', 'finished', 'missed'
}

export interface ShiftFilterRequest {
  organizationId?: number;
  workerId?: number;
  startDate?: string; // ISO 8601 date string
  endDate?: string; // ISO 8601 date string
  status?: string; // 'scheduled', 'finished', 'missed'
}
