import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { UiService } from '../services/ui.service';
import { Auth } from '../services/auth';

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const uiService = inject(UiService);
  const authService = inject(Auth);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Si el error es 401 y NO es la pantalla de login, entonces es una sesión expirada.
      if (error.status === 401 && !req.url.includes('/api/auth/login')) {
        // Si el error es 401 (No autorizado), la sesión ha expirado.
        return uiService.showModal('Tu sesión ha expirado. Por favor, inicia sesión de nuevo para continuar.').pipe(
          switchMap(() => {
            authService.logout(); // Limpiamos sesión y redirigimos
            return throwError(() => error); // Propagamos el error original
          })
        );
      }

      // Para cualquier otro error, simplemente lo propagamos.
      return throwError(() => error);
    })
  );
};