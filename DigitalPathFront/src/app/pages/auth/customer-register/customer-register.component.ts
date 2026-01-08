import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { MerchantPublicInfo } from '../../../core/models/api-response.model';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-customer-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './customer-register.component.html',
  styleUrls: ['./customer-register.component.css'],
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
export class CustomerRegisterComponent implements OnInit {
  registerForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  
  // Merchant linking
  merchantCode = '';
  merchantInfo: MerchantPublicInfo | null = null;
  isValidatingCode = false;
  codeValidated = false;

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
      merchantCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      carPlateNumber: [''],
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
    // Check if merchant code was passed in query params
    const code = this.route.snapshot.queryParamMap.get('code');
    if (code) {
      this.registerForm.get('merchantCode')?.setValue(code);
      this.validateMerchantCode();
    }
  }

  private passwordMatchValidator(g: FormGroup) {
    const password = g.get('password')?.value;
    const confirmPassword = g.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  validateMerchantCode(): void {
    const code = this.registerForm.get('merchantCode')?.value;
    if (!code || code.length !== 6) {
      return;
    }
    
    this.isValidatingCode = true;
    this.authService.validateMerchantCode(code.toUpperCase()).subscribe({
      next: (info) => {
        this.merchantInfo = info;
        this.codeValidated = true;
        this.isValidatingCode = false;
        if (!info.isActive) {
          this.toast.showError('هذا التاجر غير نشط حالياً');
          this.codeValidated = false;
        }
      },
      error: () => {
        this.merchantInfo = null;
        this.codeValidated = false;
        this.isValidatingCode = false;
      }
    });
  }
  
  onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    input.value = input.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    this.registerForm.get('merchantCode')?.setValue(input.value);
    
    // Auto validate when 6 characters
    if (input.value.length === 6) {
      this.validateMerchantCode();
    } else {
      this.codeValidated = false;
      this.merchantInfo = null;
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }
    
    if (!this.codeValidated || !this.merchantInfo) {
      this.toast.showError('يرجى إدخال كود تاجر صحيح');
      return;
    }

    this.isLoading = true;
    const registerData = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      phone: this.registerForm.value.phone,
      password: this.registerForm.value.password,
      merchantCode: this.registerForm.value.merchantCode.toUpperCase(),
      carPlateNumber: this.registerForm.value.carPlateNumber || ''
    };

    // Call real API
    this.authService.registerCustomer(registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.toast.showSuccess('تم إنشاء حسابك بنجاح! سجل دخولك الآن.');
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