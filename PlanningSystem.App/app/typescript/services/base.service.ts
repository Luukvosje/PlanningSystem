import axios,  { type AxiosInstance,type AxiosRequestConfig, type AxiosResponse, AxiosError } from 'axios';
import type { ResultObject } from '../types';

export class BaseService {
  protected axiosInstance: AxiosInstance;
  private token: string | null = null;

  constructor(baseURL: string) {
    this.axiosInstance = axios.create({
      baseURL,
      // Support HttpOnly cookie auth (backend session cookies)
      withCredentials: true,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: 30000, 
    });

    // Request interceptor to add Authorization header
    this.axiosInstance.interceptors.request.use(
      (config) => {
        if (this.token) {
          config.headers.Authorization = `Bearer ${this.token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor for error handling
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        // Handle common errors
        if (error.response) {
          // Server responded with error status
          const status = error.response.status;
          if (status === 401) {
            // Unauthorized - clear token
            this.clearToken();
          }
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Set JWT token for authentication
   */
  setToken(token: string): void {
    this.token = token;
  }

  /**
   * Get current JWT token
   */
  getToken(): string | null {
    return this.token;
  }

  /**
   * Clear JWT token
   */
  clearToken(): void {
    this.token = null;
  }

  /**
   * Update base URL
   */
  setBaseURL(baseURL: string): void {
    this.axiosInstance.defaults.baseURL = baseURL;
  }

  /**
   * Generic GET request
   */
  protected async get<T>(url: string, config?: AxiosRequestConfig): Promise<ResultObject<T>> {
    try {
      const response: AxiosResponse<ResultObject<T>> = await this.axiosInstance.get(url, config);
      return response.data;
    } catch (error) {
      return this.handleError<T>(error);
    }
  }

  /**
   * Generic POST request
   */
  protected async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ResultObject<T>> {
    try {
      const response: AxiosResponse<ResultObject<T>> = await this.axiosInstance.post(url, data, config);
      return response.data;
    } catch (error) {
      return this.handleError<T>(error);
    }
  }

  /**
   * Generic PUT request
   */
  protected async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ResultObject<T>> {
    try {
      const response: AxiosResponse<ResultObject<T>> = await this.axiosInstance.put(url, data, config);
      return response.data;
    } catch (error) {
      return this.handleError<T>(error);
    }
  }

  /**
   * Generic DELETE request
   */
  protected async delete<T>(url: string, config?: AxiosRequestConfig): Promise<ResultObject<T>> {
    try {
      const response: AxiosResponse<ResultObject<T>> = await this.axiosInstance.delete(url, config);
      return response.data;
    } catch (error) {
      return this.handleError<T>(error);
    }
  }

  /**
   * Handle API errors and convert to ResultObject format
   */
  private handleError<T>(error: any): ResultObject<T> {
    if (error.response) {
      // Server responded with error status
      const response = error.response;
      return {
        success: false,
        message: response.data?.message || error.message || 'An error occurred',
        exception: response.data?.exception || error,
        data: null as any,
      };
    } else if (error.request) {
      // Request was made but no response received
      return {
        success: false,
        message: 'No response from server. Please check your connection.',
        exception: error,
        data: null as any,
      };
    } else {
      // Something else happened
      return {
        success: false,
        message: error.message || 'An unexpected error occurred',
        exception: error,
        data: null as any,
      };
    }
  }
}
