import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, tap, map } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../../environments/environment';
import { User, LoginRequest, AuthResponse, RefreshTokenRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  constructor(
    private http: HttpClient,
    private router: Router,
    private jwtHelper: JwtHelperService
  ) {
    // Check if user is already logged in
    const token = this.getToken();
    if (token && !this.jwtHelper.isTokenExpired(token)) {
      const user = this.getUserFromToken(token);
      this.currentUserSubject.next(user);
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.storeTokens(response);
          const user = this.getUserFromToken(response.accessToken);
          this.currentUserSubject.next(user);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refresh_token');
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.storeTokens(response);
          const user = this.getUserFromToken(response.accessToken);
          this.currentUserSubject.next(user);
        })
      );
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return token ? !this.jwtHelper.isTokenExpired(token) : false;
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private storeTokens(response: AuthResponse): void {
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
  }

  private getUserFromToken(token: string): User | null {
    try {
      const decoded = this.jwtHelper.decodeToken(token);
      return {
        id: decoded.sub,
        userName: decoded.name,
        email: decoded.email,
        roles: decoded.roles || []
      };
    } catch (error) {
      return null;
    }
  }
}