// merchant-dashboard.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { MerchantService, MerchantRegistrationQR } from '../../../core/services/merchant.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/services/api.service';

interface DashboardStats {
  totalCustomers: number;
  newCustomersToday: number;
  washesToday: number;
  lastWashTime: string;
  todayRevenue: number;
  rewardsGiven: number;
  pendingRewards: number;
  // Extended statistics
  totalWashesAllTime: number;
  totalRevenueAllTime: number;
  weeklyRevenue: number;
  monthlyRevenue: number;
  washesThisWeek: number;
  washesThisMonth: number;
  activeLoyaltyCards: number;
  expiredLoyaltyCards: number;
  customersNearReward: number;
  subscriptionExpiryDate: string | null;
  plan: string;
  subscriptionStatus: string;
}

interface Activity {
  type: 'wash' | 'customer' | 'reward' | 'revenue';
  title: string;
  description: string;
  time: string;
}

interface LoyaltySettings {
  rewardName: string;
  washesRequired: number;
  timePeriod: number;
}

@Component({
  selector: 'app-merchant-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './merchant-dashboard.component.html',
  styleUrls: ['./merchant-dashboard.component.css']
})
export class MerchantDashboardComponent implements OnInit, OnDestroy {
  merchantData: any = null;
  isLoading = true;

  dashboardStats: DashboardStats = {
    totalCustomers: 0,
    newCustomersToday: 0,
    washesToday: 0,
    lastWashTime: '-',
    todayRevenue: 0,
    rewardsGiven: 0,
    pendingRewards: 0,
    // Extended statistics
    totalWashesAllTime: 0,
    totalRevenueAllTime: 0,
    weeklyRevenue: 0,
    monthlyRevenue: 0,
    washesThisWeek: 0,
    washesThisMonth: 0,
    activeLoyaltyCards: 0,
    expiredLoyaltyCards: 0,
    customersNearReward: 0,
    subscriptionExpiryDate: null,
    plan: 'basic',
    subscriptionStatus: 'inactive'
  };

  recentActivity: Activity[] = [];
  loyaltySettings: LoyaltySettings = {
    rewardName: 'غسلة مجانية',
    washesRequired: 5,
    timePeriod: 30
  };

  private refreshSubscription!: Subscription;
  merchantId: string | null = null;
  
  // QR Code Registration
  registrationQR: MerchantRegistrationQR | null = null;
  isLoadingQR = false;
  showQRModal = false;

  constructor(
    private router: Router,
    private merchantService: MerchantService,
    private authService: AuthService,
    private toast: ToastService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // Get merchantId from current user
    const user = this.authService.user();
    if (user?.id) {
      // Check if merchant account is active - check both subscription status and current status from backend
      const subscriptionStatus = (user as any).subscriptionStatus;
      if (subscriptionStatus && subscriptionStatus !== 'active') {
        this.router.navigate(['/merchant/inactive']);
        return;
      }
      this.loadMerchantIdAndDashboard(user.id);
    } else {
      this.isLoading = false;
      this.toast.showError('يجب تسجيل الدخول أولاً');
    }
    
    // Refresh data every 30 seconds
    this.refreshSubscription = interval(30000).subscribe(() => {
      if (this.merchantId) {
        this.refreshDashboardData();
      }
    });
  }

