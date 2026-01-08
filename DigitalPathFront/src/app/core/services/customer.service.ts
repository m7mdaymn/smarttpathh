import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ToastService } from './toast.service';
import { AuthService } from './auth.service';
import { ApiResponse } from './api.service';

export interface CustomerProfile {
  id: string;
  name: string;
  email: string;
  phone: string;
  avatar?: string;
  qrCode: string;
  walletBalance: number;
  loyaltyCards: LoyaltyCard[];
  washes: WashHistory[];
  notifications: Notification[];
  totalWashes?: number;
  loyaltyPoints?: number;
}

export interface LoyaltyCard {
  id: string;
  merchantId: string;
  merchantName: string;
  merchantLogo?: string;
  carImage?: string;
  washesRemaining: number;
  totalWashes: number;
  expiresAt: string;
  progress: number;
  isActive: boolean;
  isPaused?: boolean;
  merchant?: string;
  washesCompleted?: number;
  washesRequired?: number;
  expiryDate?: string;
  status?: string;
  
  // Extended merchant settings
  merchantCity?: string;
  merchantPhone?: string;
  customPrimaryColor?: string;
  customSecondaryColor?: string;
  customBusinessTagline?: string;
  customRewardMessage?: string;
  rewardDescription?: string;
  rewardTimeLimitDays?: number;
  
  // Reward status
  isRewardEarned?: boolean;
  rewardQRCode?: string;
  rewardEarnedAt?: string;
  rewardExpiresAt?: string;
  isRewardClaimed?: boolean;
  rewardClaimedAt?: string;
  
  // Statistics
  daysRemaining?: number;
  totalWashesWithMerchant?: number;
  lastWashDate?: string;
  
  // Computed display helpers
  currentWashes?: number;
}

export interface WashHistory {
  id: string;
  merchantName: string;
  merchantLogo?: string;
  date: string;
  time: string;
  amount: number;
  status: 'completed' | 'pending' | 'cancelled';
  rating?: number;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'promotion';
  date: string;
  read: boolean;
  icon: string;
}

export interface Reward {
  id: string;
  title: string;
  description: string;
  type: 'free_wash' | 'discount' | 'upgrade';
  value: number;
  expiresAt: string;
  status: 'available' | 'claimed' | 'expired';
  pointsRequired?: number;
  currentPoints?: number;
  merchant?: string;
  icon?: string;
}

export interface CarPhoto {
  id: number;
  photoUrl: string;
  uploadedAt: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private profile = signal<CustomerProfile | null>(null);
  private washes = signal<WashHistory[]>([]);
  private notifications = signal<Notification[]>([]);
  private rewards = signal<Reward[]>([]);

  readonly customerProfile = computed(() => this.profile());
  readonly washHistory = computed(() => this.washes());
  readonly customerNotifications = computed(() => this.notifications());
  readonly customerRewards = computed(() => this.rewards());

  constructor(
    private http: HttpClient,
    private toast: ToastService,
    private authService: AuthService
  ) {
    // No longer loading demo data - using real APIs
  }

