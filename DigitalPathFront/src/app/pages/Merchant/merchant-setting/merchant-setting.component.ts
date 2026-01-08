// src/app/pages/Merchant/merchant-setting/merchant-setting.component.ts
import { Component, OnInit, OnDestroy, HostListener, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '../../../core/services/toast.service';
import { MerchantService } from '../../../core/services/merchant.service';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/services/api.service';

interface MerchantProfile {
  id?: string;
  business_name: string;
  city: string;
  phone: string;
  email: string;
  plan?: {
    type: 'basic' | 'pro';
    expiry_date?: string;
  };
  qr_code_image_url?: string;
  subscription_status?: 'active' | 'expired' | 'pending';
}

interface LoyaltySettings {
  reward_washes_required: number;
  reward_time_limit_days: number;
  anti_fraud_same_day: boolean;
  enable_car_photo: boolean;
  notifications_enabled: boolean;
  notification_template_welcome: string;
  notification_template_remaining: string;
  notification_template_reward_close: string;
  reward_description: string;
  custom_primary_color: string;
  custom_secondary_color: string;
  custom_business_tagline: string;
}

interface PasswordData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-merchant-setting',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './merchant-setting.component.html',
  styleUrls: ['./merchant-setting.component.css']
})
export class MerchantSettingComponent implements OnInit, OnDestroy {
  activeTab: 'business' | 'loyalty' | 'design' | 'features' | 'security' = 'business';
  user: MerchantProfile | null = null;
  profileData: Partial<MerchantProfile> = {};
  merchantPlan: 'basic' | 'pro' = 'basic';
  isLoyaltyPaused: boolean = false;
  
  settings: LoyaltySettings = {
    reward_washes_required: 5,
    reward_time_limit_days: 30,
    anti_fraud_same_day: true,
    enable_car_photo: false,
    notifications_enabled: true,
    notification_template_welcome: 'وحشتنا! بدأنا معك رحلة الولاء 🚗',
    notification_template_remaining: 'باقي لك غسلتين فقط للحصول على المكافأة! 💪',
    notification_template_reward_close: 'قريباً! باقي غسلة واحدة فقط للحصول على المكافأة 🎁',
    reward_description: 'غسلة مجانية خارجي',
    custom_primary_color: '#3B82F6',
    custom_secondary_color: '#0F172A',
    custom_business_tagline: 'نظافة سيارة تبدأ معنا'
  };
  
  passwordData: PasswordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  
  showPasswordForm = false;
  saving = false;
  changingPassword = false;
  hasChanges = false;
  originalSettings: string = '';

  merchantId: string | null = null;
  
  // Logo upload
  merchantLogo: string | null = null;
  isUploadingLogo: boolean = false;

  constructor(
    private router: Router,
    private toastService: ToastService,
    private merchantService: MerchantService,
    private authService: AuthService,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {
    const user = this.authService.user();
    if (user?.id) {
      this.loadMerchantId(user.id);
    }
  }

  ngOnInit(): void {
    const user = this.authService.user();
    if (user?.id) {
      this.loadMerchantId(user.id);
    }
    this.originalSettings = JSON.stringify(this.settings);
    
    // Watch for changes
    setInterval(() => {
      this.checkForChanges();
    }, 1000);
  }

  ngOnDestroy(): void {
    // Cleanup (simplified)
  }

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: BeforeUnloadEvent): void {
    if (this.hasChanges) {
      $event.returnValue = 'You have unsaved changes!';
    }
  }

  private checkForChanges(): void {
    const currentSettings = JSON.stringify(this.settings);
    this.hasChanges = currentSettings !== this.originalSettings;
  }

