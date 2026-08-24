import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LinkService } from '../../core/services/link.service';
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-activacion',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="max-w-xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4">
        <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Activación Manual de Links</h1>
        <p class="text-gray-500 text-sm mt-1">Busque el código de link (SKU) para verificar y reactivar su estatus.</p>
      </div>

      <!-- Buscar Card -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 space-y-4">
        <form [formGroup]="searchForm" (ngSubmit)="onSearch()" class="space-y-4">
          <div class="space-y-1">
            <label for="sku" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Código del Link (SKU)</label>
            <div class="flex gap-2">
              <input 
                id="sku" 
                type="text" 
                formControlName="sku"
                placeholder="Ingrese el SKU del link"
                class="flex-1 px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-semibold font-mono">
              <button 
                type="submit" 
                [disabled]="searchForm.invalid || isSearching()"
                class="px-6 py-3 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center gap-2">
                @if (isSearching()) {
                  <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                  <span>Buscando...</span>
                } @else {
                  <span>🔎 Buscar</span>
                }
              </button>
            </div>
          </div>
        </form>
      </div>

      <!-- Resultados / Detalles del Estatus -->
      @if (linkInfo()) {
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
          <div class="bg-gradient-to-r from-gray-800 to-gray-900 px-6 py-4 text-white flex justify-between items-center">
            <h3 class="font-bold text-sm uppercase tracking-wider">Información del Enlace</h3>
            <span class="font-mono text-xs opacity-75">SKU: {{ searchForm.value.sku }}</span>
          </div>
          
          <div class="p-6 space-y-6">
            <div class="grid grid-cols-2 gap-4 text-sm">
              <div class="bg-gray-50 p-4 rounded-xl">
                <span class="text-xs font-bold text-gray-400 block uppercase">Nombre del Producto</span>
                <span class="font-semibold text-gray-800 mt-1 block">{{ linkInfo().nombre || 'N/A' }}</span>
              </div>
              <div class="bg-gray-50 p-4 rounded-xl">
                <span class="text-xs font-bold text-gray-400 block uppercase">Monto</span>
                <span class="font-semibold text-gray-800 mt-1 block font-mono">Q. {{ linkInfo().precio || '0.00' }}</span>
              </div>
              <div class="bg-gray-50 p-4 rounded-xl col-span-2">
                <span class="text-xs font-bold text-gray-400 block uppercase">Estatus en Neo</span>
                <div class="flex items-center gap-2 mt-2">
                  @if (linkInfo().activo === 'SI') {
                    <span class="px-2.5 py-1 bg-emerald-50 text-emerald-700 border border-emerald-200 rounded-full text-xs font-bold uppercase">
                      Activo / Vigente
                    </span>
                  } @else {
                    <span class="px-2.5 py-1 bg-red-50 text-red-700 border border-red-200 rounded-full text-xs font-bold uppercase">
                      Inactivo / Expirado
                    </span>
                  }
                </div>
              </div>
            </div>

            <!-- Acciones -->
            <div class="border-t pt-6 flex justify-end gap-3">
              <button 
                type="button" 
                (click)="cancel()"
                class="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold rounded-xl transition-all">
                Cerrar
              </button>
              
              @if (linkInfo().activo !== 'SI') {
                <button 
                  type="button"
                  (click)="activateLink()"
                  [disabled]="isActivating()"
                  class="px-6 py-2.5 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center gap-2">
                  @if (isActivating()) {
                    <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    <span>Activando...</span>
                  } @else {
                    <span>⚡ Activar Link</span>
                  }
                </button>
              }
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class ActivacionComponent {
  private readonly fb = inject(FormBuilder);
  private readonly linkService = inject(LinkService);
  private readonly ui = inject(UiService);
  private readonly router = inject(Router);

  isSearching = signal(false);
  isActivating = signal(false);
  linkInfo = signal<any | null>(null);

  readonly searchForm = this.fb.group({
    sku: ['', [Validators.required]]
  });

  constructor() {
    this.ui.title.set('Activación de Links de Pago');
  }

  onSearch() {
    if (this.searchForm.invalid) return;
    this.isSearching.set(true);
    this.linkInfo.set(null);

    const sku = this.searchForm.value.sku!;

    this.linkService.validarYConsultaLink(sku).subscribe({
      next: (res) => {
        this.isSearching.set(false);
        if (res.success && res.data) {
          this.linkInfo.set(res.data);
          
          // Auto-activation if inactive, matching the legacy code behavior
          if (res.data.activo === 'NO') {
            this.activateLink();
          } else {
            this.ui.showSuccess('El link consultado ya se encuentra activo.');
          }
        } else {
          this.ui.showError(res.errorMessage || 'Link no encontrado en Neo.');
        }
      },
      error: () => {
        this.isSearching.set(false);
        this.ui.showError('Error al realizar la consulta del link.');
      }
    });
  }

  activateLink() {
    this.isActivating.set(true);
    const sku = this.searchForm.value.sku!;

    this.linkService.updateEstadoLink(sku).subscribe({
      next: (res) => {
        this.isActivating.set(false);
        if (res.success) {
          this.ui.showSuccess('Link activado exitosamente.');
          // Refresh status
          if (this.linkInfo()) {
            this.linkInfo.set({ ...this.linkInfo(), activo: 'SI' });
          }
        } else {
          this.ui.showError('No se pudo activar el link en la base de datos.');
        }
      },
      error: () => {
        this.isActivating.set(false);
        this.ui.showError('Error al activar el link.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/home']);
  }
}
