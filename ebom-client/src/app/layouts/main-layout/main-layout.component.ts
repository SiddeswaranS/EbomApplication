import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { DxButtonModule, DxDropDownButtonModule } from 'devextreme-angular';
import { AuthService } from '../../core/auth/services/auth.service';
import { User } from '../../core/auth/models/auth.models';

interface MenuItem {
  title: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet, DxButtonModule, DxDropDownButtonModule],
  template: `
    <div class="min-h-screen bg-gray-100">
      <!-- Header -->
      <header class="bg-white shadow-sm border-b">
        <div class="px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between items-center h-16">
            <div class="flex items-center">
              <button 
                class="p-2 rounded-md lg:hidden"
                (click)="toggleSidebar()">
                <i class="dx-icon-menu text-xl"></i>
              </button>
              <h1 class="ml-4 text-xl font-semibold">EBOM System</h1>
            </div>
            
            <div class="flex items-center space-x-4">
              <!-- Notifications -->
              <dx-button
                icon="bell"
                stylingMode="text"
                [hint]="'Notifications'"
                [elementAttr]="{ class: 'notification-button' }">
                <div *dxTemplate="let data of 'content'">
                  <span class="relative">
                    <i class="dx-icon-bell text-xl"></i>
                    <span *ngIf="notificationCount > 0" 
                      class="absolute -top-1 -right-1 h-5 w-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
                      {{ notificationCount }}
                    </span>
                  </span>
                </div>
              </dx-button>
              
              <!-- User Menu -->
              <dx-drop-down-button
                [items]="userMenuItems"
                [text]="currentUser?.userName || 'User'"
                icon="user"
                stylingMode="text"
                (onItemClick)="onUserMenuClick($event)"
                [dropDownOptions]="{ width: 200 }">
              </dx-drop-down-button>
            </div>
          </div>
        </div>
      </header>

      <div class="flex">
        <!-- Sidebar -->
        <aside 
          class="w-64 bg-gray-800 min-h-screen transition-all duration-300"
          [class.hidden]="!sidebarOpen"
          [class.lg:block]="true">
          <nav class="mt-5 px-2">
            <a *ngFor="let item of menuItems"
              [routerLink]="item.path"
              routerLinkActive="bg-gray-900"
              class="group flex items-center px-2 py-2 text-sm font-medium rounded-md text-gray-300 hover:bg-gray-700 hover:text-white transition-colors">
              <i [class]="item.icon + ' mr-3 text-lg'"></i>
              {{ item.title }}
            </a>
          </nav>
        </aside>

        <!-- Main Content -->
        <main class="flex-1">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
    
    .notification-button ::ng-deep .dx-button-content {
      padding: 0;
    }
  `]
})
export class MainLayoutComponent implements OnInit {
  sidebarOpen = true;
  currentUser: User | null = null;
  notificationCount = 3;
  
  menuItems: MenuItem[] = [
    { title: 'Dashboard', path: '/dashboard', icon: 'dx-icon-chart' },
    { title: 'Entities', path: '/entities', icon: 'dx-icon-folder' },
    { title: 'Template Upload', path: '/templates/upload', icon: 'dx-icon-upload' },
    { title: 'Data Upload', path: '/data/upload', icon: 'dx-icon-import' },
    { title: 'Reports', path: '/reports', icon: 'dx-icon-doc' },
    { title: 'Settings', path: '/settings', icon: 'dx-icon-preferences' }
  ];

  userMenuItems = [
    { text: 'Profile', icon: 'user' },
    { text: 'Settings', icon: 'preferences' },
    { beginGroup: true, text: 'Logout', icon: 'runner' }
  ];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  onUserMenuClick(e: any): void {
    if (e.itemData.text === 'Logout') {
      this.authService.logout();
    }
  }
}