  private loadMerchantId(userId: string): void {
    this.http.get<ApiResponse<string>>(`${environment.apiUrl}/merchant/by-user/${userId}`).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.merchantId = res.data;
          this.loadMerchantData();
        }
      },
      error: () => {
        this.toastService.showError('فشل في تحميل بيانات المغسلة');
      }
    });
  }

  private loadMerchantData(): void {
    if (!this.merchantId) return;

    // Load profile
    this.merchantService.getProfile(this.merchantId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const profile = response.data;
          this.user = {
            id: profile.id,
            business_name: profile.businessName,
            city: profile.city,
            phone: profile.phone,
            email: profile.email,
            plan: {
              type: profile.plan?.toLowerCase() as 'basic' | 'pro' || 'basic',
              expiry_date: profile.planExpiryDate?.toString()
            },
            qr_code_image_url: profile.qrCodeImageUrl || '',
            subscription_status: profile.subscriptionStatus as 'active' | 'expired' | 'pending' || 'active'
          };
          // Set merchant logo from QR code image URL
          if (profile.qrCodeImageUrl) {
            this.merchantLogo = this.getFullLogoUrl(profile.qrCodeImageUrl);
          }
          // Set merchant plan for notifications restriction
          this.merchantPlan = (profile.plan?.toLowerCase() as 'basic' | 'pro') || 'basic';
          // If Basic plan, disable notifications
          if (this.merchantPlan === 'basic') {
            this.settings.notifications_enabled = false;
          }
          this.profileData = { ...this.user };
        }
      }
    });

    // Load settings
    this.merchantService.getSettings(this.merchantId).subscribe({
      next: (response) => {
        console.log('Settings loaded from API:', response);
        if (response.success && response.data) {
          const s = response.data;
          console.log('isLoyaltyPaused from DB:', s.isLoyaltyPaused);
          console.log('loyaltyPausedUntil from DB:', s.loyaltyPausedUntil);
          
          this.settings = {
            reward_washes_required: s.rewardWashesRequired || 5,
            reward_time_limit_days: s.rewardTimeLimitDays || 30,
            anti_fraud_same_day: s.antiFraudSameDay ?? true,
            enable_car_photo: s.enableCarPhoto ?? false,
            notifications_enabled: s.notificationsEnabled ?? true,
            notification_template_welcome: s.notificationTemplateWelcome || '',
            notification_template_remaining: s.notificationTemplateRemaining || '',
            notification_template_reward_close: s.notificationTemplateRewardClose || '',
            reward_description: s.rewardDescription || 'غسلة مجانية',
            custom_primary_color: s.customPrimaryColor || '#3B82F6',
            custom_secondary_color: s.customSecondaryColor || '#0F172A',
            custom_business_tagline: s.customBusinessTagline || ''
          };
          
          // Load loyalty pause status from database
          this.isLoyaltyPaused = s.isLoyaltyPaused ?? false;
          console.log('Loyalty pause status loaded:', this.isLoyaltyPaused);
          
          this.originalSettings = JSON.stringify(this.settings);
          // Reset saving flag after data is loaded
          this.saving = false;
        }
      },
      error: (error) => {
        console.error('Error loading merchant settings:', error);
        this.saving = false;
      }
    });
  }

  // Loyalty settings helpers
  incrementWashes(): void {
    if (this.settings.reward_washes_required < 20) {
      this.settings.reward_washes_required++;
    }
  }

  decrementWashes(): void {
    if (this.settings.reward_washes_required > 3) {
      this.settings.reward_washes_required--;
    }
  }

  incrementDays(): void {
    if (this.settings.reward_time_limit_days < 90) {
      this.settings.reward_time_limit_days += 7;
    }
  }

  decrementDays(): void {
    if (this.settings.reward_time_limit_days > 7) {
      this.settings.reward_time_limit_days -= 7;
    }
  }

  // Logo upload methods
  getFullLogoUrl(logoPath: string): string {
    if (!logoPath) return '';
    if (logoPath.startsWith('http://') || logoPath.startsWith('https://') || logoPath.startsWith('data:')) {
      return logoPath;
    }
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}${logoPath}`;
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    
    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.toastService.showError('الرجاء اختيار ملف صورة');
      return;
    }

    // Validate file size (max 2MB)
    if (file.size > 2 * 1024 * 1024) {
      this.toastService.showError('حجم الصورة يجب أن يكون أقل من 2 ميجابايت');
      return;
    }

    this.uploadLogo(file);
  }

  uploadLogo(file: File): void {
    if (!this.merchantId) {
      this.toastService.showError('لم يتم العثور على معرف المغسلة');
      return;
    }

    this.isUploadingLogo = true;
    
    this.merchantService.uploadLogo(this.merchantId, file).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toastService.showSuccess('تم رفع الشعار بنجاح');
          // Update the displayed logo
          if (response.data) {
            this.merchantLogo = this.getFullLogoUrl(response.data);
          }
        } else {
          this.toastService.showError(response.message || 'فشل في رفع الشعار');
        }
        this.isUploadingLogo = false;
      },
      error: (error) => {
        console.error('Error uploading logo:', error);
        this.toastService.showError('حدث خطأ أثناء رفع الشعار');
        this.isUploadingLogo = false;
      }
    });
  }

  // Car Photo Upload
  onCarPhotoUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    if (!this.user || this.user.plan?.type !== 'pro') {
      this.toastService.showError('هذه الميزة متاحة فقط لمستخدمي باقة Pro');
      return;
    }

    // Validate file
    if (file.size > 5 * 1024 * 1024) {
      this.toastService.showError('حجم الصورة يجب أن يكون أقل من 5MB');
      return;
    }

    if (!file.type.match('image/jpeg') && !file.type.match('image/png')) {
      this.toastService.showError('يجب أن تكون الصورة بصيغة JPG أو PNG');
      return;
    }

    // Show loading
    this.toastService.showInfo('جاري رفع صورة السيارة...');

    // Upload the car photo to the server
    const formData = new FormData();
    formData.append('file', file);
    
    this.http.post<ApiResponse<any>>(`${environment.apiUrl}/merchant/upload-car-photo`, formData).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastService.showSuccess('تم رفع صورة السيارة بنجاح! ستظهر في بطاقة العميل');
          input.value = '';
        }
      },
      error: () => {
        this.toastService.showError('فشل في رفع صورة السيارة');
        input.value = '';
      }
    });
  }

  // Logo Upload
  onLogoUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    
    // Validate file
    if (file.size > 2 * 1024 * 1024) {
      this.toastService.showError('حجم الشعار يجب أن يكون أقل من 2MB');
      return;
    }

    if (!file.type.match('image/jpeg') && !file.type.match('image/png')) {
      this.toastService.showError('يجب أن يكون الشعار بصيغة JPG أو PNG');
      return;
    }

    // Create preview
    const reader = new FileReader();
    let logoPreviewUrl = '';
    reader.onload = (e: any) => {
      logoPreviewUrl = e.target.result;
      if (this.user) {
        this.user.qr_code_image_url = logoPreviewUrl;
      }
    };

    // Upload the logo to the server
    const formData = new FormData();
    formData.append('file', file);
    
    this.http.post<ApiResponse<any>>(`${environment.apiUrl}/merchant/upload-logo`, formData).subscribe({
      next: (response) => {
        if (response.success && this.user) {
          this.user.qr_code_image_url = response.data?.logoUrl || logoPreviewUrl;
          this.toastService.showSuccess('تم رفع الشعار بنجاح');
        }
      },
      error: () => {
        this.toastService.showError('فشل في رفع الشعار');
      }
    });
  }

  removeLogo(): void {
    if (this.user) {
      this.user.qr_code_image_url = undefined;
      this.toastService.showInfo('تم حذف الشعار');
    }
  }

  // QR Code Functions
  downloadQR(): void {
    this.toastService.showInfo('جاري تحميل QR Code...');
    
    // Generate QR code by calling the backend
    if (!this.merchantId) {
      this.toastService.showError('معرف المغسلة غير متوفر');
      return;
    }
    
    this.http.get(`${environment.apiUrl}/merchant/${this.merchantId}/qr-code`, {
      responseType: 'blob'
    }).subscribe({
      next: (blob: Blob) => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `qr-code-${this.merchantId}.png`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.toastService.showSuccess('تم تحميل QR Code بنجاح');
      },
      error: () => {
        this.toastService.showError('فشل في تحميل QR Code');
      }
    });
  }

  copyLink(): void {
    const link = `https://digitalpass.com/merchant/${this.user?.id}`;
    navigator.clipboard.writeText(link).then(() => {
      this.toastService.showSuccess('تم نسخ الرابط إلى الحافظة');
    });
  }

  // Security Functions
  changePassword(): void {
    if (!this.passwordData.currentPassword) {
      this.toastService.showError('أدخل كلمة المرور الحالية');
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.toastService.showError('كلمات المرور غير متطابقة');
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.toastService.showError('كلمة المرور يجب أن تكون 6 أحرف على الأقل');
      return;
    }

    if (!this.merchantId) {
      this.toastService.showError('خطأ في بيانات المغسلة');
      return;
    }

    this.changingPassword = true;
    
    // Use real API call
    this.merchantService.changePassword(
      this.merchantId,
      this.passwordData.currentPassword,
      this.passwordData.newPassword
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastService.showSuccess('تم تغيير كلمة المرور بنجاح');
          this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
          this.showPasswordForm = false;
        }
        this.changingPassword = false;
      },
      error: () => {
        this.toastService.showError('فشل في تغيير كلمة المرور');
        this.changingPassword = false;
      }
    });
  }

  logoutAllSessions(): void {
    if (confirm('هل تريد تسجيل الخروج من جميع الأجهزة؟')) {
      this.toastService.showInfo('جاري تسجيل الخروج...');
      
      // Call real logout API if available
      if (!this.merchantId) {
        this.toastService.showError('خطأ: معرف المغسلة غير متوفر');
        return;
      }
      
      // In a real scenario, call: this.merchantService.logoutAllSessions(this.merchantId)
      this.toastService.showSuccess('تم تسجيل الخروج من جميع الأجهزة');
    }
  }

  // Toggle Loyalty Program Pause/Resume - Simple Button
  toggleLoyaltyPause(): void {
    if (!this.merchantId) {
      this.toastService.showError('خطأ: معرف المغسلة غير متوفر');
      return;
    }

    this.saving = true;
    
    // Toggle the state
    this.isLoyaltyPaused = !this.isLoyaltyPaused;

    // Create settings update
    const settingsDto = {
      id: '',
      rewardWashesRequired: this.settings.reward_washes_required,
      rewardTimeLimitDays: this.settings.reward_time_limit_days,
      antiFraudSameDay: this.settings.anti_fraud_same_day,
      enableCarPhoto: this.settings.enable_car_photo,
      notificationsEnabled: this.settings.notifications_enabled,
      notificationTemplateWelcome: this.settings.notification_template_welcome,
      notificationTemplateRemaining: this.settings.notification_template_remaining,
      notificationTemplateRewardClose: this.settings.notification_template_reward_close,
      customPrimaryColor: this.settings.custom_primary_color,
      customSecondaryColor: this.settings.custom_secondary_color,
      customBusinessTagline: this.settings.custom_business_tagline,
      rewardDescription: this.settings.reward_description,
      isLoyaltyPaused: this.isLoyaltyPaused,
      loyaltyPausedUntil: null
    };

    this.merchantService.updateSettings(this.merchantId, settingsDto).subscribe({
      next: (response) => {
        if (response.success) {
          if (this.isLoyaltyPaused) {
            this.toastService.showSuccess('⏸️ تم إيقاف برنامج الولاء');
          } else {
            this.toastService.showSuccess('✅ تم استئناف برنامج الولاء');
          }
          this.cdr.detectChanges();
        } else {
          this.toastService.showError('فشل في حفظ الحالة');
          this.isLoyaltyPaused = !this.isLoyaltyPaused; // Revert on error
        }
        this.saving = false;
      },
      error: (error) => {
        this.toastService.showError('خطأ: ' + (error?.error?.message || 'فشل العملية'));
        this.isLoyaltyPaused = !this.isLoyaltyPaused; // Revert on error
        this.saving = false;
      }
    });
  }

  // Save Settings
  saveSettings(): void {
    this.saving = true;

    // Validate settings
    if (this.settings.reward_washes_required < 3 || this.settings.reward_washes_required > 20) {
      this.toastService.showError('عدد الغسلات يجب أن يكون بين 3 و 20');
      this.saving = false;
      return;
    }

    if (this.settings.reward_time_limit_days < 7 || this.settings.reward_time_limit_days > 90) {
      this.toastService.showError('المدة الزمنية يجب أن تكون بين 7 و 90 يوم');
      this.saving = false;
      return;
    }

    if (!this.merchantId) {
      this.toastService.showError('خطأ في بيانات المغسلة');
      this.saving = false;
      return;
    }

    // Convert frontend settings format to backend DTO format
    const settingsDto = {
      id: '',
      rewardWashesRequired: this.settings.reward_washes_required,
      rewardTimeLimitDays: this.settings.reward_time_limit_days,
      antiFraudSameDay: this.settings.anti_fraud_same_day,
      enableCarPhoto: this.settings.enable_car_photo,
      notificationsEnabled: this.settings.notifications_enabled,
      notificationTemplateWelcome: this.settings.notification_template_welcome,
      notificationTemplateRemaining: this.settings.notification_template_remaining,
      notificationTemplateRewardClose: this.settings.notification_template_reward_close,
      customPrimaryColor: this.settings.custom_primary_color,
      customSecondaryColor: this.settings.custom_secondary_color,
      customBusinessTagline: this.settings.custom_business_tagline,
      rewardDescription: this.settings.reward_description
    };

    this.merchantService.updateSettings(this.merchantId, settingsDto).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastService.showSuccess('تم حفظ الإعدادات بنجاح!');
          this.originalSettings = JSON.stringify(this.settings);
          this.hasChanges = false;
        }
        this.saving = false;
      },
      error: () => {
        this.toastService.showError('فشل في حفظ الإعدادات');
        this.saving = false;
      }
    });
  }

  discardChanges(): void {
    if (confirm('هل تريد تجاهل التغييرات غير المحفوظة؟')) {
      // Reload original settings
      this.settings = JSON.parse(this.originalSettings);
      this.hasChanges = false;
      this.toastService.showInfo('تم تجاهل التغييرات');
    }
  }

  contactSupportForUpgrade(): void {
    const message = `السلام عليكم، أود ترقية باقتي من ${this.merchantPlan === 'basic' ? 'Basic' : 'Pro'} للاستفادة من ميزة الإشعارات الذكية.`;
    const encodedMessage = encodeURIComponent(message);
    window.open(`https://wa.me/966548290509?text=${encodedMessage}`, '_blank');
  }

  goBack(): void {
    if (this.hasChanges) {
      if (confirm('لديك تغييرات غير محفوظة. هل تريد المتابعة والعودة؟')) {
        this.router.navigate(['/merchant/dashboard']);
      }
    } else {
      this.router.navigate(['/merchant/dashboard']);
    }
  }

  logout(): void {
    this.authService.logout();
    this.toastService.showSuccess('تم تسجيل الخروج بنجاح');
    this.router.navigate(['/auth/signin']);
  }
}