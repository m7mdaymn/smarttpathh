import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-merchant-inactive',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './merchant-inactive.component.html',
  styleUrls: ['./merchant-inactive.component.css']
})
export class MerchantInactiveComponent implements OnInit, OnDestroy {
  merchantName: string = '';
  businessName: string = '';
  whatsappNumber = '+966548290509';
  isCheckingStatus = false;
  private statusCheckSubscription?: Subscription;

  constructor(
    private router: Router,
    private authService: AuthService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const user = this.authService.user();
    if (!user) {
      this.router.navigate(['/auth/signin']);
      return;
    }
    
    this.merchantName = user.name || '';
    this.businessName = (user as any).businessName || 'مغسلتك';
    
    // Check if already active
    this.checkMerchantStatus();
    
    // Periodically check status every 30 seconds
    this.statusCheckSubscription = interval(30000).subscribe(() => {
      this.checkMerchantStatus();
    });
  }

  checkMerchantStatus(): void {
    const user = this.authService.user();
    if (!user) return;
    
    this.isCheckingStatus = true;
    
    // Get merchant ID first
    this.http.get<any>(`${environment.apiUrl}/merchant/by-user/${user.id}`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Get merchant profile to check subscription status
          this.http.get<any>(`${environment.apiUrl}/merchant/${response.data}/profile`).subscribe({
            next: (profileResponse) => {
              this.isCheckingStatus = false;
              if (profileResponse.success && profileResponse.data) {
                const subscriptionStatus = profileResponse.data.subscriptionStatus;
                
                if (subscriptionStatus === 'active') {
                  // Update local user data
                  const updatedUser = { ...user, subscriptionStatus: 'active' };
                  localStorage.setItem('user', JSON.stringify(updatedUser));
                  
                  // Redirect to dashboard
                  this.router.navigate(['/merchant/dashboard']);
                }
              }
            },
            error: () => {
              this.isCheckingStatus = false;
            }
          });
        } else {
          this.isCheckingStatus = false;
        }
      },
      error: () => {
        this.isCheckingStatus = false;
      }
    });
  }

  contactSupport(): void {
    const message = `السلام عليكم، أنا ${this.merchantName} من ${this.businessName}. أود تفعيل حسابي وإكمال عملية الدفع.`;
    const encodedMessage = encodeURIComponent(message);
    window.open(`https://wa.me/${this.whatsappNumber}?text=${encodedMessage}`, '_blank');
  }

  refreshStatus(): void {
    this.checkMerchantStatus();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/signin']);
  }

  ngOnDestroy(): void {
    if (this.statusCheckSubscription) {
      this.statusCheckSubscription.unsubscribe();
    }
  }
}
