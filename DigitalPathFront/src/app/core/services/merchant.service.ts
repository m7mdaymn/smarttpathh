import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, ApiResponse } from './api.service';

export interface MerchantProfile {
  id: string;
  businessName: string;
  ownerName?: string;
  email: string;
  phone: string;
  city: string;
  plan: 'Basic' | 'Pro';
  subscriptionStatus: 'active' | 'expired' | 'cancelled' | 'pending' | 'suspended';
  planExpiryDate?: string;
  totalCustomers?: number;
  totalWashes?: number;
  qrCodeImageUrl?: string;
  registrationCode?: string;
}

export interface MerchantCustomer {
  id: string;
  name: string;
  email: string;
  phone: string;
  carPhoto?: string;
  plateNumber?: string;
  joinDate: string;
  currentWashes: number;  // Current washes toward reward
  totalWashesRequired: number; // Washes required for reward
  daysLeft: number;  // Days left until card expires
  status: 'active' | 'inactive';
  lastWash?: string;
}

export interface MerchantDashboardStats {
  totalCustomers: number;
  totalWashes: number;
  totalRevenue: number;
  activeSubscription: string;
  subscriptionExpiry?: string;
  newCustomersToday?: number;
  washesToday?: number;
  lastWashTime?: string;
  todayRevenue?: number;
  rewardsGiven?: number;
  pendingRewards?: number;
  recentActivity?: any[];
  // Extended statistics
  totalWashesAllTime?: number;
  totalRevenueAllTime?: number;
  weeklyRevenue?: number;
  monthlyRevenue?: number;
  washesThisWeek?: number;
  washesThisMonth?: number;
  activeLoyaltyCards?: number;
  expiredLoyaltyCards?: number;
  customersNearReward?: number;
  subscriptionExpiryDate?: string | null;
  planExpiryDate?: string | null;
  plan?: string;
  subscriptionStatus?: string;
  registrationCode?: string;
  qrCodeImageUrl?: string;
}

export interface QRScanResult {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  qrCode: string;
  lastWash?: string;
  totalWashes: number;
}

