import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth as AuthService } from '../../../core/services/auth';
import { NgOptimizedImage } from '@angular/common';

@Component({
    selector: 'app-auth',
    standalone: true,
    imports: [ReactiveFormsModule, NgOptimizedImage],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <main class="min-h-screen bg-gray-50 flex flex-col justify-center items-center p-4">
      <section class="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 border-t-8 border-green-800">
        
        <header class="text-center mb-8">
          <img ngSrc="/grupo_promerica.svg" alt="Banco Promerica" width="200" height="60" class="mx-auto mb-4 object-contain">
          <h1 class="text-2xl font-bold text-gray-900">Banca Corporativa</h1>
          <p class="text-gray-500 text-sm mt-2">Ingresa tus credenciales para continuar</p>
        </header>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-6">
          
          <div>
            <label for="username" class="block text-sm font-medium text-gray-700 mb-1">Usuario</label>
            <input 
              id="username" 
              type="text" 
              formControlName="username"
              class="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-green-800 focus:border-green-800 transition-colors"
              placeholder="Ej: admin"
              autocomplete="username">
            
            @if (loginForm.controls.username.touched && loginForm.controls.username.invalid) {
              <p class="text-red-600 text-xs mt-1">El usuario es requerido.</p>
            }
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
            <input 
              id="password" 
              type="password" 
              formControlName="password"
              class="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-green-800 focus:border-green-800 transition-colors"
              placeholder="••••••••"
              autocomplete="current-password">
          </div>

          @if (errorMsg()) {
            <div class="bg-red-50 text-red-700 p-3 rounded-lg text-sm text-center">
              {{ errorMsg() }}
            </div>
          }

          <button 
            type="submit" 
            [disabled]="loginForm.invalid || isLoading()"
            class="w-full bg-green-800 text-white font-bold py-3 px-4 rounded-lg hover:bg-green-900 focus:ring-4 focus:ring-green-300 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex justify-center items-center">
            
            @if (isLoading()) {
              <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            } @else {
              <span>Ingresar Seguramente</span>
            }
          </button>
        </form>
      </section>
    </main>
  `
})
export class Auth {
    private readonly fb = inject(NonNullableFormBuilder);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    // Formulario Estrictamente Tipado (Cero `any`)
    readonly loginForm = this.fb.group({
        username: ['', [Validators.required]],
        password: ['', [Validators.required]]
    });

    // Estado local para la vista usando Signals
    readonly isLoading = signal(false);
    readonly errorMsg = signal<string | null>(null);

    onSubmit() {
        if (this.loginForm.invalid) return;

        this.isLoading.set(true);
        this.errorMsg.set(null);

        // Los valores ya están inferidos como strings gracias a NonNullableFormBuilder
        const payload = this.loginForm.getRawValue();

        this.authService.login(payload).subscribe({
            next: (res) => {
              console.log('--- RESPUESTA EXITOSA DEL BACKEND ---');
        console.log('Estructura completa:', res);
        console.log('¿Fue exitoso?:', res?.success);
        console.log('Mensaje de error si existe:', res?.errorMessage);
                if (res.success) {
                    // Si el token es válido, el sistema navega al dashboard de campañas
                    this.router.navigate(['/home']);
                } else {
                    this.errorMsg.set(res.errorMessage || 'Credenciales incorrectas');
                    this.isLoading.set(false);
                }
                
            },
            error: (err) => {
                const errorMessage = err?.error?.message || 'Error de conexión con el servidor.';
                this.errorMsg.set(errorMessage);
                this.isLoading.set(false);
            }
        });
    }
}