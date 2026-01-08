import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-merchant-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './merchant-register.component.html',
  styleUrls: ['./merchant-register.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(30px)' }),
        animate('0.6s ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(40px)' }),
        animate('0.8s 0.2s ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class MerchantRegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  selectedPlan: 'Basic' | 'Pro' = 'Basic';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private toast: ToastService
  ) {
    this.registerForm = this.fb.group({
      // Business Info
      businessName: ['', [Validators.required, Validators.minLength(3)]],
      businessType: ['car_wash', [Validators.required]],
      city: ['', [Validators.required]],
      branchName: ['', [Validators.required]],
      
      // Contact Info
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^(05|5)(5|0|3|6|4|9|1|8|7)([0-9]{7})$/)]],
      
      // Subscription
      subscriptionType: ['Basic', [Validators.required]],
      
      // Security
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$/)
      ]],
      confirmPassword: ['', [Validators.required]],
      
      // Terms
      acceptTerms: [false, [Validators.requiredTrue]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(g: FormGroup) {
    const password = g.get('password')?.value;
    const confirmPassword = g.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  selectPlan(plan: 'Basic' | 'Pro'): void {
    this.selectedPlan = plan;
    this.registerForm.patchValue({ subscriptionType: plan });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.isLoading = true;

const registerData = {
  name: this.registerForm.value.businessName,
  businessName: this.registerForm.value.businessName,
  businessType: this.registerForm.value.businessType,
  city: this.registerForm.value.city || "Riyadh", // افتراضي إذا مش موجود
  branchName: this.registerForm.value.branchName || "Main Branch",
  email: this.registerForm.value.email,
  phone: this.registerForm.value.phone,
  password: this.registerForm.value.password,
  subscriptionType: this.registerForm.value.subscriptionType || "Basic",
  paymentMethod: "cash" // القيمة الافتراضية
};
    // Call real API
    this.authService.registerMerchant(registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.toast.showSuccess('تم إنشاء حسابك بنجاح! يرجى انتظار موافقة المسؤول.');
        this.router.navigate(['/auth/signin']);
      },
      error: (error) => {
        this.isLoading = false;
        // Error is already handled by AuthService with toast
      }
    });
  }

  togglePasswordVisibility(field: 'password' | 'confirmPassword'): void {
    if (field === 'password') {
      this.showPassword = !this.showPassword;
    } else {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  handleWhatsApp(): void {
    window.open('https://wa.me/966548290509?text=أحتاج مساعدة في التسجيل كمغسلة', '_blank');
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getPasswordStrength(password: string): { score: number, color: string, text: string } {
    let score = 0;
    if (password.length >= 6) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/\d/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    const colors = ['#EF4444', '#F59E0B', '#10B981', '#10B981', '#10B981'];
    const texts = ['ضعيفة جداً', 'ضعيفة', 'جيدة', 'قوية', 'قوية جداً'];

    return {
      score: Math.min(score, 5),
      color: colors[Math.min(score, 4)],
      text: texts[Math.min(score, 4)]
    };
  }
}