import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { MerchantPublicInfo } from '../../../core/models/api-response.model';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-customer-register-merchant',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './customer-register-merchant.component.html',
  styleUrls: ['./customer-register-merchant.component.css'],
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
export class CustomerRegisterMerchantComponent implements OnInit {
  registerForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  merchantId: string | null = null;
  merchantInfo: MerchantPublicInfo | null = null;
  isLoadingMerchant = true;
  merchantError = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private toast: ToastService
  ) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^(05|5)(5|0|3|6|4|9|1|8|7)([0-9]{7})$/)]],
      carPlateNumber: ['', [Validators.required, Validators.minLength(4)]],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$/)
      ]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Get merchantId from route params
    this.merchantId = this.route.snapshot.paramMap.get('merchantId');
    
    if (this.merchantId) {
      this.loadMerchantInfo();
    } else {
      this.merchantError = true;
      this.isLoadingMerchant = false;
      this.toast.showError('رابط التسجيل غير صالح');
    }
  }

  loadMerchantInfo(): void {
    if (!this.merchantId) return;
    
    this.authService.getMerchantPublicInfo(this.merchantId).subscribe({
      next: (merchantInfo) => {
        this.merchantInfo = merchantInfo;
        this.isLoadingMerchant = false;
        
        if (!merchantInfo.isActive) {
          this.merchantError = true;
          this.toast.showError('هذا التاجر غير نشط حالياً');
        }
      },
      error: () => {
        this.isLoadingMerchant = false;
        this.merchantError = true;
      }
    });
  }

  private passwordMatchValidator(g: FormGroup) {
    const password = g.get('password')?.value;
    const confirmPassword = g.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.invalid || !this.merchantId) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.isLoading = true;
    const registerData = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      phone: this.registerForm.value.phone,
      password: this.registerForm.value.password,
      merchantId: this.merchantId,
      carPlateNumber: this.registerForm.value.carPlateNumber
    };

    this.authService.registerCustomer(registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.toast.showSuccess('تم إنشاء حسابك بنجاح! سجل دخولك الآن.');
        this.router.navigate(['/auth/signin']);
      },
      error: () => {
        this.isLoading = false;
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
    window.open('https://wa.me/966548290509?text=أحتاج مساعدة في التسجيل كعميل', '_blank');
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
