// customer-dashboard.component.ts
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

interface User {
  id: string;
  name: string;
  phone: string;
  email: string;
  carPlate?: string;
  carPhoto?: string;
  avatar?: string;
  customerQRCode?: string;
}

interface LoyaltyCard {
  id: string;
  merchantId: string;
  merchantName: string;
  merchantLocation: string;
  merchantPhone: string;
  currentStamps: number;
  requiredStamps: number;
  expiryDate: string;
  rewardDescription: string;
  isRewardAchievable: boolean;
  isActive: boolean;
  isRewardEarned?: boolean;
  rewardQRCode?: string;
  rewardExpiresAt?: string;
  daysRemaining?: number;
  totalWashesWithMerchant?: number;
  lastWashDate?: string;
  allowCarPhotoUpload?: boolean; // Pro plan feature
}

interface Reward {
  id: string;
  title: string;
  description: string;
  type: string;
  value: number;
  expiresAt: string;
  status: 'available' | 'claimed' | 'expired';
  merchantName: string;
  qrCode?: string;
  rewardCode?: string;
  claimedAt?: string;
  isExpired: boolean;
  daysUntilExpiry: number;
}

interface WashHistory {
  id: string;
  date: string;
  time: string;
  merchantName: string;
  merchantLogo?: string;
  washType: string;
  serviceNames: string[];
  price: number;
  status: string;
  rating?: number;
  notes?: string;
  discountApplied?: number;
}

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css']
})
export class CustomerDashboardComponent implements OnInit {
  user: User = {
    id: '',
    name: '',
    phone: '',
    email: '',
    carPlate: '',
    carPhoto: '',
    avatar: ''
  };
  
  loyaltyCards: LoyaltyCard[] = [];
  allRewards: Reward[] = [];
  filteredRewards: Reward[] = [];
  washHistory: WashHistory[] = [];
  customerQRCode: string = '';
  isLoading: boolean = false;
  
  // Modal states
  showLoyaltyQRModal: boolean = false;
  showRewardQRModal: boolean = false;
  showPhotoUploadModal: boolean = false;
  showWalletExportModal: boolean = false;
  showRatingModal: boolean = false;
  hasUploadedPhoto: boolean = false;
  
  // Rating modal state
  selectedWashForRating: WashHistory | null = null;
  selectedRating: number = 0;
  hoverRating: number = 0;
  ratingComment: string = '';
  isSubmittingRating: boolean = false;
  
  // Wallet export
  selectedCardForExport: LoyaltyCard | null = null;
  
  // Rewards filtering
  rewardsTab: 'available' | 'claimed' | 'expired' = 'available';
  availableRewardsCount: number = 0;
  claimedRewardsCount: number = 0;
  expiredRewardsCount: number = 0;
  
  selectedReward: Reward | null = null;
  
  // Notifications
  notifications: any[] = [];
  unreadNotificationsCount: number = 0;
  showAllNotifications: boolean = false;
  showNotificationModal: boolean = false;
  
  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    private customerService: CustomerService,
    private authService: AuthService,
    private router: Router
  ) {}

ngOnInit(): void {
  this.loadDashboardData();
}

