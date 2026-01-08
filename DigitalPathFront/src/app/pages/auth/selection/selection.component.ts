import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { trigger, transition, style, animate, stagger, query } from '@angular/animations';

@Component({
  selector: 'app-selection',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './selection.component.html',
  styleUrls: ['./selection.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.6s ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('staggerAnimation', [
      transition(':enter', [
        query('.selection-card', [
          style({ opacity: 0, transform: 'scale(0.9)' }),
          stagger(200, [
            animate('0.5s cubic-bezier(0.35, 0, 0.25, 1)', 
              style({ opacity: 1, transform: 'scale(1)' }))
          ])
        ])
      ])
    ])
  ]
})
export class SelectionComponent {
  constructor(private router: Router) {}

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}