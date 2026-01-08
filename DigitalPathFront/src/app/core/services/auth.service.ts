import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { ApiResponse, LoginRequest, LoginResponse, User, CustomerRegistrationData, MerchantPublicInfo } from '../models/api-response.model';
import { ToastService } from './toast.service';
import { catchError, map, tap } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUser = signal<User | null>(null);
  private token = signal<string | null>(null);

  readonly isAuthenticated = computed(() => !!this.token());
  readonly user = computed(() => this.currentUser());

  constructor(
    private http: HttpClient,
    private router: Router,
    private toast: ToastService
  ) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    if (typeof localStorage === 'undefined') {
      console.log('ℹ️ [AUTH] localStorage not available');
      return;
    }
    
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');
    
    if (token && user) {
      try {
        console.log('💾 [AUTH] Loading user from storage...');
        this.token.set(token);
        this.currentUser.set(JSON.parse(user));
        console.log('✅ [AUTH] User loaded from storage:', this.currentUser());
      } catch (error) {
        console.error('❌ [AUTH] Failed to parse user from storage:', error);
        this.logout();
      }
    } else {
      console.log('ℹ️ [AUTH] No stored user data found');
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    console.log('🔐 [AUTH] Login attempt for:', credentials.email);
    
    return this.http.post<ApiResponse<LoginResponse>>(
      `${environment.apiUrl}/auth/login`,
      credentials
    ).pipe(
      tap(response => {
        console.log('📡 [AUTH] Login response received:', response);
        if (response.success && response.data) {
          console.log('💾 [AUTH] Setting auth data...');
          this.setAuthData(response.data);
          this.toast.showSuccess('تم تسجيل الدخول بنجاح!');
        }
      }),
      map(response => {
        if (response.success && response.data) {
          console.log('✅ [AUTH] Login successful');
          return response.data;
        }
        throw new Error(response.message || 'فشل تسجيل الدخول');
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.error?.errors?.[0] || error.message || 'فشل تسجيل الدخول';
        this.toast.showError(errorMessage);
        console.error('❌ [AUTH] Login error:', error);
        return throwError(() => error);
      })
    );
  }

  registerCustomer(data: CustomerRegistrationData): Observable<any> {
    return this.http.post<ApiResponse<any>>(
      `${environment.apiUrl}/auth/register/customer`,
      data
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم إنشاء الحساب بنجاح! يرجى تسجيل الدخول.');
          return response.data;
        }
        throw new Error(response.message || 'فشل التسجيل');
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.error?.errors?.[0] || 'فشل التسجيل';
        this.toast.showError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Validate merchant code before registration
   */
  validateMerchantCode(code: string): Observable<MerchantPublicInfo> {
    return this.http.post<ApiResponse<MerchantPublicInfo>>(
      `${environment.apiUrl}/qrcode/merchant/validate-code`,
      { code }
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        throw new Error(response.message || 'الكود غير صحيح');
      }),
      catchError(error => {
        const errorMessage = error.error?.message || 'الكود غير صحيح';
        this.toast.showError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Get merchant public info by ID
   */
  getMerchantPublicInfo(merchantId: string): Observable<MerchantPublicInfo> {
    return this.http.get<ApiResponse<MerchantPublicInfo>>(
      `${environment.apiUrl}/qrcode/merchant/${merchantId}/info`
    ).pipe(
      map(response => {
        if (response.success && response.data) {
          return response.data;
        }
        throw new Error(response.message || 'التاجر غير موجود');
      }),
      catchError(error => {
        const errorMessage = error.error?.message || 'التاجر غير موجود';
        this.toast.showError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  registerMerchant(data: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(
      `${environment.apiUrl}/auth/register/merchant`,
      data
    ).pipe(
      map(response => {
        if (response.success) {
          this.toast.showSuccess('تم إنشاء حساب التاجر بنجاح! يرجى تسجيل الدخول.');
          return response.data;
        }
        throw new Error(response.message || 'فشل التسجيل');
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.error?.errors?.[0] || 'فشل التسجيل';
        this.toast.showError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  private setAuthData(data: LoginResponse): void {
    if (typeof localStorage === 'undefined') {
      console.error('❌ [AUTH] localStorage not available');
      return;
    }
    
    console.log('💾 [AUTH] Setting token:', data.token.substring(0, 50) + '...');
    this.token.set(data.token);
    this.currentUser.set(data.user);
    
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    
    console.log('✅ [AUTH] Token saved successfully');
    console.log('✅ [AUTH] Current token:', this.token());
    console.log('✅ [AUTH] Current user:', this.currentUser());
  }

  logout(): void {
    this.token.set(null);
    this.currentUser.set(null);
    
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    
    this.router.navigate(['/auth/signin']);
    this.toast.showInfo('تم تسجيل الخروج بنجاح');
  }

  getToken(): string | null {
    return this.token();
  }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.role === role;
  }

  getUser(): User | null {
    return this.currentUser();
  }

  isCustomer(): boolean {
    return this.hasRole('customer');
  }

  isMerchant(): boolean {
    return this.hasRole('merchant');
  }

  isSuperAdmin(): boolean {
    return this.hasRole('superadmin');
  }
}