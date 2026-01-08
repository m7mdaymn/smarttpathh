import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastService } from './core/services/toast.service';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <!-- Toast Container -->
    <div class="toast-container">
      <div *ngFor="let toast of toasts" 
           class="toast {{toast.type}}"
           [ngClass]="{'fade-out': toast.duration && toast.duration <= 500}">
        <div class="toast-icon">
          <span *ngIf="toast.type === 'success'">✓</span>
          <span *ngIf="toast.type === 'error'">✗</span>
          <span *ngIf="toast.type === 'info'">ℹ</span>
          <span *ngIf="toast.type === 'warning'">⚠</span>
        </div>
        <div class="toast-message">{{ toast.message }}</div>
        <button class="toast-close" (click)="removeToast(toast.id)">×</button>
      </div>
    </div>
    
    <router-outlet></router-outlet>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 10000;
    }
    
    .toast {
      display: flex;
      align-items: center;
      padding: 1rem 1.5rem;
      margin-bottom: 0.75rem;
      border-radius: 10px;
      box-shadow: 0 5px 15px rgba(0,0,0,0.1);
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255,255,255,0.2);
      animation: slideIn 0.3s ease-out;
      max-width: 350px;
    }
    
    .toast.success {
      background: rgba(46, 204, 113, 0.9);
      color: white;
    }
    
    .toast.error {
      background: rgba(231, 76, 60, 0.9);
      color: white;
    }
    
    .toast.info {
      background: rgba(52, 152, 219, 0.9);
      color: white;
    }
    
    .toast.warning {
      background: rgba(241, 196, 15, 0.9);
      color: white;
    }
    
    .toast-icon {
      margin-right: 0.75rem;
      font-size: 1.2rem;
    }
    
    .toast-message {
      flex: 1;
      font-size: 0.9rem;
    }
    
    .toast-close {
      background: none;
      border: none;
      color: white;
      font-size: 1.2rem;
      cursor: pointer;
      opacity: 0.7;
      transition: opacity 0.2s;
      padding: 0;
      margin-left: 1rem;
    }
    
    .toast-close:hover {
      opacity: 1;
    }
    
    .fade-out {
      opacity: 0;
      transform: translateX(100%);
      transition: all 0.3s ease-out;
    }
    
    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateX(100%);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }
  `]
})
export class AppComponent {
  private destroy$ = new Subject<void>();
  toasts: Array<any & { id: number }> = [];
  private toastId = 0;

  constructor(private toastService: ToastService) {
    this.toastService.toast$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(toast => {
      const toastWithId = { ...toast, id: ++this.toastId };
      this.toasts.push(toastWithId);
      
      if (toast.duration) {
        setTimeout(() => {
          this.removeToast(toastWithId.id);
        }, toast.duration);
      }
    });
  }

  removeToast(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}