  // Demo Data - DEPRECATED (keeping for fallback only)
  private loadDemoData(): void {
    // Demo Profile
    const demoProfile: CustomerProfile = {
      id: 'cust_001',
      name: 'محمد أحمد',
      email: 'mohamed@example.com',
      phone: '0551234567',
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Mohamed',
      qrCode: 'DP-CUST-001-2024',
      walletBalance: 250,
      loyaltyCards: [
        {
          id: 'card_001',
          merchantId: 'merch_001',
          merchantName: 'مغسلة النور',
          merchantLogo: 'https://api.dicebear.com/7.x/shapes/svg?seed=Nour',
          carImage: 'https://api.dicebear.com/7.x/shapes/svg?seed=Car1',
          washesRemaining: 3,
          totalWashes: 5,
          expiresAt: '2024-03-15',
          progress: 40,
          isActive: true
        },
        {
          id: 'card_002',
          merchantId: 'merch_002',
          merchantName: 'مغسلة الأصالة',
          merchantLogo: 'https://api.dicebear.com/7.x/shapes/svg?seed=Asala',
          carImage: 'https://api.dicebear.com/7.x/shapes/svg?seed=Car2',
          washesRemaining: 8,
          totalWashes: 10,
          expiresAt: '2024-04-20',
          progress: 20,
          isActive: true
        }
      ],
      washes: [
        {
          id: 'wash_001',
          merchantName: 'مغسلة النور',
          merchantLogo: 'https://api.dicebear.com/7.x/shapes/svg?seed=Nour',
          date: '2024-01-15',
          time: '10:30 ص',
          amount: 50,
          status: 'completed',
          rating: 5
        },
        {
          id: 'wash_002',
          merchantName: 'مغسلة الأصالة',
          merchantLogo: 'https://api.dicebear.com/7.x/shapes/svg?seed=Asala',
          date: '2024-01-10',
          time: '03:45 م',
          amount: 60,
          status: 'completed',
          rating: 4
        }
      ],
      notifications: [
        {
          id: 'notif_001',
          title: 'مغسلة النور',
          message: 'عرض خاص: احصل على غسلة مجانية بعد 3 زيارات!',
          type: 'promotion',
          date: '2024-01-14',
          read: false,
          icon: '🎁'
        },
        {
          id: 'notif_002',
          title: 'تذكير',
          message: 'باقي لك غسلتين للحصول على مكافأة مجانية في مغسلة النور',
          type: 'info',
          date: '2024-01-13',
          read: false,
          icon: '⏰'
        }
      ]
    };

    // Demo Rewards
    const demoRewards: Reward[] = [
      {
        id: 'reward_001',
        title: 'غسلة مجانية',
        description: 'احصل على غسلة مجانية بعد إكمال 5 زيارات',
        type: 'free_wash',
        value: 100,
        expiresAt: '2024-02-28',
        status: 'available'
      },
      {
        id: 'reward_002',
        title: 'خصم 20%',
        description: 'خصم 20% على خدمة التلميع',
        type: 'discount',
        value: 20,
        expiresAt: '2024-01-31',
        status: 'available'
      }
    ];

    this.profile.set(demoProfile);
    this.washes.set(demoProfile.washes);
    this.notifications.set(demoProfile.notifications);
    this.rewards.set(demoRewards);
  }

