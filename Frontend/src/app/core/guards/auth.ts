import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth';

export const authGuard: CanActivateFn = () => {
    const authService = inject(Auth);
    const router = inject(Router);

    // Leemos el valor del Signal computado
    if (authService.isAuthenticated()) {
        return true;
    }

    // Si no está autenticado, redirigimos usando parseUrl (más eficiente que navigate)
    return router.parseUrl('/login');
};