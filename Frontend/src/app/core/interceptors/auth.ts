import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Auth } from '../services/auth';

function generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = (Math.random() * 16) | 0;
        const v = c === 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(Auth);
    const token = authService.token();

    const headers: { [key: string]: string } = {
        'x-transaction-id': generateGuid(),
        'Strict-Transport-Security': 'max-age=31536000; includeSubDomains',
        'X-Frame-Options': 'DENY'
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    req = req.clone({ setHeaders: headers });

    return next(req);
};