export interface MerchantSettings {
  id: string;
  merchantId?: string;
  rewardWashesRequired: number;
  rewardTimeLimitDays: number;
  rewardDescription?: string;
  rewardDescriptionAr?: string;
  antiFraudSameDay: boolean;
  enableCarPhoto: boolean;
  isLoyaltyPaused: boolean;
  loyaltyPausedUntil?: string | Date | null;
  notificationsEnabled: boolean;
  notificationTemplateWelcome?: string;
  notificationTemplateRemaining?: string;
  notificationTemplateRewardClose?: string;
  customPrimaryColor?: string;
  customSecondaryColor?: string;
  customBusinessTagline?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MerchantService {
  constructor(private apiService: ApiService) {}

  /**
   * Get merchant profile
   */
  getProfile(merchantId: string): Observable<ApiResponse<MerchantProfile>> {
    return this.apiService.get(`merchant/${merchantId}/profile`);
  }

  /**
   * Update merchant profile
   */
  updateProfile(merchantId: string, data: Partial<MerchantProfile>): Observable<ApiResponse<MerchantProfile>> {
    return this.apiService.put(`merchant/${merchantId}/profile`, data);
  }

  /**
   * Get merchant dashboard
   */
  getDashboard(merchantId: string): Observable<ApiResponse<MerchantDashboardStats>> {
    return this.apiService.get(`merchant/${merchantId}/dashboard`);
  }

  /**
   * Add new customer to merchant
   */
  addCustomer(merchantId: string, customerData: {name: string, email: string, phone: string, plateNumber: string}): Observable<ApiResponse<MerchantCustomer>> {
    return this.apiService.post(`merchant/${merchantId}/customers`, customerData);
  }

  /**
   * Get all customers of merchant
   */
  getCustomers(merchantId: string): Observable<ApiResponse<MerchantCustomer[]>> {
    return this.apiService.get(`merchant/${merchantId}/customers`);
  }

  /**
   * Get specific customer details
   */
  getCustomer(merchantId: string, customerId: string): Observable<ApiResponse<MerchantCustomer>> {
    return this.apiService.get(`merchant/${merchantId}/customers/${customerId}`);
  }

  /**
   * Create a loyalty card to link a customer to a merchant
   */
  createLoyaltyCard(merchantId: string, customerId: string): Observable<ApiResponse<boolean>> {
    return this.apiService.post(`merchant/${merchantId}/loyalty-card`, { customerId });
  }

  /**
   * Validate QR code - get customer details WITHOUT recording a wash
   */
  validateCustomerQR(merchantId: string, qrCode: string): Observable<ApiResponse<QRScanResult>> {
    return this.apiService.post(`merchant/${merchantId}/validate-customer-qr`, { 
      customerQRCode: qrCode
    });
  }

  /**
   * Scan QR code - DEPRECATED: Use validateCustomerQR for validation only
   * This endpoint records a wash, use recordWash instead
   */
  scanQRCode(merchantId: string, qrCode: string): Observable<ApiResponse<QRScanResult>> {
    return this.apiService.post(`merchant/${merchantId}/scan-qr`, { 
      customerQRCode: qrCode,
      merchantId: merchantId,
      amount: 0
    });
  }

  /**
   * Record wash for customer (simple version)
   */
  recordWashSimple(merchantId: string, customerId: string, amount?: number): Observable<ApiResponse<any>> {
    return this.apiService.post(`merchant/${merchantId}/customers/${customerId}/wash`, {
      amount
    });
  }
  
  /**
   * Record wash with full details
   * Uses the new record-wash endpoint that only records (doesn't validate)
   */
  recordWash(washData: {
    customerId: string;
    customerQRCode: string; // Original QR code from scan
    merchantId: string;
    washType: string;
    price: number;
    carPlateNumber?: string;
    notes?: string;
  }): Observable<ApiResponse<any>> {
    // Use the new record-wash endpoint (separate from scan-qr)
    const processRequest = {
      customerQRCode: washData.customerQRCode,
      merchantId: washData.merchantId,
      serviceDescription: washData.washType || washData.notes || '',
      price: washData.price
    };
    return this.apiService.post(`merchant/${washData.merchantId}/record-wash`, processRequest);
  }

  /**
   * Get merchant settings
   */
  getSettings(merchantId: string): Observable<ApiResponse<MerchantSettings>> {
    return this.apiService.get(`merchant/${merchantId}/settings`);
  }

  /**
   * Update merchant settings
   */
  updateSettings(merchantId: string, settings: Partial<MerchantSettings>): Observable<ApiResponse<MerchantSettings>> {
    return this.apiService.put(`merchant/${merchantId}/settings`, settings);
  }
  /**
   * Change subscription plan
   */
  changePlan(merchantId: string, newPlan: 'Basic' | 'Pro'): Observable<ApiResponse<any>> {
    return this.apiService.post(`merchant/${merchantId}/change-plan`, { newPlan });
  }

  /**
   * Cancel subscription
   */
  cancelSubscription(merchantId: string): Observable<ApiResponse<any>> {
    return this.apiService.post(`merchant/${merchantId}/cancel-subscription`, {});
  }

  /**
   * Get revenue analytics
   */
  getAnalytics(merchantId: string, startDate?: string, endDate?: string): Observable<ApiResponse<any>> {
    let endpoint = `merchant/${merchantId}/analytics`;
    if (startDate && endDate) {
      endpoint += `?startDate=${startDate}&endDate=${endDate}`;
    }
    return this.apiService.get(endpoint);
  }

  /**
   * Change password
   */
  changePassword(merchantId: string, currentPassword: string, newPassword: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`merchant/${merchantId}/password`, {
      currentPassword,
      newPassword
    });
  }

  /**
   * Get merchant ID by user ID
   */
  getMerchantIdByUserId(userId: string): Observable<ApiResponse<string>> {
    return this.apiService.get(`merchant/by-user/${userId}`);
  }

  /**
   * Upload car photo for customer
   */
  uploadCarPhoto(merchantId: string, customerId: string, photo: File): Observable<ApiResponse<any>> {
    const formData = new FormData();
    formData.append('photo', photo);
    return this.apiService.post(`merchant/${merchantId}/customers/${customerId}/car-photo`, formData);
  }

  /**
   * Get car photos for customer
   */
  getCarPhotos(merchantId: string, customerId: string): Observable<ApiResponse<any[]>> {
    return this.apiService.get(`merchant/${merchantId}/customers/${customerId}/car-photos`);
  }

  /**
   * Get subscription status
   */
  getSubscriptionStatus(merchantId: string): Observable<ApiResponse<any>> {
    return this.apiService.get(`subscription/${merchantId}/status`);
  }

  /**
   * Check if can add customer
   */
  checkCanAddCustomer(merchantId: string): Observable<ApiResponse<boolean>> {
    return this.apiService.post(`subscription/check-can-add-customer`, {});
  }

