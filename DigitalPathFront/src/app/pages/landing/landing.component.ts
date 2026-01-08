import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { trigger, transition, style, animate, stagger, query, keyframes } from '@angular/animations';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css'],
  animations: [
    // Hero text animation
    trigger('heroTextAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(30px)' }),
        animate('1s 0.5s ease-out', 
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),

    // Feature cards animation
    trigger('featureAnimation', [
      transition(':enter', [
        query('.feature-card', [
          style({ opacity: 0, transform: 'translateY(50px)' }),
          stagger(200, [
            animate('0.8s cubic-bezier(0.35, 0, 0.25, 1)', 
              style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ])
      ])
    ]),

    // Fade in animation
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('0.8s 0.3s ease-out', style({ opacity: 1 }))
      ])
    ]),

    // Slide in from left
    trigger('slideInLeft', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-50px)' }),
        animate('0.8s 0.5s ease-out', 
          style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),

    // Slide in from right
    trigger('slideInRight', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(50px)' }),
        animate('0.8s 0.5s ease-out', 
          style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),

    // Button hover animation
    trigger('buttonHover', [
      transition('* => hover', [
        animate('0.3s ease-out', 
          keyframes([
            style({ transform: 'scale(1)', offset: 0 }),
            style({ transform: 'scale(1.05)', offset: 0.5 }),
            style({ transform: 'scale(1.03)', offset: 1 })
          ])
        )
      ])
    ])
  ]
})
export class LandingComponent implements OnInit {
  isScrolled = false;
  hoverState: { [key: string]: boolean } = {};

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    // إذا كان المستخدم مسجل دخول، أعده إلى الصفحة الرئيسية المناسبة
    if (this.authService.isAuthenticated()) {
      const userRole = this.authService.user()?.role || 'customer';
      if (userRole === 'merchant') {
        this.router.navigate(['/merchant/dashboard']);
      } else if (userRole === 'superadmin') {
        this.router.navigate(['/superadmin/dashboard']);
      } else {
        this.router.navigate(['/customer/dashboard']);
      }
    }
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 50;
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  handleWhatsApp(): void {
    window.open('https://wa.me/966548290509?text=أريد الاشتراك في Digital Pass', '_blank');
  }

  setHoverState(key: string, state: boolean): void {
    this.hoverState[key] = state;
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}