  loadMerchantIdAndDashboard(userId: string): void {
    // First get the merchantId by userId
    this.http.get<ApiResponse<string>>(`${environment.apiUrl}/merchant/by-user/${userId}`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.merchantId = response.data; // Backend returns string directly
          this.loadDashboardData();
        } else {
          this.toast.showError('فشل في تحديد معرف المغسلة');
          this.isLoading = false;
        }
      },
      error: () => {
        this.toast.showError('فشل في تحميل بيانات المغسلة');
        this.isLoading = false;
      }
    });
  }

  loadDashboardData(): void {
    // Requires merchantId - should be set by loadMerchantIdAndDashboard
    if (!this.merchantId) {
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.merchantService.getDashboard(this.merchantId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const dashboard = response.data;
          this.dashboardStats = {
            totalCustomers: dashboard.totalCustomers || 0,
            newCustomersToday: dashboard.newCustomersToday || 0,
            washesToday: dashboard.washesToday || 0,
            lastWashTime: dashboard.lastWashTime || '-',
            todayRevenue: dashboard.todayRevenue || 0,
            rewardsGiven: dashboard.rewardsGiven || 0,
            pendingRewards: dashboard.pendingRewards || 0,
            // Extended statistics from backend
            totalWashesAllTime: dashboard.totalWashesAllTime || 0,
            totalRevenueAllTime: dashboard.totalRevenue || 0,
            weeklyRevenue: dashboard.weeklyRevenue || 0,
            monthlyRevenue: dashboard.monthlyRevenue || 0,
            washesThisWeek: dashboard.washesThisWeek || 0,
            washesThisMonth: dashboard.washesThisMonth || 0,
            activeLoyaltyCards: 0,
            expiredLoyaltyCards: 0,
            customersNearReward: 0,
            subscriptionExpiryDate: dashboard.planExpiryDate || null,
            plan: dashboard.plan || 'basic',
            subscriptionStatus: dashboard.subscriptionStatus || 'inactive'
          };
          
          // Store registration info with URL
          if (dashboard.registrationCode || dashboard.qrCodeImageUrl) {
            // Use the correct URL format: /auth/register/{merchantId}
            const registrationUrl = `${window.location.origin}/auth/register/${this.merchantId}`;
            this.registrationQR = {
              registrationCode: dashboard.registrationCode || '',
              qrCodeBase64: dashboard.qrCodeImageUrl || '',
              qrCodeImage: dashboard.qrCodeImageUrl || '',
              registrationUrl: registrationUrl
            };
          }
          
          this.recentActivity = (dashboard.recentActivity || []).map((a: any) => ({
            type: a.type,
            title: a.title,
            description: a.description,
            time: a.time
          }));
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.toast.showError('فشل في تحميل بيانات لوحة التحكم');
        this.isLoading = false;
      }
    });

    // Load merchant profile
    if (this.merchantId) {
      this.merchantService.getProfile(this.merchantId).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.merchantData = {
              businessName: response.data.businessName,
              city: response.data.city,
              plan: response.data.plan?.toLowerCase() || 'basic'
            };
          }
        }
      });

      // Load settings
      this.merchantService.getSettings(this.merchantId).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.loyaltySettings = {
              rewardName: response.data.rewardDescription || 'غسلة مجانية',
              washesRequired: response.data.rewardWashesRequired || 5,
              timePeriod: response.data.rewardTimeLimitDays || 30
            };
          }
        }
      });

      // Load registration QR code
      this.loadRegistrationQR();
    }
  }

  loadRegistrationQR(): void {
    this.isLoadingQR = true;
    this.merchantService.getRegistrationQRCode(this.merchantId!).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Generate the registration URL if not present
          const qrData = response.data;
          if (!qrData.registrationUrl) {
            // Use the correct URL format: /auth/register/{merchantId}
            qrData.registrationUrl = `${window.location.origin}/auth/register/${this.merchantId}`;
          }
          this.registrationQR = qrData;
        }
        this.isLoadingQR = false;
      },
      error: () => {
        this.isLoadingQR = false;
      }
    });
  }

  generateNewQRCode(): void {
    this.isLoadingQR = true;
    this.merchantService.generateRegistrationQRCode().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const qrData = response.data;
          // Generate the registration URL if not present
          if (!qrData.registrationUrl) {
            // Use the correct URL format: /auth/register/{merchantId}
            qrData.registrationUrl = `${window.location.origin}/auth/register/${this.merchantId}`;
          }
          this.registrationQR = qrData;
          this.toast.showSuccess('تم إنشاء رمز QR جديد بنجاح');
        }
        this.isLoadingQR = false;
      },
      error: () => {
        this.toast.showError('فشل في إنشاء رمز QR');
        this.isLoadingQR = false;
      }
    });
  }

  copyRegistrationCode(): void {
    if (this.registrationQR?.registrationCode) {
      navigator.clipboard.writeText(this.registrationQR.registrationCode).then(() => {
        this.toast.showSuccess('تم نسخ الكود بنجاح');
      });
    }
  }

  copyRegistrationUrl(): void {
    if (this.registrationQR?.registrationUrl) {
      navigator.clipboard.writeText(this.registrationQR.registrationUrl).then(() => {
        this.toast.showSuccess('تم نسخ الرابط بنجاح');
      });
    }
  }

  openQRModal(): void {
    this.showQRModal = true;
  }

  closeQRModal(): void {
    this.showQRModal = false;
  }

  refreshDashboardData(): void {
    if (this.merchantId) {
      this.loadDashboardData();
    }
  }

  getTimeAgo(timeString: string): string {
    // Simple time formatting
    return `آخر غسلة ${timeString}`;
  }

  scanQR(): void {
    this.router.navigate(['/merchant/scan-qr']);
  }

  navigateTo(route: string): void {
    this.router.navigate([`/merchant/${route}`]);
  }

  addCustomer(): void {
    // Implementation for adding customer
    this.router.navigate(['/merchant/customers'], { queryParams: { add: 'true' } });
  }

  viewCustomers(): void {
    this.router.navigate(['/merchant/customers']);
  }



  viewAllActivity(): void {
    this.router.navigate(['/merchant/activity-logs']);
  }

  logout(): void {
    this.authService.logout();
    this.toast.showSuccess('تم تسجيل الخروج بنجاح');
    this.router.navigate(['/auth/signin']);
  }

  goBack(): void {
    window.history.back();
  }

  ngOnDestroy(): void {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }
}