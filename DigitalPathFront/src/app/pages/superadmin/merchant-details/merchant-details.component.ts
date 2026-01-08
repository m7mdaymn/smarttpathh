import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuperAdminService } from '../../../core/services/super-admin.service';
import { ToastService } from '../../../core/services/toast.service';

interface Merchant {
  id: string;
  businessName: string;
  ownerName: string;
  email: string;
  phone: string;
  city: string;
  plan: 'basic' | 'pro';
  customers: number;
  customersGrowth: number;
  totalWashes: number;
  joinDate: string;
  status: 'active' | 'inactive' | 'pending' | 'awaiting_approval' | 'suspended';
  subscriptionStartDate: string;
  subscriptionEndDate: string;
  isBlocked?: boolean;
  registrationCode?: string;
  qrCodeImageUrl?: string;
}

interface Statistics {
  totalBusinesses: number;
  activeBusinesses: number;
  inactiveBusinesses: number;
  totalCustomers: number;
  customerGrowth: number;
  totalWashes: number;
  last30DaysWashes: number;
  avgWashesPerDay: number;
  totalRewards: number;
  redeemedRewards: number;
  basicPlanCount: number;
  proPlanCount: number;
  basicAvgCustomers: number;
  basicAvgWashes: number;
  proAvgCustomers: number;
  proAvgWashes: number;
  activeBusinessesGrowth: number;
  avgWashesPerBusiness: number;
  avgWashesGrowth: number;
}

interface PlatformSettings {
  name: string;
  supportEmail: string;
  supportPhone: string;
  basicPlanPrice: number;
  proPlanPrice: number;
  trialPeriod: number;
  emailNotifications: boolean;
  smsNotifications: boolean;
  renewalReminders: boolean;
  maintenanceMode: boolean;
  maintenanceMessage: string;
}

@Component({
  selector: 'app-merchant-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './merchant-details.component.html',
  styleUrls: ['./merchant-details.component.css']
})
export class MerchantDetailsComponent implements OnInit {
  activeTab: 'merchants' | 'statistics' | 'platform' = 'merchants';
  searchTerm = '';
  
  // Signals for state management
  merchants = signal<Merchant[]>([]);
  selectedMerchant = signal<Merchant | null>(null);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isEditModalOpen = signal<boolean>(false);
  isAddModalOpen = signal<boolean>(false);
  isActivateModalOpen = signal<boolean>(false);
  activationMonths = signal<number>(1);
  merchantToActivate = signal<string | null>(null);
  editFormData = signal<any>({
    businessName: '',
    ownerName: '',
    city: '',
    plan: 'basic'
  });
  addFormData = signal<any>({
    businessName: '',
    ownerName: '',
    email: '',
    city: '',
    plan: 'basic'
  });
  
  // Computed values
  filteredMerchants = computed(() => {
    const term = this.searchTerm.toLowerCase();
    if (!term) return this.merchants();
    return this.merchants().filter(m =>
      m.businessName.toLowerCase().includes(term) ||
      m.ownerName.toLowerCase().includes(term) ||
      m.email.toLowerCase().includes(term) ||
      m.city.toLowerCase().includes(term)
    );
  });
  
  activeMerchantsCount = computed(() => 
    this.merchants().filter(m => m.status === 'active').length
  );
  
  // Legacy properties for compatibility
  merchant: Merchant | null = null;
  merchantsData: Merchant[] = [];
  
  statistics: Statistics = {
    totalBusinesses: 68,
    activeBusinesses: 52,
    inactiveBusinesses: 12,
    totalCustomers: 1245,
    customerGrowth: 15.3,
    totalWashes: 5234,
    last30DaysWashes: 425,
    avgWashesPerDay: 14.2,
    totalRewards: 312,
    redeemedRewards: 189,
    basicPlanCount: 42,
    proPlanCount: 26,
    basicAvgCustomers: 18,
    basicAvgWashes: 95,
    proAvgCustomers: 32,
    proAvgWashes: 165,
    activeBusinessesGrowth: 8.2,
    avgWashesPerBusiness: 77,
    avgWashesGrowth: 12.5
  };
  
