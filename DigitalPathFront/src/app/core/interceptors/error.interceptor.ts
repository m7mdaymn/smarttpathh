// src/app/core/interceptors/error.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private toast: ToastService,
    private authService: AuthService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // ✅ IMPORTANT: Don't logout on auth endpoint errors
        // Auth endpoints (login/register) can fail without logging user out
        if (req.url.includes('/auth/login') || 
            req.url.includes('/auth/register') ||
            req.url.includes('/auth/')) {
          console.log('ℹ️ [ERROR] Auth endpoint error, letting it pass through:', error.status);
          return throwError(() => error);
        }

        let errorMessage = 'An error occurred';
        
        if (error.error instanceof ErrorEvent) {
          errorMessage = error.error.message;
        } else {
          errorMessage = error.error?.message || error.message;
          
          // Only logout on 401 for PROTECTED endpoints (not auth endpoints)
          if (error.status === 401) {
            console.error('❌ [INTERCEPTOR] 401 Unauthorized on protected endpoint, logging out');
            this.toast.showError('انتهت جلستك. يرجى تسجيل الدخول مجددًا.');
            this.authService.logout();
            return throwError(() => error);
          }
          
          if (error.status === 403) {
            this.toast.showError('ليس لديك صلاحية للوصول إلى هذا المورد.');
          } else if (error.status >= 500) {
            this.toast.showError('خطأ في الخادم. حاول مرة أخرى لاحقًا.');
          } else if (error.status === 0) {
            this.toast.showError('لا يمكن الاتصال بالخادم. تحقق من اتصالك بالإنترنت.');
          }
        }
        
        return throwError(() => error);
      })
    );
  }
}