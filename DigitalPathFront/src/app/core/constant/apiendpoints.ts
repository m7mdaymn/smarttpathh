import { environment } from "../../../environments/environment";

export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: `${environment.apiUrl}/auth/login`,
    REGISTER_CUSTOMER: `${environment.apiUrl}/auth/register/customer`,
    REGISTER_MERCHANT: `${environment.apiUrl}/auth/register/merchant`,
  },

};
