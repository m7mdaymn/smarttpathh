import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, ApiResponse } from './api.service';

export interface SystemStats {
  totalUsers: number;
  totalCustomers: number;
  totalMerchants: number;
  totalWashes: number;
  totalRevenue: number;
  activeSubscriptions: number;
  // Extended stats
  totalBusinesses?: number;
  activeBusinesses?: number;
  activeBusinessesGrowth?: number;
  inactiveBusinesses?: number;
  customerGrowth?: number;
  last30DaysWashes?: number;
  avgWashesPerDay?: number;
  avgWashesPerBusiness?: number;
  avgWashesGrowth?: number;
  totalRewards?: number;
  redeemedRewards?: number;
  basicPlanCount?: number;
  proPlanCount?: number;
  basicAvgCustomers?: number;
  basicAvgWashes?: number;
  proAvgCustomers?: number;
  proAvgWashes?: number;
}

export interface DashboardData {
  stats: SystemStats;
  recentActivity: Activity[];
  revenueByMerchant: any[];
  // Extended dashboard fields
  totalCustomers?: number;
  totalMerchants?: number;
  totalRevenue?: number;
  activeWashes?: number;
  newCustomersToday?: number;
  newMerchantsToday?: number;
  // New SaaS statistics
  newMerchantsThisMonth?: number;
  newCustomersThisMonth?: number;
  totalSaaSRevenueAllTime?: number;
  totalSaaSRevenueThisMonth?: number;
  activeMerchants?: number;
  inactiveMerchants?: number;
  basicPlanCount?: number;
  proPlanCount?: number;
  totalWashesAllTime?: number;
  totalWashesThisMonth?: number;
  totalRewardsGiven?: number;
  totalRewardsClaimed?: number;
  // Wash Revenue (merchants' earnings from washes)
  totalWashRevenueAllTime?: number;
  totalWashRevenueThisMonth?: number;
  avgWashPrice?: number;
}

export interface Activity {
  id: string;
  type: string;
  title: string;
  description: string;
  timestamp: string;
  status: string;
}

export interface MerchantDetails {
  id: string;
  businessName: string;
  ownerName: string;
  email: string;
  phone: string;
  city: string;
  plan: 'Basic' | 'Pro';
  status: 'active' | 'inactive' | 'suspended' | 'pending';
  joinDate: string;
  totalWashes: number;
  totalCustomers: number;
  subscriptionStatus: 'active' | 'expired' | 'cancelled' | 'pending' | 'suspended';
  isBlocked: boolean;
  registrationCode?: string;
  qrCodeImageUrl?: string;
}

export interface CustomerDetails {
  id: string;
  name: string;
  email: string;
  phone: string;
  joinDate: string;
  status: 'active' | 'inactive';
  totalWashes: number;
  plateNumber?: string;
  isBlocked: boolean;
}

export interface UserReport {
  id: string;
  userId: string;
  userName: string;
  type: 'merchant' | 'customer';
  reason: string;
  description: string;
  status: 'pending' | 'reviewing' | 'resolved' | 'dismissed';
  submittedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class SuperAdminService {
  constructor(private apiService: ApiService) {}

  /**
   * Get system dashboard
   */
  getDashboard(): Observable<ApiResponse<DashboardData>> {
    return this.apiService.get('SuperAdmin/dashboard');
  }

  /**
   * Get system statistics
   */
  getStatistics(): Observable<ApiResponse<SystemStats>> {
    return this.apiService.get('SuperAdmin/statistics');
  }

  /**
   * Get all merchants
   */
  getAllMerchants(page: number = 1, limit: number = 10): Observable<ApiResponse<MerchantDetails[]>> {
    return this.apiService.get(`SuperAdmin/merchants?page=${page}&limit=${limit}`);
  }

  /**
   * Get merchant details
   */
  getMerchantDetails(merchantId: string): Observable<ApiResponse<MerchantDetails>> {
    return this.apiService.get(`SuperAdmin/merchants/${merchantId}`);
  }

  /**
   * Suspend merchant account
   */
  suspendMerchant(merchantId: string, reason: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchant/${merchantId}/suspend`, {});
  }

  /**
   * Activate merchant account with subscription months
   */
  activateMerchant(merchantId: string, months: number = 1): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchant/${merchantId}/activate`, { months });
  }