  // Real API Methods
  getCustomerProfile(): Observable<CustomerProfile> {
    const userId = this.authService.user()?.id;
    
    if (!userId) {
      return of(this.profile()!);
    }

    return this.http.get<ApiResponse<CustomerProfile>>(`${environment.apiUrl}/customer/${userId}/profile`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            // Map backend DTO to frontend interface
            const profile: CustomerProfile = {
              ...response.data,
              loyaltyCards: (response.data as any).loyaltyCards?.map((lc: any) => ({
                id: lc.id,
                merchantId: lc.merchantId,
                merchantName: lc.merchantName,
                merchantLogo: lc.merchantLogo,
                washesRemaining: lc.washesRequired - lc.washesCompleted,
                totalWashes: lc.washesRequired,
                expiresAt: lc.expiresAt,
                progress: lc.progress || 0,
                isActive: lc.isActive
              })) || [],
              washes: (response.data as any).washes?.map((w: any) => ({
                id: w.id,
                merchantName: w.merchantName,
                merchantLogo: w.merchantLogo,
                date: w.washDate?.split('T')[0] || w.washDate,
                time: new Date(w.washDate).toLocaleTimeString('ar-SA', { hour: '2-digit', minute: '2-digit' }),
                amount: w.amount,
                status: w.status,
                rating: w.rating
              })) || [],
              notifications: (response.data as any).notifications?.map((n: any) => ({
                id: n.id,
                title: n.title,
                message: n.message,
                type: n.type,
                date: n.createdAt?.split('T')[0] || n.createdAt,
                read: n.isRead,
                icon: this.getNotificationIcon(n.type)
              })) || []
            };
            this.profile.set(profile);
            return profile;
          }
          throw new Error(response.message || 'Failed to load profile');
        }),
        catchError(error => {
          this.toast.showError('فشل في تحميل بيانات الملف الشخصي');
          return of(this.profile()!); // Return cached data on error
        })
      );
  }

  private getNotificationIcon(type: string): string {
    switch (type) {
      case 'promotion': return '🎁';
      case 'success': return '✅';
      case 'warning': return '⚠️';
      default: return 'ℹ️';
    }
  }

  getWashHistory(): Observable<WashHistory[]> {
    const userId = this.authService.user()?.id;
    
    if (!userId) {
      return of(this.washes());
    }

    return this.http.get<ApiResponse<WashHistory[]>>(`${environment.apiUrl}/customer/washes/${userId}`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            const washes: WashHistory[] = response.data.map((w: any) => ({
              id: w.id,
              merchantName: w.merchantName,
              merchantLogo: w.merchantLogo,
              date: w.washDate?.split('T')[0] || w.washDate,
              time: new Date(w.washDate).toLocaleTimeString('ar-SA', { hour: '2-digit', minute: '2-digit' }),
              amount: w.amount,
              status: w.status,
              rating: w.rating
            }));
            this.washes.set(washes);
            return washes;
          }
          throw new Error(response.message || 'Failed to load wash history');
        }),
        catchError(error => {
          this.toast.showError('فشل في تحميل سجل الغسلات');
          return of(this.washes());
        })
      );
  }

  getNotifications(): Observable<Notification[]> {
    const userId = this.authService.user()?.id;
    
    if (!userId) {
      return of(this.notifications());
    }

    return this.http.get<ApiResponse<Notification[]>>(`${environment.apiUrl}/customer/notifications/${userId}`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            const notifications: Notification[] = response.data.map((n: any) => ({
              id: n.id,
              title: n.title,
              message: n.message,
              type: n.type,
              date: n.createdAt?.split('T')[0] || n.createdAt,
              read: n.isRead,
              icon: this.getNotificationIcon(n.type)
            }));
            this.notifications.set(notifications);
            return notifications;
          }
          throw new Error(response.message || 'Failed to load notifications');
        }),
        catchError(error => {
          this.toast.showError('فشل في تحميل الإشعارات');
          return of(this.notifications());
        })
      );
  }

  getRewards(): Observable<Reward[]> {
    const userId = this.authService.user()?.id;
    
    if (!userId) {
      return of(this.rewards());
    }

    return this.http.get<ApiResponse<Reward[]>>(`${environment.apiUrl}/customer/rewards/${userId}`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            const rewards: Reward[] = response.data.map((r: any) => ({
              id: r.id,
              title: r.title,
              description: r.description,
              type: r.type,
              value: r.value,
              expiresAt: r.expiresAt?.split('T')[0] || r.expiresAt,
              status: r.status
            }));
            this.rewards.set(rewards);
            return rewards;
          }
          throw new Error(response.message || 'Failed to load rewards');
        }),
        catchError(error => {
          this.toast.showError('فشل في تحميل المكافآت');
          return of(this.rewards());
        })
      );
  }

  /**
   * Get rewards for a specific user ID
   * Backend route: GET /api/customer/rewards/{userId}
   */
  getRewardsById(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/rewards/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading rewards:', error);
        return of({ success: false, data: [] });
      })
    );
  }

  purchasePass(passData: any): Observable<any> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      this.toast.showError('يجب تسجيل الدخول أولاً');
      return of(null);
    }

    return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/customer/purchase-pass`, {
      ...passData,
      userId
    })
      .pipe(
        map(response => {
          if (response.success) {
            this.toast.showSuccess('تم شراء الباقة بنجاح');
            return response.data;
          }
          throw new Error(response.message || 'Failed to purchase pass');
        }),
        catchError(error => {
          this.toast.showError(error.error?.message || 'فشل في شراء الباقة');
          return of(null);
        })
      );
  }

  markNotificationAsRead(notificationId: string): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}/customer/notification/${notificationId}/read`, {})
      .pipe(
        map(response => {
          if (response.success) {
            this.toast.showSuccess('تم تحديث الإشعار');
            this.notifications.update(notifs =>
              notifs.map(n => n.id === notificationId ? { ...n, read: true } : n)
            );
            return response.data;
          }
          throw new Error(response.message || 'Failed to mark notification as read');
        }),
        catchError(error => {
          this.toast.showError(error.error?.message || 'فشل في تحديث الإشعار');
          return of(null);
        })
      );
  }

  claimReward(rewardId: string): Observable<any> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      this.toast.showError('يجب تسجيل الدخول أولاً');
      return of(null);
    }

    return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/customer/rewards/${rewardId}/claim`, { userId })
      .pipe(
        map(response => {
          if (response.success) {
            this.toast.showSuccess('تم استلام المكافأة بنجاح');
            this.rewards.update(rewards =>
              rewards.map(r => r.id === rewardId ? { ...r, status: 'claimed' } : r)
            );
            return response.data;
          }
          throw new Error(response.message || 'Failed to claim reward');
        }),
        catchError(error => {
          this.toast.showError(error.error?.message || 'فشل في استلام المكافأة');
          return of(null);
        })
      );
  }

  updateProfile(profileData: any): Observable<any> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      this.toast.showError('يجب تسجيل الدخول أولاً');
      return of(null);
    }

    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}/customer/profile`, {
      ...profileData,
      userId
    })
      .pipe(
        map(response => {
          if (response.success) {
            this.toast.showSuccess('تم تحديث الملف الشخصي بنجاح');
            this.getCustomerProfile().subscribe();
            return response.data;
          }
          throw new Error(response.message || 'Failed to update profile');
        }),
        catchError(error => {
          this.toast.showError(error.error?.message || 'فشل في تحديث الملف الشخصي');
          return of(null);
        })
      );
  }

  addFunds(amount: number, paymentMethod: string = 'wallet'): Observable<any> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      this.toast.showError('يجب تسجيل الدخول أولاً');
      return of(null);
    }

    return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/customer/wallet/add-funds`, {
      userId,
      amount,
      paymentMethod
    })
      .pipe(
        map(response => {
          if (response.success) {
            this.toast.showSuccess(`تم إضافة ${amount} ريال إلى المحفظة`);
            // Refresh profile to get updated balance
            this.getCustomerProfile().subscribe();
            return response.data;
          }
          throw new Error(response.message || 'Failed to add funds');
        }),
        catchError(error => {
          this.toast.showError(error.error?.message || 'فشل في إضافة الرصيد');
          return of(null);
        })
      );
  }

  // Download QR Code
  downloadQRCode(): Observable<Blob> {
    const userId = this.authService.user()?.id || '';
    return this.http.get(
      `${environment.apiUrl}/customer/${userId}/qr-code`,
      { responseType: 'blob' }
    ).pipe(
      catchError(error => {
        this.toast.showError('فشل في تحميل رمز QR');
        throw error;
      })
    ) as Observable<Blob>;
  }

  // ✅ Car Photos Methods
  uploadCarPhoto(photo: File): Observable<any> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      this.toast.showError('يجب تسجيل الدخول أولاً');
      return of(null);
    }

    const formData = new FormData();
    formData.append('photo', photo);

    return this.http.post<ApiResponse<any>>(
      `${environment.apiUrl}/customer/car-photos/${userId}`,
      formData
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم رفع صورة السيارة بنجاح');
          return response.data;
        }
        throw new Error(response.message || 'Failed to upload photo');
      }),
      catchError(error => {
        this.toast.showError(error.error?.message || 'فشل في رفع الصورة');
        return of(null);
      })
    );
  }

  getCarPhotos(): Observable<any[]> {
    const userId = this.authService.user()?.id;
    if (!userId) {
      return of([]);
    }

    return this.http.get<ApiResponse<any[]>>(
      `${environment.apiUrl}/customer/car-photos/${userId}`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        return [];
      }),
      catchError(error => {
        console.error('Error loading car photos:', error);
        return of([]);
      })
    );
  }

  deleteCarPhoto(photoId: string): Observable<any> {
    return this.http.delete<ApiResponse<any>>(
      `${environment.apiUrl}/customer/car-photo/${photoId}`
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم حذف الصورة بنجاح');
          return response.data;
        }
        throw new Error(response.message || 'Failed to delete photo');
      }),
      catchError(error => {
        this.toast.showError(error.error?.message || 'فشل في حذف الصورة');
        return of(null);
      })
    );
  }

  // Demo Methods
  addDemoWash(wash: WashHistory): void {
    this.washes.update(washes => [wash, ...washes]);
  }

  addDemoNotification(notification: Notification): void {
    this.notifications.update(notifs => [notification, ...notifs]);
  }

  // ================================================
  // ⭐ **الخدمات والاشتراكات الجديدة**
  // ================================================

  /**
   * الحصول على جميع المغسلات المتاحة
   */
  getAllMerchants(): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(
      `${environment.apiUrl}/merchant/all`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        return [];
      }),
      catchError(error => {
        console.error('Error loading merchants:', error);
        return of([]);
      })
    );
  }

  /**
   * الحصول على خدمات مغسلة معينة
   */
  getMerchantServices(merchantId: string): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(
      `${environment.apiUrl}/services/merchant/${merchantId}`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        return [];
      }),
      catchError(error => {
        console.error('Error loading services:', error);
        return of([]);
      })
    );
  }

  /**
   * الحصول على اشتراكات العميل الحالية
   */
  getMySubscriptions(): Observable<any[]> {
    const userId = this.authService.user()?.id || '';
    return this.http.get<ApiResponse<any[]>>(
      `${environment.apiUrl}/subscriptions/customer-by-user/${userId}`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        return [];
      }),
      catchError(error => {
        console.error('Error loading subscriptions:', error);
        return of([]);
      })
    );
  }

  /**
   * الاشتراك في مغسلة
   */
  subscribeToMerchant(merchantId: string, planType: string = 'free'): Observable<any> {
    return this.http.post<ApiResponse<any>>(
      `${environment.apiUrl}/subscriptions/subscribe`,
      { merchantId, planType }
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم الاشتراك بنجاح!');
          return response.data;
        }
        throw new Error(response.message || 'Failed to subscribe');
      }),
      catchError(error => {
        this.toast.showError(error.error?.message || 'فشل الاشتراك');
        return of(null);
      })
    );
  }

  /**
   * إلغاء الاشتراك
   */
  cancelSubscription(subscriptionId: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(
      `${environment.apiUrl}/subscriptions/cancel/${subscriptionId}`,
      {}
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم إلغاء الاشتراك بنجاح');
          return response.data;
        }
        throw new Error(response.message || 'Failed to cancel');
      }),
      catchError(error => {
        this.toast.showError(error.error?.message || 'فشل إلغاء الاشتراك');
        return of(null);
      })
    );
  }

  /**
   * الحصول على رصيد المحفظة
   */
  getWalletBalance(): Observable<any> {
    const userId = this.authService.user()?.id || '';
    return this.http.get<any>(
      `${environment.apiUrl}/customer/${userId}/wallet`
    ).pipe(
      catchError(error => {
        console.error('Error loading wallet balance:', error);
        return of({ success: false, data: { balance: 0 } });
      })
    );
  }
  
  /**
   * الحصول على بطاقات الولاء للعميل
   * Backend expects userId for this endpoint: GET /api/customer/loyalty-cards/{userId}
   */
  getLoyaltyCards(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/loyalty-cards/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading loyalty cards:', error);
        return of({ success: false, data: [] });
      })
    );
  }
  
  /**
   * الحصول على رمز QR الخاص بالعميل
   */
  getCustomerQRCode(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/qrcode/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading QR code:', error);
        return of({ success: false, data: null });
      })
    );
  }
  
  /**
   * الحصول على صورة رمز QR الخاص بالعميل
   */
  getCustomerQRCodeImage(userId: string | number): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/qrcode/${userId}/image`,
      { responseType: 'blob' }
    ).pipe(
      catchError(error => {
        console.error('Error loading QR code image:', error);
        return of(new Blob());
      })
    );
  }
  
  /**
   * الحصول على سجل الغسلات مع userId
   * Backend route: GET /api/customer/washes/{userId}
   */
  getWashHistoryById(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/washes/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading wash history:', error);
        return of({ success: false, data: [] });
      })
    );
  }
  
  /**
   * الحصول على صور السيارة مع userId
   */
  getCarPhotosById(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/car-photos/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading car photos:', error);
        return of({ success: false, data: [] });
      })
    );
  }
  
  /**
   * رفع صورة السيارة مع userId
   */
  uploadCarPhotoById(userId: string | number, photo: File): Observable<any> {
    const formData = new FormData();
    formData.append('photo', photo);
    
    return this.http.post<any>(
      `${environment.apiUrl}/customer/car-photos/${userId}`,
      formData
    ).pipe(
      catchError(error => {
        console.error('Error uploading car photo:', error);
        return of({ success: false });
      })
    );
  }
  
  /**
   * الحصول على الملف الشخصي للعميل بواسطة userId
   * Backend route: GET /api/customer/profile/{userId}
   */
  getProfileById(userId: string | number): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/profile/${userId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading customer profile:', error);
        return of({ success: false, data: null });
      })
    );
  }

  /**
   * تقييم غسلة معينة
   */
  rateWash(washId: string, rating: number, comments?: string): Observable<any> {
    return this.http.post<any>(
      `${environment.apiUrl}/customer/wash/${washId}/rate`,
      { rating, comments: comments || '' }
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم تقييم الغسلة بنجاح');
          return response.data;
        }
        throw new Error(response.message || 'Failed to rate wash');
      }),
      catchError(error => {
        this.toast.showError(error.error?.message || 'فشل في تقييم الغسلة');
        return of(null);
      })
    );
  }

  /**
   * الحصول على تفاصيل غسلة معينة
   */
  getWashDetails(washId: string): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/customer/wash/${washId}`
    ).pipe(
      catchError(error => {
        console.error('Error loading wash details:', error);
        return of({ success: false, data: null });
      })
    );
  }
}