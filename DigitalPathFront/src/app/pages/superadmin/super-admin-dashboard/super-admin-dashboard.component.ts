import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { interval, Subscription } from 'rxjs';
import { SuperAdminService } from '../../../core/services/super-admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './super-admin-dashboard.component.html', // تأكد من الاسم الصحيح هنا
  styleUrls: ['./super-admin-dashboard.component.css'],
  animations: [
    trigger('cardAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.3s cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class SuperAdminDashboardComponent implements OnInit, OnDestroy {
  currentTime = '';
  private timeSubscription!: Subscription;
  isLoading = true;
  
  systemData = {
    totalCustomers: 0,
    totalMerchants: 0,
    totalRevenue: 0,
    activeWashes: 0,
    // New statistics
    newMerchantsThisMonth: 0,
    newCustomersThisMonth: 0,
    totalSaaSRevenueAllTime: 0,
    totalSaaSRevenueThisMonth: 0,
    activeMerchants: 0,
    inactiveMerchants: 0,
    basicPlanCount: 0,
    proPlanCount: 0,
    totalWashesAllTime: 0,
    totalWashesThisMonth: 0,
    totalRewardsGiven: 0,
    totalRewardsClaimed: 0,
    // Wash Revenue (merchants' earnings)
    totalWashRevenueAllTime: 0,
    totalWashRevenueThisMonth: 0,
    avgWashPrice: 0,
    stats: {
      monthlyGrowth: 0,
      systemUptime: 0,
      avgTransactionValue: 0,
      totalTransactions: 0
    } as any
  };
  
  recentActivity: any[] = [];

  constructor(
    private router: Router,
    private superAdminService: SuperAdminService,
    private authService: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.updateTime();
    this.timeSubscription = interval(60000).subscribe(() => {
      this.updateTime();
    });
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.superAdminService.getDashboard().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const dashboard = response.data;
          this.systemData = {
            totalCustomers: dashboard.totalCustomers || 0,
            totalMerchants: dashboard.totalMerchants || 0,
            totalRevenue: dashboard.totalSaaSRevenueAllTime || 0,
            activeWashes: dashboard.activeWashes || 0,
            // New statistics
            newMerchantsThisMonth: dashboard.newMerchantsThisMonth || 0,
            newCustomersThisMonth: dashboard.newCustomersThisMonth || 0,
            totalSaaSRevenueAllTime: dashboard.totalSaaSRevenueAllTime || 0,
            totalSaaSRevenueThisMonth: dashboard.totalSaaSRevenueThisMonth || 0,
            activeMerchants: dashboard.activeMerchants || 0,
            inactiveMerchants: dashboard.inactiveMerchants || 0,
            basicPlanCount: dashboard.basicPlanCount || 0,
            proPlanCount: dashboard.proPlanCount || 0,
            totalWashesAllTime: dashboard.totalWashesAllTime || 0,
            totalWashesThisMonth: dashboard.totalWashesThisMonth || 0,
            totalRewardsGiven: dashboard.totalRewardsGiven || 0,
            totalRewardsClaimed: dashboard.totalRewardsClaimed || 0,
            // Wash Revenue (merchants' earnings)
            totalWashRevenueAllTime: dashboard.totalWashRevenueAllTime || 0,
            totalWashRevenueThisMonth: dashboard.totalWashRevenueThisMonth || 0,
            avgWashPrice: dashboard.avgWashPrice || 0,
            stats: dashboard.stats || {
              monthlyGrowth: 0,
              systemUptime: 0,
              avgTransactionValue: 0,
              totalTransactions: 0
            }
          };
          this.recentActivity = (dashboard.recentActivity || []).map((a: any) => ({
            icon: a.icon || '📊',
            type: a.type || 'info',
            title: a.title,
            description: a.description,
            time: a.time,
            status: a.status || 'success',
            statusText: a.statusText || 'مكتمل'
          }));
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.toast.showError('فشل في تحميل بيانات لوحة التحكم');
        this.isLoading = false;
      }
    });
  }

  private updateTime(): void {
    const now = new Date();
    this.currentTime = now.toLocaleTimeString('ar-SA', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  }

  navigateTo(route: string): void {
    this.router.navigate([`/superadmin/${route}`]);
  }

  openSettings(): void {
    console.log('Opening settings...');
  }

  viewAllActivity(): void {
    this.router.navigate(['/superadmin/activity-logs']);
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
    if (this.timeSubscription) {
      this.timeSubscription.unsubscribe();
    }
  }
}