  /**
   * Get all customers
   */
  getAllCustomers(page: number = 1, limit: number = 10): Observable<ApiResponse<CustomerDetails[]>> {
    return this.apiService.get(`SuperAdmin/customers?page=${page}&limit=${limit}`);
  }

  /**
   * Get customer details
   */
  getCustomerDetails(customerId: string): Observable<ApiResponse<CustomerDetails>> {
    return this.apiService.get(`SuperAdmin/customers/${customerId}`);
  }

  /**
   * Suspend customer account
   */
  suspendCustomer(customerId: string, reason: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/customer/${customerId}/suspend`, {});
  }

  /**
   * Activate customer account
   */
  activateCustomer(customerId: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/customer/${customerId}/activate`, {});
  }

  /**
   * Get user reports
   */
  getUserReports(status?: string): Observable<ApiResponse<UserReport[]>> {
    let endpoint = 'SuperAdmin/reports';
    if (status) {
      endpoint += `?status=${status}`;
    }
    return this.apiService.get(endpoint);
  }

  /**
   * Get specific report
   */
  getReport(reportId: string): Observable<ApiResponse<UserReport>> {
    return this.apiService.get(`SuperAdmin/reports/${reportId}`);
  }

  /**
   * Update report status
   */
  updateReportStatus(reportId: string, status: string, remarks?: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/reports/${reportId}`, {
      status,
      remarks
    });
  }

  /**
   * Get revenue analytics
   */
  getRevenueAnalytics(startDate?: string, endDate?: string): Observable<ApiResponse<any>> {
    let endpoint = 'SuperAdmin/revenue-analytics';
    if (startDate && endDate) {
      endpoint += `?startDate=${startDate}&endDate=${endDate}`;
    }
    return this.apiService.get(endpoint);
  }

  /**
   * Get system logs
   */
  getSystemLogs(limit: number = 100): Observable<ApiResponse<any[]>> {
    return this.apiService.get(`SuperAdmin/system-logs?limit=${limit}`);
  }

  /**
   * Get platform settings
   */
  getPlatformSettings(): Observable<ApiResponse<any>> {
    return this.apiService.get('SuperAdmin/settings');
  }

  /**
   * Update platform settings
   */
  updatePlatformSettings(settings: any): Observable<ApiResponse<any>> {
    return this.apiService.put('SuperAdmin/settings', settings);
  }

  /**
   * Send system announcement
   */
  sendAnnouncement(title: string, message: string, type: string): Observable<ApiResponse<any>> {
    return this.apiService.post('SuperAdmin/announcements', {
      title,
      message,
      type
    });
  }

  /**
   * Get subscription upgrades/downgrades
   */
  getPlanChanges(limit: number = 50): Observable<ApiResponse<any[]>> {
    return this.apiService.get(`SuperAdmin/plan-changes?limit=${limit}`);
  }

  /**
   * Export data report
   */
  exportReport(reportType: string, startDate: string, endDate: string): Observable<ApiResponse<any>> {
    return this.apiService.post('SuperAdmin/export', {
      reportType,
      startDate,
      endDate
    });
  }

  /**
   * Update merchant details
   */
  updateMerchant(merchantId: string, data: any): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchants/${merchantId}`, data);
  }

  /**
   * Create new merchant
   */
  createMerchant(data: any): Observable<ApiResponse<any>> {
    return this.apiService.post('SuperAdmin/merchants', data);
  }

  /**
   * Block merchant (User.IsActive = false, cannot login)
   */
  blockMerchant(merchantId: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchant/${merchantId}/block`, {});
  }

  /**
   * Unblock merchant (User.IsActive = true, can login)
   */
  unblockMerchant(merchantId: string): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchant/${merchantId}/unblock`, {});
  }

  /**
   * Suspend subscription (SubscriptionStatus = suspended, can login but cannot use features)
   */
  suspendSubscription(merchantId: string, reason: string = ''): Observable<ApiResponse<any>> {
    return this.apiService.put(`SuperAdmin/merchant/${merchantId}/suspend`, { reason });
  }
}
