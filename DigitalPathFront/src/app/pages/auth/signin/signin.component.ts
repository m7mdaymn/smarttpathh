import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: 'signin.component.html',
  styleUrls: ['signin.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.6s ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(30px)' }),
        animate('0.8s 0.3s ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class SigninComponent {
  signinForm: FormGroup;
  isLoading = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private toast: ToastService
  ) {
    this.signinForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  onSubmit(): void {
    if (this.signinForm.invalid) {
      this.markFormGroupTouched(this.signinForm);
      return;
    }

    this.isLoading = true;
    const { email, password } = this.signinForm.value;

    // Use real API call
    this.authService.login({ email, password }).subscribe({
      next: (response) => {
        this.isLoading = false;
        const userRole = response.user?.role || 'customer';
        const subscriptionStatus = response.user?.subscriptionStatus;
        
        // Navigate based on user role
        if (userRole === 'merchant') {
          // Check if merchant is inactive/pending - redirect to inactive page
          if (subscriptionStatus === 'inactive' || subscriptionStatus === 'pending' || subscriptionStatus === 'awaiting_approval') {
            this.router.navigate(['/merchant/inactive']);
          } else {
            this.router.navigate(['/merchant/dashboard']);
          }
        } else if (userRole === 'superadmin') {
          this.router.navigate(['/superadmin/dashboard']);
        } else {
          this.router.navigate(['/customer/dashboard']);
        }
      },
      error: (error) => {
        this.isLoading = false;
        // Error is already handled by AuthService with toast
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  handleWhatsApp(): void {
    window.open('https://wa.me/966548290509?text=أحتاج مساعدة في تسجيل الدخول', '_blank');
  }

  handleForgotPassword(): void {
    this.toast.showInfo('سيتم إرسال رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني');
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }
}