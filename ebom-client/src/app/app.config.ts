import { ApplicationConfig, importProvidersFrom, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { JwtModule } from '@auth0/angular-jwt';
import { JwtInterceptor } from './core/auth/interceptors/jwt.interceptor';
import { entityReducer } from './store/entities/entity.reducer';
import { EntityEffects } from './store/entities/entity.effects';

import { routes } from './app.routes';

export function tokenGetter() {
  return localStorage.getItem('access_token');
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideAnimations(),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
    }),
    provideStore({
      entities: entityReducer
    }),
    provideEffects([EntityEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode()
    }),
    importProvidersFrom(
      JwtModule.forRoot({
        config: {
          tokenGetter: tokenGetter,
          allowedDomains: ['localhost:7001', 'localhost:5000'],
          disallowedRoutes: ['localhost:7001/api/auth', 'localhost:5000/api/auth']
        }
      })
    ),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ]
};
