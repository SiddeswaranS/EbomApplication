import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { DxFormModule, DxButtonModule, DxTextBoxModule } from 'devextreme-angular';
import { AuthService } from '../../../core/auth/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoginRequest } from '../../../core/auth/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DxFormModule, DxButtonModule, DxTextBoxModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <h2 class="mt-6 text-3xl font-extrabold text-gray-900">
            Sign in to EBOM System
          </h2>
          <p class="mt-2 text-sm text-gray-600">
            Engineering Bill of Materials Management
          </p>
        </div>
        
        <div class="mt-8 bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <dx-form 
            [formData]="loginForm"
            [showValidationSummary]="true"
            [showColonAfterLabel]="false"
            labelLocation="top">
            
            <dxi-item 
              dataField="email"
              [label]="{ text: 'Email Address' }"
              editorType="dxTextBox"
              [editorOptions]="{ 
                placeholder: 'Enter your email',
                mode: 'email',
                stylingMode: 'outlined',
                elementAttr: { class: 'w-full' }
              }">
              <dxi-validation-rule type="required" message="Email is required"></dxi-validation-rule>
              <dxi-validation-rule type="email" message="Invalid email format"></dxi-validation-rule>
            </dxi-item>
            
            <dxi-item 
              dataField="password"
              [label]="{ text: 'Password' }"
              editorType="dxTextBox"
              [editorOptions]="{ 
                placeholder: 'Enter your password',
                mode: 'password',
                stylingMode: 'outlined',
                elementAttr: { class: 'w-full' }
              }">
              <dxi-validation-rule type="required" message="Password is required"></dxi-validation-rule>
              <dxi-validation-rule type="stringLength" [min]="6" message="Password must be at least 6 characters"></dxi-validation-rule>
            </dxi-item>
            
            <dxi-item itemType="button" 
              [buttonOptions]="{
                text: loading ? 'Signing in...' : 'Sign In',
                type: 'default',
                useSubmitBehavior: true,
                onClick: login.bind(this),
                disabled: loading,
                width: '100%',
                height: '44px',
                stylingMode: 'contained',
                elementAttr: { class: 'mt-6' }
              }">
            </dxi-item>
          </dx-form>

          <div class="mt-6">
            <div class="relative">
              <div class="absolute inset-0 flex items-center">
                <div class="w-full border-t border-gray-300"></div>
              </div>
              <div class="relative flex justify-center text-sm">
                <span class="px-2 bg-white text-gray-500">Demo Credentials</span>
              </div>
            </div>

            <div class="mt-6 text-center text-sm text-gray-600">
              <p>Email: admin&#64;ebom.com</p>
              <p>Password: admin123</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class LoginComponent implements OnInit {
  loginForm: LoginRequest = {
    email: '',
    password: ''
  };
  
  loading = false;
  returnUrl: string = '/dashboard';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    // Get return url from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    
    // If already logged in, redirect
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  login(e: any): void {
    e.preventDefault();
    
    if (!this.loginForm.email || !this.loginForm.password) {
      this.notify.error('Please fill in all fields');
      return;
    }

    this.loading = true;
    
    this.authService.login(this.loginForm).subscribe({
      next: () => {
        this.notify.success('Login successful');
        this.router.navigate([this.returnUrl]);
      },
      error: (error) => {
        this.loading = false;
        this.notify.error(error.error?.message || 'Login failed. Please try again.');
      }
    });
  }
}