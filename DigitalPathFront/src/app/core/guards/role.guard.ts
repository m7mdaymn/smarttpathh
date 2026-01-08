import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const user = authService.user();
  const requiredRole = route.data['role'];

  console.log(`🛡️ [RoleGuard] Checking access for role: ${requiredRole}`);
  console.log(`👤 [RoleGuard] Current user role: ${user?.role}`);

  // 1. Check if user has the required role
  if (user && user.role === requiredRole) {
    // For merchants, check subscription status
    if (user.role === 'merchant') {
      const subscriptionStatus = (user as any).subscriptionStatus;
      const targetPath = state.url;
      
      console.log(`📋 [RoleGuard] Merchant subscription status: ${subscriptionStatus}`);
      
      // If merchant is inactive and trying to access anything except inactive page
      if (subscriptionStatus !== 'active' && !targetPath.includes('/merchant/inactive')) {
        console.warn('⛔ [RoleGuard] Merchant inactive, redirecting to inactive page');
        return router.createUrlTree(['/merchant/inactive']);
      }
      
      // If merchant is active and trying to access inactive page, redirect to dashboard
      if (subscriptionStatus === 'active' && targetPath.includes('/merchant/inactive')) {
        console.log('✅ [RoleGuard] Merchant active, redirecting to dashboard');
        return router.createUrlTree(['/merchant/dashboard']);
      }
    }
    return true;
  }

  // 2. If not authenticated at all, redirect to signin
  if (!user) {
    console.warn('⛔ [RoleGuard] No user found, redirecting to signin');
    return router.createUrlTree(['/auth/signin']);
  }

  // 3. User is authenticated but has WRONG role -> Redirect to their own dashboard
  console.warn(`⛔ [RoleGuard] Role mismatch! Required: ${requiredRole}, Found: ${user.role}`);

  if (user.role === 'customer') {
    return router.createUrlTree(['/customer/dashboard']);
  } else if (user.role === 'merchant') {
    // Check subscription status for redirect
    const subscriptionStatus = (user as any).subscriptionStatus;
    if (subscriptionStatus !== 'active') {
      return router.createUrlTree(['/merchant/inactive']);
    }
    return router.createUrlTree(['/merchant/dashboard']);
  } else if (user.role === 'superadmin') {
    return router.createUrlTree(['/superadmin/dashboard']);
  }

  // Fallback
  return router.createUrlTree(['/']);
};