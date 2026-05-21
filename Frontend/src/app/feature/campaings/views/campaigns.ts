import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { Store } from '../signals/store';
import { Card } from '../components/card';
import { Auth } from '../../../core/services/auth'; // Inyectamos Auth
import { Router, RouterLink } from '@angular/router';

@Component({
    selector: 'app-campaigns',
    standalone: true,
    imports: [Card, RouterLink],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <section class="min-h-screen bg-gray-50 p-8">
      <header class="mb-8 flex justify-between items-end">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Gestión de Campañas</h1>
          <p class="text-gray-600 mt-2">
            Total activas: <span class="font-semibold text-green-800">{{ store.activeCount() }}</span>
          </p>
        </div>
        
        <div class="flex gap-4">
          <a routerLink="new" class="px-4 py-2 text-sm font-medium text-white bg-green-800 hover:bg-green-900 rounded-lg transition-colors shadow-sm">
            + Nueva Campaña
          </a>
          <button (click)="onLogout()" class="px-4 py-2 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-lg transition-colors border border-red-200">
            Cerrar Sesión
          </button>
        </div>
      </header>

      @if (store.error()) {
        <div class="bg-red-50 border-l-4 border-red-600 p-4 rounded-md text-red-800">
          <p class="font-medium">Error del sistema</p>
          <p class="text-sm">{{ store.error() }}</p>
        </div>
      } @else if (store.isLoading()) {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (i of [1,2,3,4,5,6]; track i) {
            <div class="h-48 bg-gray-200 rounded-xl animate-pulse"></div>
          }
        </div>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (campaign of store.campaigns(); track campaign.campId) {
            @defer (on viewport) {
              <app-campaign-card 
                [data]="campaign" 
                (edit)="onEdit($event)"
                (delete)="onDelete($event)" /> } @placeholder {
              <div class="h-48 bg-gray-100 rounded-xl border border-gray-200"></div>
            }
          } @empty {
            <div class="col-span-full text-center py-12 bg-white rounded-xl border border-dashed border-gray-300">
              <p class="text-gray-500">No hay campañas registradas en el sistema.</p>
            </div>
          }
        </div>
      }

      </section>
  `
})
export class Campaigns implements OnInit {
    protected readonly store = inject(Store);
    private readonly authService = inject(Auth); // Instanciamos
    private readonly router = inject(Router);

    ngOnInit() {
        this.store.load();
    }

    onLogout() {
        this.authService.logout();
    }

    onEdit(id: string) {
        this.router.navigate(['/campaigns/edit', id]);
    }

    // Agrega esto si decides poner un botón de eliminar en la tarjeta (Card)
    onDelete(id: string) {
        if (confirm('¿Seguro que deseas inactivar/eliminar esta campaña?')) {
            const currentUser = 'JOSIAE_AC'; // Idealmente sale del Auth Service
            this.store.removeCampaign(id, currentUser);
        }
    }
}