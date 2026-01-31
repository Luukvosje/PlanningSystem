
import { BaseService } from './base.service';
import type { User, UserAddRequest, UserAuthRequest, UserAuthResponse, ResultObject } from '../types';

export class UserService extends BaseService {
 
  async createUser(request: UserAddRequest): Promise<ResultObject<User>> {
    return this.post<User>('/user/create', request);
  }

  async login(request: UserAuthRequest): Promise<ResultObject<UserAuthResponse>> {
    const result = await this.post<UserAuthResponse>('/user/login', request);
    
    if (result.success && result.data?.token) {
      this.setToken(result.data.token);
    }
    
    return result;
  }

  async me(): Promise<ResultObject<User>> {
    return this.get<User>('/user/me')
  }

  async logout(): Promise<ResultObject<boolean>> {
    const result = await this.post<boolean>('/user/logout')
    // Clear any in-memory token (if backend also used JWT at some point)
    this.clearToken()
    return result
  }
}
