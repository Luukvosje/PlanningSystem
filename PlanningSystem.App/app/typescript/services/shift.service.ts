import { BaseService } from './base.service';
import {
  Shift,
  ShiftCreateRequest,
  ShiftUpdateRequest,
  ShiftStatusUpdateRequest,
  ShiftFilterRequest,
  ResultObject,
} from '../types';

export class ShiftService extends BaseService {
  async getShifts(filter?: ShiftFilterRequest): Promise<ResultObject<Shift[]>> {
    const params = new URLSearchParams();
    
    if (filter?.organizationId) {
      params.append('organizationId', filter.organizationId.toString());
    }
    if (filter?.workerId) {
      params.append('workerId', filter.workerId.toString());
    }
    if (filter?.startDate) {
      params.append('startDate', filter.startDate);
    }
    if (filter?.endDate) {
      params.append('endDate', filter.endDate);
    }
    if (filter?.status) {
      params.append('status', filter.status);
    }

    const queryString = params.toString();
    const url = queryString ? `/shift?${queryString}` : '/shift';
    
    return this.get<Shift[]>(url);
  }

  async getShift(id: string): Promise<ResultObject<Shift>> {
    return this.get<Shift>(`/shift/${id}`);
  }

  async createShift(request: ShiftCreateRequest): Promise<ResultObject<Shift>> {
    return this.post<Shift>('/shift/create', request);
  }


  async updateShift(id: string, request: ShiftUpdateRequest): Promise<ResultObject<Shift>> {
    return this.put<Shift>(`/shift/${id}`, request);
  }


  async deleteShift(id: string): Promise<ResultObject<boolean>> {
    return this.delete<boolean>(`/shift/${id}`);
  }

  async updateShiftStatus(
    id: string,
    request: ShiftStatusUpdateRequest
  ): Promise<ResultObject<Shift>> {
    return this.put<Shift>(`/shift/${id}/status`, request);
  }
}