  platformSettings: PlatformSettings = {
    name: 'Digital Pass',
    supportEmail: 'support@digitalpass.com',
    supportPhone: '0548290509',
    basicPlanPrice: 99,
    proPlanPrice: 149,
    trialPeriod: 7,
    emailNotifications: true,
    smsNotifications: false,
    renewalReminders: true,
    maintenanceMode: false,
    maintenanceMessage: 'نظام الصيانة قيد التطوير حالياً، سوف نعود قريباً.'
  };

  constructor(
    private superAdminService: SuperAdminService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadMerchants();
  }

  loadMerchants(): void {
    this.isLoading.set(true);
    this.superAdminService.getAllMerchants().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const merchants: Merchant[] = response.data.map((m: any) => ({
            id: m.id,
            businessName: m.businessName,
            ownerName: m.ownerName,
            email: m.email,
            phone: m.phone || '',
            city: m.city,
            plan: m.plan?.toLowerCase() === 'pro' ? 'pro' : 'basic',
            customers: m.customers || m.totalCustomers || 0,
            customersGrowth: m.customersGrowth || 0,
            totalWashes: m.totalWashes || 0,
            joinDate: m.joinDate,
            status: m.status || 'active',
            subscriptionStartDate: m.subscriptionStartDate || m.joinDate || '',
            subscriptionEndDate: m.subscriptionEndDate || '',
            isBlocked: m.isBlocked || false,
            registrationCode: m.registrationCode || '',
            qrCodeImageUrl: m.qrCodeImageUrl || ''
          } as Merchant));
          this.merchants.set(merchants);
          this.merchantsData = merchants; // Keep for compatibility
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toast.showError('فشل في تحميل بيانات المتاجر');
        this.isLoading.set(false);
      }
    });
  }

  setActiveTab(tab: 'merchants' | 'statistics' | 'platform'): void {
    this.activeTab = tab;
    if (tab === 'statistics') {
      this.loadStatistics();
    } else if (tab === 'platform') {
      this.loadPlatformSettings();
    }
  }

  loadStatistics(): void {
    this.superAdminService.getStatistics().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const stats = response.data;
          this.statistics = {
            totalBusinesses: stats.totalBusinesses || 0,
            activeBusinesses: stats.activeBusinesses || 0,
            inactiveBusinesses: stats.inactiveBusinesses || 0,
            totalCustomers: stats.totalCustomers || 0,
            customerGrowth: stats.customerGrowth || 0,
            totalWashes: stats.totalWashes || 0,
            last30DaysWashes: stats.last30DaysWashes || 0,
            avgWashesPerDay: stats.avgWashesPerDay || 0,
            totalRewards: stats.totalRewards || 0,
            redeemedRewards: stats.redeemedRewards || 0,
            basicPlanCount: stats.basicPlanCount || 0,
            proPlanCount: stats.proPlanCount || 0,
            basicAvgCustomers: stats.basicAvgCustomers || 0,
            basicAvgWashes: stats.basicAvgWashes || 0,
            proAvgCustomers: stats.proAvgCustomers || 0,
            proAvgWashes: stats.proAvgWashes || 0,
            activeBusinessesGrowth: stats.activeBusinessesGrowth || 0,
            avgWashesPerBusiness: stats.avgWashesPerBusiness || 0,
            avgWashesGrowth: stats.avgWashesGrowth || 0
          };
        }
      }
    });
  }

  loadPlatformSettings(): void {
    this.superAdminService.getPlatformSettings().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const settings = response.data;
          this.platformSettings = {
            name: settings.name || 'Digital Pass',
            supportEmail: settings.supportEmail || '',
            supportPhone: settings.supportPhone || '',
            basicPlanPrice: settings.basicPlanPrice || 99,
            proPlanPrice: settings.proPlanPrice || 149,
            trialPeriod: settings.trialPeriod || 7,
            emailNotifications: settings.emailNotifications || false,
            smsNotifications: settings.smsNotifications || false,
            renewalReminders: settings.renewalReminders || false,
            maintenanceMode: settings.maintenanceMode || false,
            maintenanceMessage: settings.maintenanceMessage || ''
          };
        }
      }
    });
  }

  getActiveMerchantsCount(): number {
    return this.activeMerchantsCount();
  }

  filterMerchants(): void {
    // Filtering is now handled by computed signal
    // This method kept for compatibility with existing template
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('ar-SA', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getCurrentDate(): string {
    return new Date().toISOString();
  }

  getEndDate(months: number): string {
    const endDate = new Date();
    endDate.setMonth(endDate.getMonth() + months);
    return endDate.toISOString();
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'active': return 'نشط';
      case 'inactive': return 'غير مفعل';
      case 'suspended': return 'موقوف';
      case 'pending': return 'قيد الانتظار';
      case 'awaiting_approval': return 'بانتظار الموافقة';
      case 'expired': return 'منتهي';
      default: return 'غير معروف';
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'status-active';
      case 'inactive': return 'status-inactive';
      case 'pending': return 'status-pending';
      case 'awaiting_approval': return 'status-pending';
      case 'expired': return 'status-expired';
      default: return 'status-unknown';
    }
  }

  editMerchant(merchantId: string): void {
    const merchant = this.merchants().find(m => m.id === merchantId);
    if (merchant) {
      this.selectedMerchant.set(merchant);
      // Initialize form data with merchant details
      this.editFormData.set({
        businessName: merchant.businessName,
        ownerName: merchant.ownerName,
        city: merchant.city,
        plan: merchant.plan
      });
      this.isEditModalOpen.set(true);
    }
  }

  closeEditModal(): void {
    this.isEditModalOpen.set(false);
    this.selectedMerchant.set(null);
    this.editFormData.set({
      businessName: '',
      ownerName: '',
      city: '',
      plan: 'basic'
    });
  }

  saveEditedMerchant(): void {
    const merchant = this.selectedMerchant();
    if (!merchant) return;

    if (!this.editFormData().businessName?.trim() || !this.editFormData().ownerName?.trim()) {
      this.toast.showError('يرجى ملء جميع الحقول المطلوبة');
      return;
    }

    this.isSaving.set(true);
    const updateRequest = {
      businessName: this.editFormData().businessName,
      ownerName: this.editFormData().ownerName,
      city: this.editFormData().city,
      plan: this.editFormData().plan
    };

    this.superAdminService.updateMerchant(merchant.id, updateRequest).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.toast.showSuccess('تم تحديث بيانات التاجر بنجاح');
          this.closeEditModal();
          this.loadMerchants(); // Reload merchants list to show updated data
        } else {
          this.toast.showError(response.message || 'فشل تحديث البيانات');
        }
        this.isSaving.set(false);
      },
      error: (err: any) => {
        console.error('Error updating merchant:', err);
        this.toast.showError('حدث خطأ أثناء تحديث البيانات');
        this.isSaving.set(false);
      }
    });
  }

    goBack(): void {
    window.history.back();
  }
  suspendMerchant(merchantId: string): void {
    if (!confirm('هل أنت متأكد من تعليق هذا التاجر؟')) return;
    
    this.isLoading.set(true);
    this.superAdminService.suspendMerchant(merchantId, 'تم التعليق من قبل المسؤول').subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.toast.showSuccess('تم تعليق التاجر بنجاح');
          this.loadMerchants(); // Reload to get updated status
        } else {
          this.toast.showError(response.message || 'فشل في تعليق التاجر');
        }
      },
      error: (err) => {
        console.error('Error suspending merchant:', err);
        this.toast.showError('فشل في تعليق التاجر');
        this.isLoading.set(false);
      }
    });
  }

  // Open activation modal with months selection
  openActivateModal(merchantId: string): void {
    this.merchantToActivate.set(merchantId);
    this.activationMonths.set(1);
    this.isActivateModalOpen.set(true);
  }

  // Close activation modal
  closeActivateModal(): void {
    this.isActivateModalOpen.set(false);
    this.merchantToActivate.set(null);
    this.activationMonths.set(1);
  }

  // Confirm activation with selected months
  confirmActivation(): void {
    const merchantId = this.merchantToActivate();
    if (!merchantId) return;

    const months = this.activationMonths();
    this.isLoading.set(true);
    this.closeActivateModal();

    this.superAdminService.activateMerchant(merchantId, months).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.toast.showSuccess(response.message || `تم تفعيل التاجر بنجاح لمدة ${months} شهر`);
          this.loadMerchants(); // Reload to get updated status
        } else {
          this.toast.showError(response.message || 'فشل في تفعيل التاجر');
        }
      },
      error: (err) => {
        console.error('Error activating merchant:', err);
        this.toast.showError('فشل في تفعيل التاجر');
        this.isLoading.set(false);
      }
    });
  }

  toggleMerchantStatus(merchantId: string): void {
    const merchant = this.merchants().find(m => m.id === merchantId);
    if (!merchant) return;
    
    if (merchant.status === 'active') {
      this.suspendMerchant(merchantId);
    } else {
      // Open activation modal for inactive, pending, or awaiting_approval status
      this.openActivateModal(merchantId);
    }
  }

  addMerchant(): void {
    this.addFormData.set({
      businessName: '',
      ownerName: '',
      email: '',
      city: '',
      plan: 'basic'
    });
    this.isAddModalOpen.set(true);
  }

  closeAddModal(): void {
    this.isAddModalOpen.set(false);
    this.addFormData.set({
      businessName: '',
      ownerName: '',
      email: '',
      city: '',
      plan: 'basic'
    });
  }

  saveNewMerchant(): void {
    const formData = this.addFormData();
    
    // Validation
    if (!formData.businessName?.trim() || !formData.ownerName?.trim() || 
        !formData.email?.trim() || !formData.city?.trim()) {
      this.toast.showError('يرجى ملء جميع الحقول المطلوبة');
      return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      this.toast.showError('يرجى إدخال بريد إلكتروني صحيح');
      return;
    }

    this.isSaving.set(true);
    
    const createRequest = {
      businessName: formData.businessName.trim(),
      ownerName: formData.ownerName.trim(),
      email: formData.email.trim(),
      city: formData.city.trim(),
      plan: formData.plan
    };

    this.superAdminService.createMerchant(createRequest).subscribe({
      next: (response: any) => {
        this.isSaving.set(false);
        if (response.success) {
          this.toast.showSuccess('تم إضافة التاجر بنجاح');
          this.closeAddModal();
          this.loadMerchants(); // Reload merchants list
        } else {
          this.toast.showError(response.message || 'فشل إضافة التاجر');
        }
      },
      error: (err: any) => {
        console.error('Error creating merchant:', err);
        this.toast.showError(err.error?.message || 'حدث خطأ أثناء إضافة التاجر');
        this.isSaving.set(false);
      }
    });
  }

  saveSettings(): void {
    this.isSaving.set(true);
    
    this.superAdminService.updatePlatformSettings(this.platformSettings).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.showSuccess('تم حفظ الإعدادات بنجاح');
        }
        this.isSaving.set(false);
      },
      error: (err) => {
        this.toast.showError('فشل في حفظ الإعدادات');
        this.isSaving.set(false);
      }
    });
  }

  loadSettings(): void {
    console.log('Refreshing settings');
    this.loadMerchants();
    // Reload other settings data
  }

  /**
   * Block merchant - User cannot login at all
   */
  blockMerchant(merchantId: string): void {
    if (!confirm('هل أنت متأكد من حظر هذا التاجر؟ لن يتمكن من تسجيل الدخول.')) return;
    
    this.isLoading.set(true);
    this.superAdminService.blockMerchant(merchantId).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.toast.showSuccess('تم حظر التاجر بنجاح');
          this.loadMerchants();
        } else {
          this.toast.showError(response.message || 'فشل في حظر التاجر');
        }
      },
      error: (err) => {
        console.error('Error blocking merchant:', err);
        this.toast.showError('فشل في حظر التاجر');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Unblock merchant - User can login again
   */
  unblockMerchant(merchantId: string): void {
    if (!confirm('هل أنت متأكد من إلغاء حظر هذا التاجر؟')) return;
    
    this.isLoading.set(true);
    this.superAdminService.unblockMerchant(merchantId).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.toast.showSuccess('تم إلغاء حظر التاجر بنجاح');
          this.loadMerchants();
        } else {
          this.toast.showError(response.message || 'فشل في إلغاء حظر التاجر');
        }
      },
      error: (err) => {
        console.error('Error unblocking merchant:', err);
        this.toast.showError('فشل في إلغاء حظر التاجر');
        this.isLoading.set(false);
      }
    });
  }

  onMaintenanceToggle(): void {
    if (this.platformSettings.maintenanceMode) {
      const confirm = window.confirm('هل أنت متأكد من تفعيل وضع الصيانة؟ سيتوقف النظام مؤقتاً.');
      if (!confirm) {
        this.platformSettings.maintenanceMode = false;
      }
    }
  }
}