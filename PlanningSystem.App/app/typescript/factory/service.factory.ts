import { UserService } from '../services/user.service';
import { OrganizationService } from '../services/organization.service';
import { ShiftService } from '../services/shift.service';

export class ServiceFactory {
  private baseURL: string;

  public readonly userService;
  public readonly organizationService;
  public readonly shiftService;

  constructor(baseURL: string = 'http://localhost:5157') {
    this.baseURL = baseURL;
    this.userService = new UserService(baseURL);
    this.organizationService = new OrganizationService(baseURL);
    this.shiftService = new ShiftService(baseURL);
  }


  setBaseUrl(baseURL: string): void {
    this.baseURL = baseURL;
    
    // Update existing service instances
    if (this.userService) {
      this.userService.setBaseURL(baseURL);
    }
    if (this.organizationService) {
      this.organizationService.setBaseURL(baseURL);
    }
    if (this.shiftService) {
      this.shiftService.setBaseURL(baseURL);
    }
  }


  setToken(token: string): void {
    if (this.userService) {
      this.userService.setToken(token);
    }
    if (this.organizationService) {
      this.organizationService.setToken(token);
    }
    if (this.shiftService) {
      this.shiftService.setToken(token);
    }
  }


  clearToken(): void {
    if (this.userService) {
      this.userService.clearToken();
    }
    if (this.organizationService) {
      this.organizationService.clearToken();
    }
    if (this.shiftService) {
      this.shiftService.clearToken();
    }
  }

  getBaseUrl(): string {
    return this.baseURL;
  }
}
