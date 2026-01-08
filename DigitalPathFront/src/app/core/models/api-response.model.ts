export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface User {
  id: string;
  email: string;
  name: string;
  role: 'customer' | 'merchant' | 'superadmin';
  phone?: string;
  avatar?: string;
  subscriptionStatus?: 'active' | 'inactive' | 'pending' | 'awaiting_approval' | 'expired';
  businessName?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface CustomerRegisterRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface CustomerRegistrationData {
  name: string;
  email: string;
  phone: string;
  password: string;
  merchantId?: string;
  merchantCode?: string;
  carPlateNumber?: string;
}

export interface MerchantPublicInfo {
  merchantId: string;
  businessName: string;
  city: string;
  address?: string;
  logo?: string;
  customLogo?: string;
  plan: string;
  isActive: boolean;
}

export interface MerchantRegisterRequest {
  name: string;
  branch: string;
  subscriptionType: 'Basic' | 'Pro';
  paymentMethod: string;
  email: string;
  password: string;
}