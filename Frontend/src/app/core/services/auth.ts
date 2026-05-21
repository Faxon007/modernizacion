import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { Router } from '@angular/router'; // Importamos el router
import { ApiResponse } from '../models/api-response';
import { environment } from '../../../environments/environment';

export interface AuthResponse { accessToken: string; expiresAt: string; username: string; role: string; }
export interface LoginPayload { username: string; password: string; }

@Injectable({ providedIn: 'root' })
export class Auth {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);

    // 1. Inicializamos el Signal leyendo del sessionStorage para sobrevivir al F5
    private readonly _token = signal<string | null>(sessionStorage.getItem('prom_token'));
    private readonly _user = signal<string | null>(sessionStorage.getItem('prom_username'));
    readonly token = this._token.asReadonly();
    readonly username = this._user.asReadonly();
    readonly isAuthenticated = computed(() => this._token() !== null);

    login(payload: LoginPayload) {
        const url = `${environment.apiBase}/auth/token`;

        return this.http.post<ApiResponse<AuthResponse>>(url, payload).pipe(
            tap(response => {
                if (response.success && response.data) {
                    const jwt = response.data.accessToken;
                    const user = response.data.username;
                    this._token.set(jwt);
                    this._user.set(user);
                    // 2. Guardamos físicamente en la sesión
                    sessionStorage.setItem('prom_token', jwt);
                    sessionStorage.setItem('prom_username', user);
                }
            })
        );
    }

    logout(): void {
        // 3. Limpiamos memoria, almacenamiento y redirigimos
        this._token.set(null);
        sessionStorage.removeItem('prom_token');
        this.router.navigate(['/login']);
    }
}