import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'entities',
        loadComponent: () => import('./features/entities/entity-list/entity-list.component').then(m => m.EntityListComponent)
      },
      {
        path: 'entities/:id',
        loadComponent: () => import('./features/entities/entity-detail/entity-detail.component').then(m => m.EntityDetailComponent)
      },
      {
        path: 'templates/upload',
        loadComponent: () => import('./features/templates/template-upload/template-upload.component').then(m => m.TemplateUploadComponent)
      },
      {
        path: 'data/upload',
        loadComponent: () => import('./features/data/data-upload-wizard/data-upload-wizard.component').then(m => m.DataUploadWizardComponent)
      },
      {
        path: 'data/upload/:entityId',
        loadComponent: () => import('./features/data/data-upload-wizard/data-upload-wizard.component').then(m => m.DataUploadWizardComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