// Open loyalty QR modal
openLoyaltyQRModalMethod(): void {
  this.showLoyaltyQRModal = true;
}
  loadDashboardData(): void {
    this.isLoading = true;
    
    // Get current user from AuthService
    const currentUser = this.authService.user() as any;
    
    if (currentUser) {
      // Initialize with basic data from auth
      this.user = {
        id: currentUser.id || '',
        name: currentUser.name || '',
        phone: currentUser.phone || '',
        email: currentUser.email || '',
        carPlate: currentUser.carPlate || '',
        carPhoto: currentUser.carPhoto || '',
      };
      this.hasUploadedPhoto = !!currentUser.carPhoto;
      
      // Load full customer profile from API for more details
      this.customerService.getProfileById(this.user.id).subscribe({
        next: (response: any) => {
          if (response && response.success && response.data) {
            const profile = response.data;
            this.user = {
              ...this.user,
              name: profile.name || this.user.name,
              phone: profile.phone || this.user.phone,
              email: profile.email || this.user.email,
              carPlate: profile.plateNumber || profile.carPlate || this.user.carPlate,
              carPhoto: profile.carPhoto ? this.getFullPhotoUrl(profile.carPhoto) : this.user.carPhoto,
              customerQRCode: profile.qrCode || '',
              avatar: profile.avatar || ''
            };
            this.hasUploadedPhoto = !!this.user.carPhoto;
          }
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error loading profile:', err);
          this.isLoading = false;
        }
      });
      
      // Load all data in parallel
      this.loadCarPhotos();
      this.loadLoyaltyCards();
      this.loadRewards();
      this.loadWashHistory();
      this.loadCustomerQRCode();
      this.loadNotifications();
    } else {
      this.isLoading = false;
    }
  }

  getUserAvatar(user: any): string {
    if (user.avatar) return user.avatar;
    if (user.name) {
      const name = user.name.split(' ').map((n: string) => n[0]).join('');
      return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=3B82F6&color=fff&size=128`;
    }
    return '';
  }

  getUserInitials(): string {
    return this.user.name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  loadLoyaltyCards(): void {
    this.customerService.getLoyaltyCards(this.user.id).subscribe({
      next: (response: any) => {
        if (response && response.data) {
          console.log('💳 [LOYALTY-CARDS] Raw response:', response.data);
          this.loyaltyCards = response.data.map((card: any) => {
            // Enable car photo upload if merchant is on Pro plan
            const merchantPlan = card.merchantPlan || card.plan || 'basic';
            const allowCarPhotoUpload = card.allowCarPhotoUpload !== undefined 
              ? card.allowCarPhotoUpload 
              : merchantPlan?.toLowerCase() === 'pro';
            
            return {
              id: card.id,
              merchantId: card.merchantId,
              merchantName: card.merchantName || 'تاجر',
              // Try merchantLocation first, then merchantCity, then fallback to default
              merchantLocation: card.merchantLocation || card.merchantCity || card.city || 'موقع غير محدد',
              merchantPhone: card.merchantPhone || 'غير متوفر',
              currentStamps: card.currentStamps || card.washesCompleted || 0,
              requiredStamps: card.requiredStamps || card.washesRequired || 10,
              expiryDate: this.formatDate(card.expiryDate),
              rewardDescription: card.rewardDescription || 'مكافأة',
              isRewardAchievable: (card.currentStamps || 0) >= (card.requiredStamps || 10),
              isActive: card.isActive !== false,
              isRewardEarned: card.isRewardEarned,
              rewardQRCode: card.rewardQRCode,
              rewardExpiresAt: card.rewardExpiresAt,
              daysRemaining: card.daysRemaining,
              totalWashesWithMerchant: card.totalWashesWithMerchant,
              lastWashDate: card.lastWashDate ? this.formatDate(card.lastWashDate) : undefined,
              allowCarPhotoUpload: allowCarPhotoUpload
            };
          });
          console.log('💳 [LOYALTY-CARDS] Mapped cards:', this.loyaltyCards);
        }
      },
      error: (error) => {
        console.error('❌ [LOYALTY-CARDS] Error loading loyalty cards:', error);
      }
    });
  }

  loadRewards(): void {
    console.log('[REWARDS] Loading for customer:', this.user.id);
    this.customerService.getRewardsById(this.user.id).subscribe({
      next: (response: any) => {
        console.log('[REWARDS] API Response:', response);
        if (response && response.data && response.data.length > 0) {
          console.log('[REWARDS] Found ' + response.data.length + ' rewards');
          this.processRewardsData(response.data);
        } else {
          console.warn('[REWARDS] No rewards data in response');
        }
      },
      error: (error) => {
        console.error('[REWARDS] Error loading rewards:', error);
      }
    });
  }

  processRewardsData(rewardsData: any[]): void {
    const now = new Date();
    console.log('[REWARDS] Processing ' + rewardsData.length + ' rewards...');
    
    this.allRewards = rewardsData.map((reward: any) => {
      const expiryDate = new Date(reward.expiresAt || reward.expiryDate || '');
      const daysUntilExpiry = Math.ceil((expiryDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
      
      const mappedReward = {
        id: reward.id,
        title: reward.title || 'مكافأة',
        description: reward.description || '',
        type: reward.type || 'free_wash',
        value: reward.value || 0,
        expiresAt: reward.expiresAt || reward.expiryDate,
        status: this.determineRewardStatus(reward.status, expiryDate),
        merchantName: reward.merchantName || reward.merchant || 'غير محدد',
        qrCode: reward.rewardQRCode || reward.qrCode, // Map rewardQRCode properly
        rewardCode: reward.rewardCode,
        claimedAt: reward.claimedAt || '',
        isExpired: reward.isExpired || expiryDate < now,
        daysUntilExpiry: Math.max(0, daysUntilExpiry)
      };
      console.log('[REWARDS] Reward QR:', mappedReward.qrCode, 'Title:', mappedReward.title);
      return mappedReward;
    });
    
    // Update counters
    this.availableRewardsCount = this.allRewards.filter(r => r.status === 'available').length;
    this.claimedRewardsCount = this.allRewards.filter(r => r.status === 'claimed').length;
    this.expiredRewardsCount = this.allRewards.filter(r => r.status === 'expired').length;
    
    // Filter for current tab
    this.filterRewards();
  }

  determineRewardStatus(status: string, expiryDate: Date): 'available' | 'claimed' | 'expired' {
    const now = new Date();
    
    if (status === 'claimed' || status === 'used') return 'claimed';
    if (status === 'expired' || expiryDate < now) return 'expired';
    return 'available';
  }

  filterRewards(): void {
    this.filteredRewards = this.allRewards.filter(reward => {
      if (this.rewardsTab === 'available') return reward.status === 'available';
      if (this.rewardsTab === 'claimed') return reward.status === 'claimed';
      if (this.rewardsTab === 'expired') return reward.status === 'expired';
      return true;
    });
  }

  setRewardsTab(tab: 'available' | 'claimed' | 'expired'): void {
    this.rewardsTab = tab;
    this.filterRewards();
  }

  loadWashHistory(): void {
    this.customerService.getWashHistoryById(this.user.id).subscribe({
      next: (response: any) => {
        if (response && response.data) {
          this.washHistory = response.data.map((wash: any) => ({
            id: wash.id,
            date: this.formatDate(wash.washDate || wash.date),
            time: this.formatTime(wash.washDate || wash.date),
            merchantName: wash.merchantName || 'تاجر',
            merchantLogo: wash.merchantLogo,
            washType: wash.serviceNames?.length > 0 ? wash.serviceNames.join('، ') : 'غسيل عادي',
            serviceNames: wash.serviceNames || [],
            price: wash.amount || wash.price || 0,
            status: wash.status || 'completed',
            rating: wash.rating,
            notes: wash.notes,
            discountApplied: wash.discountApplied || 0
          }));
        }
      },
      error: (error) => {
        console.error('Error loading wash history:', error);
      }
    });
  }

  loadCustomerQRCode(): void {
    this.customerService.getCustomerQRCodeImage(this.user.id).subscribe({
      next: (blob: Blob) => {
        if (blob && blob.size > 0) {
          const reader = new FileReader();
          reader.onload = () => {
            this.customerQRCode = reader.result as string;
          };
          reader.readAsDataURL(blob);
        }
      },
      error: (error) => {
        console.error('Error loading QR code:', error);
        this.customerQRCode = '';
      }
    });
  }

  loadNotifications(): void {
    this.customerService.getNotifications().subscribe({
      next: (notifications: any[]) => {
        this.notifications = notifications.map(n => ({
          id: n.id,
          title: n.title,
          message: n.message,
          type: n.type,
          date: n.date || this.formatDate(n.createdAt),
          read: n.read || n.isRead,
          icon: this.getNotificationIcon(n.type)
        }));
        this.unreadNotificationsCount = this.notifications.filter(n => !n.read).length;
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
      }
    });
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'wash_complete': return '🚗';
      case 'reward_earned': return '🎁';
      case 'reward_close': return '⏰';
      case 'reward_expiry': return '⚠️';
      case 'reward_claimed': return '✅';
      case 'promotion': return '🎉';
      default: return '🔔';
    }
  }

  markAsRead(notification: any): void {
    if (notification.read) return;
    
    this.customerService.markNotificationAsRead(notification.id).subscribe({
      next: () => {
        notification.read = true;
        this.unreadNotificationsCount = this.notifications.filter(n => !n.read).length;
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  loadCarPhotos(): void {
    console.log('📸 [CAR-PHOTO] Loading car photos for user:', this.user.id);
    this.customerService.getCarPhotosById(this.user.id).subscribe({
      next: (response: any) => {
        console.log('📸 [CAR-PHOTO] Response received:', response);
        if (response && response.success && response.data && response.data.length > 0) {
          // Get the most recent car photo
          const latestPhoto = response.data[0];
          console.log('📸 [CAR-PHOTO] Latest photo found:', latestPhoto);
          if (latestPhoto && latestPhoto.photoUrl) {
            // Construct full URL for the car photo
            this.user.carPhoto = this.getFullPhotoUrl(latestPhoto.photoUrl);
            this.hasUploadedPhoto = true;
            console.log('📸 [CAR-PHOTO] Car photo set to:', this.user.carPhoto);
          }
        } else {
          console.log('📸 [CAR-PHOTO] No photos in response');
        }
      },
      error: (error) => {
        console.error('❌ [CAR-PHOTO] Error loading car photos:', error);
      }
    });
  }

  /**
   * Constructs the full URL for a photo path from the backend
   * Backend returns relative URLs like /uploads/car-photos/...
   * Need to prepend the backend base URL (without /api)
   */
  getFullPhotoUrl(photoUrl: string): string {
    if (!photoUrl) return '';
    
    // If already a full URL (data URL or http(s) URL), return as is
    if (photoUrl.startsWith('data:') || photoUrl.startsWith('http://') || photoUrl.startsWith('https://')) {
      return photoUrl;
    }
    
    // Get base URL from environment (remove /api suffix)
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}${photoUrl}`;
  }

  generateFallbackQR(): string {
    return `CUST-${this.user.id}-QR`;
  }

  generateRewardCode(reward: Reward): string {
    return `REWARD-${reward.id}-${Date.now()}`;
  }

  getProgressPercentage(card: LoyaltyCard): number {
    if (card.requiredStamps === 0) return 0;
    return (card.currentStamps / card.requiredStamps) * 100;
  }

  getRemainingStamps(card: LoyaltyCard): number {
    return Math.max(0, card.requiredStamps - card.currentStamps);
  }

  getRewardIcon(type: string): string {
    switch(type) {
      case 'free_wash': return '🚗';
      case 'discount': return '💰';
      case 'cashback': return '💵';
      default: return '🎁';
    }
  }

  getRewardStatusText(status: string): string {
    switch(status) {
      case 'available': return 'متاحة';
      case 'claimed': return 'مستخدمة';
      case 'expired': return 'منتهية';
      default: return status;
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('ar-SA');
    } catch (e) {
      return dateString;
    }
  }

  formatTime(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleTimeString('ar-SA', { hour: '2-digit', minute: '2-digit' });
    } catch (e) {
      return '-';
    }
  }

  // Reward QR Methods
  showRewardQR(reward: Reward): void {
    this.selectedReward = reward;
    this.showRewardQRModal = true;
  }

  // Generate QR image URL from text QR code
  getRewardQRImageUrl(qrCode: string): string {
    if (!qrCode) return '';
    // Use backend endpoint to generate QR image
    const encodedQR = encodeURIComponent(qrCode);
    return `${environment.apiUrl}/qrcode/reward/${encodedQR}/image?size=250`;
  }

  // Handle QR image load error - show text code as fallback
  onQRImageError(event: any): void {
    console.warn('⚠️ QR image failed to load, showing text fallback');
    const imgElement = event.target as HTMLImageElement;
    imgElement.style.display = 'none';
    // Show the fallback text element by finding the next sibling
    const parent = imgElement.parentElement;
    if (parent) {
      const fallback = parent.querySelector('.qr-fallback-text');
      if (fallback) {
        (fallback as HTMLElement).style.display = 'flex';
      }
    }
  }

  showRewardQRForCard(card: LoyaltyCard): void {
    if (card.isRewardEarned && card.rewardQRCode) {
      // Create a reward object from card data
      this.selectedReward = {
        id: card.id,
        title: card.rewardDescription,
        description: 'مكافأة من ' + card.merchantName,
        type: 'free_wash',
        value: 0,
        expiresAt: card.rewardExpiresAt || card.expiryDate,
        status: 'available',
        merchantName: card.merchantName,
        qrCode: card.rewardQRCode,
        claimedAt: '',
        isExpired: false,
        daysUntilExpiry: card.daysRemaining || 0
      };
      this.showRewardQRModal = true;
    } else {
      // Find corresponding reward in rewards list
      const reward = this.allRewards.find(r => 
        r.merchantName === card.merchantName && 
        r.status === 'available'
      );
      
      if (reward) {
        this.showRewardQR(reward);
      }
    }
  }

  openLoyaltyQRModal(): void {
    this.showLoyaltyQRModal = true;
  }

  closeLoyaltyQRModal(): void {
    this.showLoyaltyQRModal = false;
  }

  closeRewardQRModal(): void {
    this.showRewardQRModal = false;
    this.selectedReward = null;
  }

  // Notification Modal Methods
  toggleNotificationModal(): void {
    this.showNotificationModal = !this.showNotificationModal;
  }

  closeNotificationModal(): void {
    this.showNotificationModal = false;
  }

  markAllAsRead(): void {
    this.notifications.forEach(notification => {
      if (!notification.read) {
        this.markAsRead(notification);
      }
    });
  }

  // Check if any merchant allows car photo upload (Pro plan)
  canUploadCarPhoto(): boolean {
    return this.loyaltyCards.some(card => card.allowCarPhotoUpload === true);
  }

  // Export loyalty card to wallet - downloads card image
  exportToWallet(card: LoyaltyCard): void {
    // Show export modal with options
    this.selectedCardForExport = card;
    this.showWalletExportModal = true;
  }

  // Download card as image - Enhanced with comprehensive customer data
  downloadCardImage(card: LoyaltyCard): void {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Card dimensions (expanded for more content)
    canvas.width = 380;
    canvas.height = 620;

    // Draw card background with premium gradient
    const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
    gradient.addColorStop(0, '#1e3a5f');
    gradient.addColorStop(0.5, '#0f2744');
    gradient.addColorStop(1, '#0a1929');
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.roundRect(0, 0, canvas.width, canvas.height, 24);
    ctx.fill();

    // Add premium border effect
    ctx.strokeStyle = 'rgba(251, 191, 36, 0.3)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.roundRect(4, 4, canvas.width - 8, canvas.height - 8, 22);
    ctx.stroke();

    // Add shine effect
    ctx.fillStyle = 'rgba(255, 255, 255, 0.04)';
    ctx.beginPath();
    ctx.ellipse(canvas.width/2, -80, 250, 180, 0, 0, Math.PI * 2);
    ctx.fill();

    // Digital Pass Logo/Title
    ctx.font = 'bold 14px sans-serif';
    ctx.fillStyle = '#64748b';
    ctx.textAlign = 'center';
    ctx.fillText('DIGITAL PASS', canvas.width/2, 28);

    // Merchant name (prominent)
    ctx.font = 'bold 22px sans-serif';
    ctx.fillStyle = '#fbbf24';
    ctx.fillText(card.merchantName, canvas.width/2, 60);

    // Merchant location
    if (card.merchantLocation) {
      ctx.font = '12px sans-serif';
      ctx.fillStyle = '#94a3b8';
      ctx.fillText(`📍 ${card.merchantLocation}`, canvas.width/2, 82);
    }

    // QR Code area (white background with shadow effect)
    const qrSize = 160;
    const qrX = (canvas.width - qrSize) / 2;
    const qrY = 100;
    
    // Shadow for QR container
    ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
    ctx.shadowBlur = 15;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 5;
    
    ctx.fillStyle = '#ffffff';
    ctx.beginPath();
    ctx.roundRect(qrX - 12, qrY - 12, qrSize + 24, qrSize + 24, 12);
    ctx.fill();
    
    // Reset shadow
    ctx.shadowColor = 'transparent';
    ctx.shadowBlur = 0;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 0;

    // Customer Info Section
    const infoStartY = qrY + qrSize + 50;
    
    // Customer name
    ctx.font = 'bold 18px sans-serif';
    ctx.fillStyle = '#ffffff';
    ctx.textAlign = 'center';
    ctx.fillText(this.user.name, canvas.width/2, infoStartY);

    // Customer phone
    if (this.user.phone) {
      ctx.font = '14px sans-serif';
      ctx.fillStyle = '#94a3b8';
      ctx.fillText(`📱 ${this.user.phone}`, canvas.width/2, infoStartY + 25);
    }

    // Car plate number
    if (this.user.carPlate) {
      ctx.font = 'bold 14px sans-serif';
      ctx.fillStyle = '#60a5fa';
      ctx.fillText(`🚗 ${this.user.carPlate}`, canvas.width/2, infoStartY + 48);
    }

    // Divider line
    const dividerY = infoStartY + 70;
    ctx.beginPath();
    ctx.strokeStyle = 'rgba(148, 163, 184, 0.3)';
    ctx.lineWidth = 1;
    ctx.moveTo(40, dividerY);
    ctx.lineTo(canvas.width - 40, dividerY);
    ctx.stroke();

    // Progress Section
    const progressY = dividerY + 25;
    ctx.font = '14px sans-serif';
    ctx.fillStyle = '#94a3b8';
    ctx.textAlign = 'center';
    ctx.fillText('التقدم نحو المكافأة', canvas.width/2, progressY);

    // Progress bar
    const barWidth = 300;
    const barHeight = 14;
    const barX = (canvas.width - barWidth) / 2;
    const barY = progressY + 15;
    const progress = card.currentStamps / card.requiredStamps;

    ctx.fillStyle = '#374151';
    ctx.beginPath();
    ctx.roundRect(barX, barY, barWidth, barHeight, 7);
    ctx.fill();

    const progressGradient = ctx.createLinearGradient(barX, 0, barX + barWidth * progress, 0);
    progressGradient.addColorStop(0, '#0ea5e9');
    progressGradient.addColorStop(0.5, '#06b6d4');
    progressGradient.addColorStop(1, '#10b981');
    ctx.fillStyle = progressGradient;
    ctx.beginPath();
    ctx.roundRect(barX, barY, barWidth * progress, barHeight, 7);
    ctx.fill();

    // Progress text
    ctx.font = 'bold 18px sans-serif';
    ctx.fillStyle = '#ffffff';
    ctx.fillText(`${card.currentStamps} / ${card.requiredStamps}`, canvas.width/2, barY + 40);

    // Reward description (highlighted)
    const rewardY = barY + 65;
    ctx.font = 'bold 14px sans-serif';
    ctx.fillStyle = '#fbbf24';
    ctx.fillText(`🎁 ${card.rewardDescription || 'غسلة مجانية'}`, canvas.width/2, rewardY);

    // Additional info section
    const footerY = rewardY + 40;
    
    // Expiry date - with proper validation
    if (card.expiryDate) {
      const expiryDate = new Date(card.expiryDate);
      // Check if date is valid before formatting
      if (!isNaN(expiryDate.getTime())) {
        const formattedExpiry = expiryDate.toLocaleDateString('ar-SA', { 
          year: 'numeric', 
          month: 'long', 
          day: 'numeric' 
        });
        ctx.font = '12px sans-serif';
        ctx.fillStyle = '#94a3b8';
        ctx.fillText(`📅 صالحة حتى: ${formattedExpiry}`, canvas.width/2, footerY);
      } else {
        // If date is invalid, show days remaining instead
        if (card.daysRemaining && card.daysRemaining > 0) {
          ctx.font = '12px sans-serif';
          ctx.fillStyle = '#94a3b8';
          ctx.fillText(`📅 صالحة لمدة ${card.daysRemaining} يوم`, canvas.width/2, footerY);
        }
      }
    } else if (card.daysRemaining && card.daysRemaining > 0) {
      // Fallback to days remaining if no expiry date
      ctx.font = '12px sans-serif';
      ctx.fillStyle = '#94a3b8';
      ctx.fillText(`📅 صالحة لمدة ${card.daysRemaining} يوم`, canvas.width/2, footerY);
    }

    // Merchant phone
    if (card.merchantPhone) {
      ctx.font = '12px sans-serif';
      ctx.fillStyle = '#94a3b8';
      ctx.fillText(`☎️ للاستفسار: ${card.merchantPhone}`, canvas.width/2, footerY + 22);
    }

    // QR Code text (bottom)
    ctx.font = '10px monospace';
    ctx.fillStyle = '#475569';
    ctx.fillText(this.user.customerQRCode || 'DP-CUST-...', canvas.width/2, canvas.height - 35);

    // Timestamp
    const now = new Date();
    ctx.font = '9px sans-serif';
    ctx.fillStyle = '#475569';
    ctx.fillText(`تم التصدير: ${now.toLocaleDateString('ar-SA')}`, canvas.width/2, canvas.height - 18);

    // Load QR image and draw it
    const qrImg = new Image();
    qrImg.crossOrigin = 'anonymous';
    qrImg.onload = () => {
      ctx.drawImage(qrImg, qrX, qrY, qrSize, qrSize);
      
      // Download the canvas as image
      const link = document.createElement('a');
      link.download = `loyalty-card-${card.merchantName.replace(/\s+/g, '-')}.png`;
      link.href = canvas.toDataURL('image/png');
      link.click();
    };
    qrImg.onerror = () => {
      // Draw placeholder text in QR area
      ctx.font = '12px sans-serif';
      ctx.fillStyle = '#6b7280';
      ctx.textAlign = 'center';
      ctx.fillText('امسح الكود', qrX + qrSize/2, qrY + qrSize/2 - 10);
      ctx.fillText('من التطبيق', qrX + qrSize/2, qrY + qrSize/2 + 10);
      
      const link = document.createElement('a');
      link.download = `loyalty-card-${card.merchantName.replace(/\s+/g, '-')}.png`;
      link.href = canvas.toDataURL('image/png');
      link.click();
    };
    qrImg.src = this.customerQRCode;
  }

  // Close wallet export modal
  closeWalletExportModal(): void {
    this.showWalletExportModal = false;
    this.selectedCardForExport = null;
  }

  // Rating Modal Methods
  openRatingModal(wash: WashHistory): void {
    this.selectedWashForRating = wash;
    this.selectedRating = 0;
    this.hoverRating = 0;
    this.ratingComment = '';
    this.showRatingModal = true;
  }

  closeRatingModal(): void {
    this.showRatingModal = false;
    this.selectedWashForRating = null;
    this.selectedRating = 0;
    this.hoverRating = 0;
    this.ratingComment = '';
  }

  setRating(rating: number): void {
    this.selectedRating = rating;
  }

  getRatingText(rating: number): string {
    switch (rating) {
      case 1: return 'سيء جداً 😞';
      case 2: return 'سيء 😕';
      case 3: return 'مقبول 😐';
      case 4: return 'جيد 🙂';
      case 5: return 'ممتاز 😍';
      default: return '';
    }
  }

  submitRating(): void {
    if (!this.selectedWashForRating || this.selectedRating === 0) return;

    this.isSubmittingRating = true;
    this.customerService.rateWash(this.selectedWashForRating.id, this.selectedRating, this.ratingComment).subscribe({
      next: (response: any) => {
        console.log('✅ Rating submitted:', response);
        // Update the wash in the local list
        const washIndex = this.washHistory.findIndex(w => w.id === this.selectedWashForRating?.id);
        if (washIndex !== -1) {
          this.washHistory[washIndex].rating = this.selectedRating;
        }
        this.closeRatingModal();
        this.isSubmittingRating = false;
      },
      error: (error) => {
        console.error('❌ Error submitting rating:', error);
        this.isSubmittingRating = false;
        alert('حدث خطأ أثناء إرسال التقييم. حاول مرة أخرى.');
      }
    });
  }

  // Car Photo Methods
  uploadCarPhoto(): void {
    if (!this.canUploadCarPhoto()) {
      alert('ميزة رفع صور السيارة متاحة فقط مع التجار المشتركين في الباقة الاحترافية');
      return;
    }
    console.log('📸 [CAR-PHOTO] Upload button clicked. Current state: hasUploadedPhoto=', this.hasUploadedPhoto);
    this.showPhotoUploadModal = true;
  }

  changeCarPhoto(): void {
    if (this.hasUploadedPhoto) {
      const confirmChange = confirm('لقد قمت برفع صورة مسبقاً. هل تريد تغييرها؟');
      if (confirmChange) {
        this.showPhotoUploadModal = true;
      }
    } else {
      this.uploadCarPhoto();
    }
  }

  triggerFileUpload(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Allow replacing existing photo on Pro plan
      console.log('📸 [CAR-PHOTO] File selected:', file.name, 'Current hasUploadedPhoto:', this.hasUploadedPhoto);

      if (!file.type.match('image.*')) {
        alert('الرجاء اختيار ملف صورة');
        return;
      }

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.user.carPhoto = e.target.result;
        this.hasUploadedPhoto = true;
        console.log('📸 [CAR-PHOTO] Photo loaded, saving...');
        this.saveCarPhoto();
        this.closePhotoUploadModal();
      };
      reader.readAsDataURL(file);
    }
  }

  saveCarPhoto(): void {
    if (this.user.carPhoto) {
      const blob = this.dataURLtoBlob(this.user.carPhoto);
      const file = new File([blob], 'car-photo.jpg', { type: 'image/jpeg' });
      
      console.log('📸 [CAR-PHOTO] Uploading to backend. User ID:', this.user.id, 'File size:', file.size);
      this.customerService.uploadCarPhotoById(this.user.id, file).subscribe({
        next: (response) => {
          console.log('✅ [CAR-PHOTO] Photo uploaded successfully:', response);
        },
        error: (error) => {
          console.error('❌ [CAR-PHOTO] Upload error:', error);
        }
      });
    }
  }

  dataURLtoBlob(dataURL: string): Blob {
    const arr = dataURL.split(',');
    const mime = arr[0].match(/:(.*?);/)![1];
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);
    
    while (n--) {
      u8arr[n] = bstr.charCodeAt(n);
    }
    
    return new Blob([u8arr], { type: mime });
  }

  // Modal Methods
  closePhotoUploadModal(): void {
    this.showPhotoUploadModal = false;
  }

  // Empty states helpers
  getEmptyIcon(): string {
    switch(this.rewardsTab) {
      case 'available': return '🎁';
      case 'claimed': return '✅';
      case 'expired': return '⏰';
      default: return '🎁';
    }
  }

  getEmptyTitle(): string {
    switch(this.rewardsTab) {
      case 'available': return 'لا توجد مكافآت متاحة';
      case 'claimed': return 'لم تستخدم أي مكافآت بعد';
      case 'expired': return 'لا توجد مكافآت منتهية';
      default: return 'لا توجد مكافآت';
    }
  }

  getEmptyMessage(): string {
    switch(this.rewardsTab) {
      case 'available': return 'أكمل غسلاتك للحصول على مكافآت مجانية!';
      case 'claimed': return 'ستظهر المكافآت المستخدمة هنا بعد استبدالها';
      case 'expired': return 'جميع مكافآتك صالحة حالياً';
      default: return '';
    }
  }
  // Add these methods to the component class

// Get completed wash icons (teal with bubbles)
getCompletedWashIcons(card: LoyaltyCard): number[] {
  return Array(card.currentStamps).fill(0);
}

// Get remaining wash icons (gray outline)
getRemainingWashIcons(card: LoyaltyCard): number[] {
  const remaining = Math.max(0, card.requiredStamps - card.currentStamps);
  return Array(remaining).fill(0);
}

// Get remaining washes count
getRemainingWashes(card: LoyaltyCard): number {
  return Math.max(0, card.requiredStamps - card.currentStamps);
}



  // Logout - Fixed to use the correct AuthService method
  logout(): void {
    if (confirm('هل تريد تسجيل الخروج؟')) {
      this.authService.logout();
    }
  }
}