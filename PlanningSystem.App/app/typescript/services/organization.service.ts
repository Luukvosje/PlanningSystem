/**
 * Organization service for organization management
 */

import { BaseService } from './base.service';
import {
  Organization,
  User,
  OrganizationCreateRequest,
  OrganizationUpdateRequest,
  OrganizationAddUserRequest,
  ResultObject,
} from '../types';

export class OrganizationService extends BaseService {

  async getUserOrganizations(): Promise<ResultObject<Organization[]>> {
    return this.get<Organization[]>('/organization');
  }

  /**
   * Get organization by ID
   */
  async getOrganization(id: number): Promise<ResultObject<Organization>> {
    return this.get<Organization>(`/organization/${id}`);
  }

  /**
   * Create a new organization
   */
  async createOrganization(request: OrganizationCreateRequest): Promise<ResultObject<Organization>> {
    return this.post<Organization>('/organization/create', request);
  }

  /**
   * Update an organization (requires Admin or Manager role)
   */
  async updateOrganization(
    id: number,
    request: OrganizationUpdateRequest
  ): Promise<ResultObject<Organization>> {
    return this.put<Organization>(`/organization/${id}`, request);
  }

  /**
   * Delete an organization (requires Admin role)
   */
  async deleteOrganization(id: number): Promise<ResultObject<boolean>> {
    return this.delete<boolean>(`/organization/${id}`);
  }

  /**
   * Get all users in an organization
   */
  async getOrganizationUsers(id: number): Promise<ResultObject<User[]>> {
    return this.get<User[]>(`/organization/${id}/users`);
  }

  /**
   * Add a user to an organization (requires Admin or Manager role)
   */
  async addUserToOrganization(
    id: number,
    request: OrganizationAddUserRequest
  ): Promise<ResultObject<boolean>> {
    return this.post<boolean>(`/organization/${id}/users`, request);
  }

  /**
   * Remove a user from an organization (requires Admin or Manager role)
   */
  async removeUserFromOrganization(id: number, userId: number): Promise<ResultObject<boolean>> {
    return this.delete<boolean>(`/organization/${id}/users/${userId}`);
  }
}
