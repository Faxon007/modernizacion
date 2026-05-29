import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { UiService } from '../../../../core/services/ui.service';
import { Auth as AuthService } from '../../../../core/services/auth';
import { MenuService } from '../../../../core/services/menu.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
    <header class="bg-white border-b px-6 py-4 flex justify-between items-center print:hidden">
      <img src="assets/images/logoN.png" alt="Logo" class="h-12">
      <div class="flex items-center gap-4">
        <span class="text-[#007139] text-xl font-semibold">{{ ui.title() }}</span>
        <button (click)="logout()" class="text-sm text-red-600 hover:underline">Cerrar Sesión</button>
      </div>
    </header>

<!-- Barra de Navegación -->
<nav class="bg-[#007139] sticky top-0 z-50 shadow-lg print:hidden">
  <div class="h-1 bg-[#7bc342]"></div>
  <div class="container mx-auto flex">
    @for (item of menu.navItems(); track item.id) {
      <div class="group relative">
        <!-- Botón Principal -->
        <button class="px-6 py-4 text-white hover:bg-[#7bc342]/20 transition-all duration-300 flex items-center gap-2 font-medium">
          {{ item.nombre }}
          @if (item.children?.length) { 
            <span class="text-[10px] opacity-70 group-hover:rotate-180 transition-transform duration-300">▼</span> 
          }
        </button>

        <!-- Menú Desplegable -->
        @if (item.children?.length) {
          <div class="absolute hidden group-hover:block pt-1 left-0 min-w-[280px] animate-in fade-in slide-in-from-top-2 duration-200">
            <ul class="bg-white rounded-xl shadow-2xl border-2 border-[#7bc342] overflow-hidden">
              @for (sub of item.children; track sub.id) {
                <li>
                  <a [routerLink]="['/' + sub.path.replace('.aspx', '')]" 
                     class="block px-5 py-4 text-gray-700 hover:bg-[#7bc342]/10 hover:text-[#007139] transition-colors border-b border-gray-100 last:border-0">
                    <div class="flex flex-col">
                      <span class="font-bold text-sm uppercase tracking-wide">{{ sub.nombre }}</span>
                      <small class="text-gray-500 mt-1 leading-tight">{{ sub.descripcion }}</small>
                    </div>
                  </a>
                </li>
              }
            </ul>
          </div>
        }
      </div>
    }
  </div>
</nav>

    <div class="container mx-auto mt-4 px-4">
      @if (ui.success()) {
        <div class="bg-blue-100 border-l-4 border-blue-500 text-blue-700 p-4 mb-4 flex justify-between">
          <span>{{ ui.success() }}</span>
          <button (click)="ui.success.set(null)">×</button>
        </div>
      }
    </div>

    <main class="container mx-auto p-4">
      <router-outlet />
    </main>

    @if (ui.modalError()) {
      <div class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 animate-in fade-in duration-200">
        <div class="bg-white rounded-2xl shadow-2xl max-w-lg w-full border border-gray-100 overflow-hidden animate-in zoom-in-95 duration-200">
          <div class="bg-red-500 px-6 py-4 flex items-center gap-3">
            <span class="text-white text-xl">⚠️</span>
            <h3 class="text-white font-bold text-lg">Notificación de Error</h3>
          </div>
          <div class="p-6 text-gray-700 font-medium">
            {{ ui.modalError() }}
          </div>
          <div class="bg-gray-50 px-6 py-4 text-right border-t border-gray-100">
            <button (click)="ui.closeModal()" class="bg-white border border-gray-300 hover:bg-gray-100 text-gray-700 font-semibold py-2 px-6 rounded-xl shadow-sm transition-colors">
              Cerrar
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class LayoutComponent implements OnInit {
  protected readonly menu = inject(MenuService);
  protected readonly ui = inject(UiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

ngOnInit() {
  const user = this.auth.username();
  if (user) {
    this.menu.fetchMenu(user);
  }
}

  logout() {
    this.auth.logout();
  }
}
