

import { type User } from './models';

export interface ResultObject<T> {
  message?: string;
  exception?: any;
  success: boolean;
  data: T;
}

export interface UserAuthResponse {
  token: string;
  user: User;
}