  /**
   * Check if can create offer
   */
  checkCanCreateOffer(merchantId: string): Observable<ApiResponse<boolean>> {
    return this.apiService.post(`subscription/check-can-create-offer`, {});
  }

  /**
   * Upgrade to Pro plan
   */
  upgradeToPro(merchantId: string): Observable<ApiResponse<boolean>> {
    return this.apiService.post(`subscription/upgrade`, {});
  }

  /**
   * Validate customer QR code before wash
   */
  validateQRCode(qrCode: string): Observable<ApiResponse<boolean>> {
    return this.apiService.post(`qrcode/validate`, { qrCode });
  }

  // ========== MERCHANT REGISTRATION QR CODE METHODS ==========

  /**
   * Get merchant registration QR code (for customers to scan)
   */
  getRegistrationQRCode(merchantId: string): Observable<ApiResponse<MerchantRegistrationQR>> {
    return this.apiService.get(`qrcode/merchant/registration`);
  }

  /**
   * Generate new merchant registration QR code
   */
  generateRegistrationQRCode(): Observable<ApiResponse<MerchantRegistrationQR>> {
    return this.apiService.post(`qrcode/merchant/registration/generate`, {});
  }

  /**
   * Get public merchant info (for registration page)
   */
  getMerchantPublicInfo(merchantId: string): Observable<ApiResponse<MerchantPublicInfo>> {
    return this.apiService.get(`qrcode/merchant/${merchantId}/info`);
  }

  /**
   * Validate merchant registration code (6-digit code)
   */
  validateMerchantCode(code: string): Observable<ApiResponse<MerchantPublicInfo>> {
    return this.apiService.post(`qrcode/merchant/validate-code`, { code });
  }

  // ========== REWARD REDEMPTION METHODS ==========

  /**
   * Validate a reward QR code before redemption
   */
  validateRewardQR(merchantId: string, rewardQRCode: string): Observable<ApiResponse<RewardRedemptionResult>> {
    return this.apiService.post(`merchant/${merchantId}/validate-reward`, { rewardQRCode });
  }

  /**
   * Redeem a reward for a customer
   */
  redeemReward(merchantId: string, rewardQRCode: string): Observable<ApiResponse<RewardRedemptionResult>> {
    return this.apiService.post(`merchant/${merchantId}/redeem-reward`, { rewardQRCode });
  }
  // Add these methods to merchant.service.ts

/**
 * Update customer information
 */
updateCustomer(merchantId: string, customerId: string, data: any): Observable<ApiResponse<any>> {
  return this.apiService.put(`merchant/${merchantId}/customers/${customerId}`, data);
}

/**
 * Toggle customer active status
 */
toggleCustomerStatus(merchantId: string, customerId: string, activate: boolean): Observable<ApiResponse<any>> {
  const endpoint = activate ? 'activate' : 'deactivate';
  return this.apiService.put(`merchant/${merchantId}/customers/${customerId}/${endpoint}`, {});
}

/**
 * Delete customer
 */
deleteCustomer(merchantId: string, customerId: string): Observable<ApiResponse<any>> {
  return this.apiService.delete(`merchant/${merchantId}/customers/${customerId}`);
}

/**
 * Upload merchant logo
 */
uploadLogo(merchantId: string, logo: File): Observable<ApiResponse<string>> {
  const formData = new FormData();
  formData.append('logo', logo);
  return this.apiService.post(`merchant/${merchantId}/logo`, formData);
}

/**
 * Update merchant profile with logo
 */
updateProfileWithLogo(merchantId: string, profile: Partial<MerchantProfile>): Observable<ApiResponse<MerchantProfile>> {
  return this.apiService.put(`merchant/${merchantId}/profile`, profile);
}
}

// Reward redemption result interface
export interface RewardRedemptionResult {
  success: boolean;
  title: string;
  message: string;
  customerName: string;
  customerPhone: string;
  rewardTitle: string;
  rewardType: string;
  rewardValue: number;
  rewardExpiresAt?: Date;
  isAlreadyClaimed: boolean;
  isExpired: boolean;
}

// New interfaces for merchant registration QR
export interface MerchantRegistrationQR {
  qrCodeBase64?: string;
  qrCodeImage?: string; // Alternative property name
  registrationCode: string;
  registrationUrl?: string;
  generatedAt?: string;
  merchantId?: string;
}

export interface MerchantPublicInfo {
  merchantId: string;
  businessName: string;
  city: string;
  logo?: string;
  plan: string;
  isActive: boolean;
}
