import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'auth',
    children: [
      {
        path: 'selection',
        loadComponent: () => import('./pages/auth/selection/selection.component').then(m => m.SelectionComponent)
      },
      {
        path: 'signin',
        loadComponent: () => import('./pages/auth/signin/signin.component').then(m => m.SigninComponent)
      },
      {
        path: 'customer-register',
        loadComponent: () => import('./pages/auth/customer-register/customer-register.component').then(m => m.CustomerRegisterComponent)
      },
      {
        path: 'merchant-register',
        loadComponent: () => import('./pages/auth/merchant-register/merchant-register.component').then(m => m.MerchantRegisterComponent)
      },
      {
        path: 'register/:merchantId',
        loadComponent: () => import('./pages/auth/customer-register-merchant/customer-register-merchant.component').then(m => m.CustomerRegisterMerchantComponent)
      }
    ]
  },
  {
    path: 'customer',
    canActivate: [authGuard, roleGuard],
    data: { role: 'customer' },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/Customer/customer-dashboard/customer-dashboard.component').then(m => m.CustomerDashboardComponent)
      }
    ]
  },
  {
    path: 'merchant',
    canActivate: [authGuard, roleGuard],
    data: { role: 'merchant' },
    children: [
      {
        path: 'inactive',
        loadComponent: () => import('./pages/Merchant/merchant-inactive/merchant-inactive.component').then(m => m.MerchantInactiveComponent)
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/Merchant/merchant-dashboard/merchant-dashboard.component').then(m => m.MerchantDashboardComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./pages/Merchant/merchant-customers/merchant-customers.component').then(m => m.MerchantCustomersComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./pages/Merchant/merchant-setting/merchant-setting.component').then(m => m.MerchantSettingComponent)
      },
      {
        path: 'scan-qr',
        loadComponent: () => import('./pages/Merchant/scan-qr/scan-qr.component').then(m => m.ScanQrComponent)
      }
    ]
  },
  {
    path: 'superadmin',
    canActivate: [authGuard, roleGuard],
    data: { role: 'superadmin' },
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/superadmin/super-admin-dashboard/super-admin-dashboard.component').then(m => m.SuperAdminDashboardComponent)
      },
      {
        path: 'merchant-details',
        loadComponent: () => import('./pages/superadmin/merchant-details/merchant-details.component').then(m => m.MerchantDetailsComponent)
      },
      {
        path: 'merchant-details/:id',
        loadComponent: () => import('./pages/superadmin/merchant-details/merchant-details.component').then(m => m.MerchantDetailsComponent)
      }
      //  {
      //   path: 'platform-settings',
      //   loadComponent: () => import('./pages/superadmin/platform-settings/platform-settings.component').then(m => m.PlatformSettingsComponent)
      // }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];