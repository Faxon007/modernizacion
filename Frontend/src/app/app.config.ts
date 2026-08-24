import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { mockInterceptor } from './core/interceptors/mock.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    // Usamos provideAnimationsAsync, que es la forma más moderna y recomendada.
    provideAnimationsAsync(),
    // Registramos los interceptores funcionales. El orden importa.
    provideHttpClient(withInterceptors([mockInterceptor, authInterceptor, errorInterceptor])),
  ],
};