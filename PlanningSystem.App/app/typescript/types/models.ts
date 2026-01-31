export interface User {
  id: number;
  guid: string;
  name: string;
  email: string;
}

export interface Organization {
  id: number;
  name: string;
  users?: User[];
  shifts?: Shift[];
  organizationUserMaps?: OrganizationUserMap[];
}

export interface OrganizationUserMap {
  id: number;
  userId: number;
  user?: User;
  organizationId: number;
  organization?: Organization;
  role: OrganizationRole;
}

export enum OrganizationRole {
  Admin = 'Admin',
  Manager = 'Manager',
  Member = 'Member',
  Viewer = 'Viewer'
}

export interface Shift {
  id: string; // Guid as string
  startTime: string; // ISO 8601 date string
  endTime: string; // ISO 8601 date string
  location: string;
  status: string; // 'scheduled', 'finished', 'missed'
  organizationId: number;
  organization?: Organization;
  workerId: number;
  worker?: User;
}

export enum ShiftStatus {
  Scheduled = 'scheduled',
  Finished = 'finished',
  Missed = 'missed'
}
