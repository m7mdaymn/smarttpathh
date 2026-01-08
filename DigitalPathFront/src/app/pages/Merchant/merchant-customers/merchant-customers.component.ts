import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MerchantService, MerchantCustomer } from '../../../core/services/merchant.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/services/api.service';

@Component({
  selector: 'app-merchant-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './merchant-customers.component.html',
  styleUrls: ['./merchant-customers.component.css']
})
export class MerchantCustomersComponent implements OnInit {
  // Signals
  customers = signal<MerchantCustomer[]>([]);
  selectedCustomer = signal<MerchantCustomer | null>(null);
  isLoading = signal(false);
  
  // Plain property for ngModel binding
  searchQueryValue = '';
  
  // Get filtered customers as a getter method (called in template)
  get filteredCustomers(): MerchantCustomer[] {
    const query = this.searchQueryValue.toLowerCase();
    if (!query) return this.customers();
    
    return this.customers().filter(c => 
      c.name.toLowerCase().includes(query) ||
      c.email.toLowerCase().includes(query) ||
      c.phone.includes(query)
    );
  }

  // Get active customers count
  get activeCustomersCount(): number {
    return this.customers().filter(c => c.status === 'active').length;
  }

  // Modal states
  showAddModal = false;
  showEditModal = false;
  showConfirmModal = false;
  confirmAction: 'delete' | 'deactivate' | 'activate' | null = null;
  confirmMessage = '';

  // Form
  customerForm: FormGroup;
  merchantId: string | null = null;

  constructor(
    private merchantService: MerchantService,
    private authService: AuthService,
    private toast: ToastService,
    private http: HttpClient,
    private fb: FormBuilder
  ) {
    this.customerForm = this.fb.group({
      id: [''],
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{9,}$/)]],
      carPlate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const user = this.authService.user() as any;
    if (user?.id) {
      // Get merchantId from API
      this.http.get<ApiResponse<string>>(`${environment.apiUrl}/merchant/by-user/${user.id}`).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.merchantId = response.data;
            this.loadCustomers();
          } else {
            this.toast.showError('فشل في تحديد معرف المغسلة');
          }
        },
        error: () => {
          this.toast.showError('فشل في تحميل بيانات المغسلة');
        }
      });
    }
  }

  loadCustomers(): void {
    if (!this.merchantId) return;
    
    this.isLoading.set(true);
    this.merchantService.getCustomers(this.merchantId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.customers.set(response.data);
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.toast.showError('فشل في تحميل العملاء');
        this.isLoading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.customerForm.reset();
    this.showAddModal = true;
  }

  openEditModal(customer: MerchantCustomer): void {
    this.selectedCustomer.set(customer);
    this.customerForm.patchValue({
      id: customer.id,
      name: customer.name,
      email: customer.email,
      phone: customer.phone,
      carPlate: (customer as any).carPlate || ''
    });
    this.showEditModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.customerForm.reset();
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.customerForm.reset();
    this.selectedCustomer.set(null);
  }

  closeConfirmModal(): void {
    this.showConfirmModal = false;
    this.confirmAction = null;
  }

  saveCustomer(): void {
    if (!this.merchantId || this.customerForm.invalid) {
      this.toast.showError('يرجى ملء جميع الحقول بشكل صحيح');
      return;
    }

    const isEditing = this.selectedCustomer();
    const formValue = this.customerForm.value;

    if (isEditing) {
      // Update existing customer
      const updateData = {
        name: formValue.name,
        email: formValue.email,
        phone: formValue.phone
      };

      this.http.put<ApiResponse<MerchantCustomer>>(
        `${environment.apiUrl}/merchant/${this.merchantId}/customers/${formValue.id}`,
        updateData
      ).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.showSuccess('تم تحديث بيانات العميل بنجاح');
            this.closeEditModal();
            this.loadCustomers();
          }
        },
        error: () => {
          this.toast.showError('فشل في تحديث بيانات العميل');
        }
      });
    } else {
      // Add new customer
      const newCustomerData = {
        name: formValue.name,
        email: formValue.email,
        phone: formValue.phone,
        plateNumber: formValue.carPlate
      };

      this.merchantService.addCustomer(this.merchantId, newCustomerData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.showSuccess('تم إضافة العميل بنجاح');
            this.closeAddModal();
            this.loadCustomers();
          }
        },
        error: () => {
          this.toast.showError('فشل في إضافة العميل');
        }
      });
    }
  }

  confirmDeactivate(customer: MerchantCustomer): void {
    this.selectedCustomer.set(customer);
    this.confirmAction = 'deactivate';
    this.confirmMessage = `هل أنت متأكد من تعطيل العميل ${customer.name}؟`;
    this.showConfirmModal = true;
  }

  confirmActivate(customer: MerchantCustomer): void {
    this.selectedCustomer.set(customer);
    this.confirmAction = 'activate';
    this.confirmMessage = `هل أنت متأكد من تفعيل العميل ${customer.name}؟`;
    this.showConfirmModal = true;
  }

  confirmDelete(customer: MerchantCustomer): void {
    this.selectedCustomer.set(customer);
    this.confirmAction = 'delete';
    this.confirmMessage = `هل أنت متأكد من حذف العميل ${customer.name}؟ سيتم حذف جميع بيانات العميل والمعاملات.`;
    this.showConfirmModal = true;
  }

  executeConfirm(): void {
    if (!this.merchantId || !this.selectedCustomer()) {
      return;
    }

    const customer = this.selectedCustomer()!;
    const customerId = customer.id;

    if (this.confirmAction === 'delete') {
      this.http.delete<ApiResponse<boolean>>(
        `${environment.apiUrl}/merchant/${this.merchantId}/customers/${customerId}`
      ).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.showSuccess('تم حذف العميل بنجاح');
            this.closeConfirmModal();
            this.loadCustomers();
          }
        },
        error: () => {
          this.toast.showError('فشل في حذف العميل');
        }
      });
    } else if (this.confirmAction === 'deactivate') {
      this.http.put<ApiResponse<MerchantCustomer>>(
        `${environment.apiUrl}/merchant/${this.merchantId}/customers/${customerId}/deactivate`,
        {}
      ).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.showSuccess('تم تعطيل العميل بنجاح');
            this.closeConfirmModal();
            this.loadCustomers();
          }
        },
        error: () => {
          this.toast.showError('فشل في تعطيل العميل');
        }
      });
    } else if (this.confirmAction === 'activate') {
      this.http.put<ApiResponse<MerchantCustomer>>(
        `${environment.apiUrl}/merchant/${this.merchantId}/customers/${customerId}/activate`,
        {}
      ).subscribe({
        next: (response) => {
          if (response.success) {
            this.toast.showSuccess('تم تفعيل العميل بنجاح');
            this.closeConfirmModal();
            this.loadCustomers();
          }
        },
        error: () => {
          this.toast.showError('فشل في تفعيل العميل');
        }
      });
    }
  }

  getStatusBadgeClass(customer: MerchantCustomer): string {
    return customer.status === 'active' ? 'badge-success' : 'badge-danger';
  }

  getStatusText(customer: MerchantCustomer): string {
    return customer.status === 'active' ? '✅ نشط' : '❌ معطل';
  }
}
