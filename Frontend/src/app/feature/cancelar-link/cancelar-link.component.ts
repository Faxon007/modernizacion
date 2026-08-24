import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LinkService, LinkCtaInfo } from '../../core/services/link.service';
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-cancelar-link',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="max-w-xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4">
        <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Desactivación Link Automático</h1>
        <p class="text-gray-500 text-sm mt-1">Busque y dé de baja programaciones automáticas de links de cobro.</p>
      </div>

      <!-- Tipo de Búsqueda Card -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 space-y-6">
        <div class="flex items-center gap-6 border-b pb-4">
          <label class="flex items-center gap-2 cursor-pointer font-semibold text-gray-700">
            <input 
              type="radio" 
              name="searchType" 
              value="cuenta" 
              [checked]="searchType() === 'cuenta'"
              (change)="setSearchType('cuenta')"
              class="w-4 h-4 text-[#007139] focus:ring-[#7bc342]">
            Búsqueda por producto
          </label>
          <label class="flex items-center gap-2 cursor-pointer font-semibold text-gray-700">
            <input 
              type="radio" 
              name="searchType" 
              value="correlativo" 
              [checked]="searchType() === 'correlativo'"
              (change)="setSearchType('correlativo')"
              class="w-4 h-4 text-[#007139] focus:ring-[#7bc342]">
            Búsqueda por correlativo
          </label>
        </div>

        <!-- Formulario por Cuenta -->
        @if (searchType() === 'cuenta') {
          <form [formGroup]="cuentaForm" (ngSubmit)="onSearchCuenta()" class="space-y-4">
            <div class="space-y-1">
              <label for="numCta" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Número de cuenta TC / Préstamo</label>
              <div class="flex gap-2">
                <input 
                  id="numCta" 
                  type="text" 
                  formControlName="numCta"
                  placeholder="Ingrese el número de cuenta"
                  class="flex-1 px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-semibold font-mono">
                <button 
                  type="submit" 
                  [disabled]="cuentaForm.invalid || isSearching()"
                  class="px-6 py-3 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center gap-2">
                  @if (isSearching()) {
                    <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    <span>Buscando...</span>
                  } @else {
                    <span>🔎 Buscar</span>
                  }
                </button>
              </div>
              @if (cuentaForm.controls.numCta.touched && cuentaForm.controls.numCta.invalid) {
                <p class="text-red-600 text-xs mt-1">Debe ingresar únicamente números.</p>
              }
            </div>
          </form>
        }

        <!-- Formulario por Correlativo -->
        @if (searchType() === 'correlativo') {
          <form [formGroup]="correlativoForm" (ngSubmit)="onSearchCorrelativo()" class="space-y-4">
            <div class="space-y-1">
              <label for="correlativo" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Número de Correlativo / Parámetro</label>
              <div class="flex gap-2">
                <input 
                  id="correlativo" 
                  type="text" 
                  formControlName="correlativo"
                  placeholder="Ingrese el código de parámetro"
                  class="flex-1 px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-semibold font-mono">
                <button 
                  type="submit" 
                  [disabled]="correlativoForm.invalid || isSearching()"
                  class="px-6 py-3 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center gap-2">
                  @if (isSearching()) {
                    <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    <span>Buscando...</span>
                  } @else {
                    <span>🔎 Buscar</span>
                  }
                </button>
              </div>
              @if (correlativoForm.controls.correlativo.touched && correlativoForm.controls.correlativo.invalid) {
                <p class="text-red-600 text-xs mt-1">Debe ingresar únicamente números.</p>
              }
            </div>
          </form>
        }
      </div>

      <!-- Detalle de Link Encontrado -->
      @if (linkInfo()) {
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
          <div class="bg-gradient-to-r from-gray-800 to-gray-900 px-6 py-4 text-white flex justify-between items-center">
            <h3 class="font-bold text-sm uppercase tracking-wider">Detalles de Programación</h3>
            <span class="font-mono text-xs opacity-75">ID: {{ linkInfo()?.codParametro }}</span>
          </div>
          
          <div class="p-6 space-y-6">
            <div class="grid grid-cols-3 gap-4 text-sm">
              <div class="bg-gray-50 p-4 rounded-xl">
                <span class="text-xs font-bold text-gray-400 block uppercase">Correlativo</span>
                <span class="font-semibold text-gray-800 mt-1 block font-mono">{{ linkInfo()?.codParametro }}</span>
              </div>
              <div class="bg-gray-50 p-4 rounded-xl">
                <span class="text-xs font-bold text-gray-400 block uppercase">Día de Cobro</span>
                <span class="font-semibold text-gray-800 mt-1 block font-mono">{{ linkInfo()?.diaMes }} de cada mes</span>
              </div>
              <div class="bg-gray-50 p-4 rounded-xl">
                <span class="text-xs font-bold text-gray-400 block uppercase">Próxima Fecha</span>
                <span class="font-semibold text-gray-800 mt-1 block font-mono">{{ linkInfo()?.proximaFecha | date:'dd/MM/yyyy' }}</span>
              </div>
            </div>

            <!-- Acciones -->
            <div class="border-t pt-6 flex justify-end gap-3">
              <button 
                type="button" 
                (click)="cancel()"
                class="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold rounded-xl transition-all">
                Cancelar
              </button>
              
              <button 
                type="button"
                (click)="deshabilitarLink()"
                [disabled]="isDisabling()"
                class="px-6 py-2.5 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-red-600/10 flex items-center gap-2">
                @if (isDisabling()) {
                  <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                  <span>Deshabilitando...</span>
                } @else {
                  <span>🚫 Deshabilitar Link</span>
                }
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class CancelarLinkComponent {
  private readonly fb = inject(FormBuilder);
  private readonly linkService = inject(LinkService);
  private readonly ui = inject(UiService);
  private readonly router = inject(Router);

  searchType = signal<'cuenta' | 'correlativo'>('cuenta');
  isSearching = signal(false);
  isDisabling = signal(false);
  linkInfo = signal<LinkCtaInfo | null>(null);

  readonly cuentaForm = this.fb.group({
    numCta: ['', [Validators.required, Validators.pattern('^[0-9]+$')]]
  });

  readonly correlativoForm = this.fb.group({
    correlativo: ['', [Validators.required, Validators.pattern('^[0-9]+$')]]
  });

  constructor() {
    this.ui.title.set('Cancelar Programación Link');
  }

  setSearchType(type: 'cuenta' | 'correlativo') {
    this.searchType.set(type);
    this.linkInfo.set(null);
    this.cuentaForm.reset();
    this.correlativoForm.reset();
  }

  onSearchCuenta() {
    if (this.cuentaForm.invalid) return;
    this.isSearching.set(true);
    this.linkInfo.set(null);

    const numCta = this.cuentaForm.value.numCta!;

    this.linkService.buscarCta(numCta).subscribe({
      next: (res) => {
        this.isSearching.set(false);
        if (res.success && res.data) {
          this.linkInfo.set(res.data);
        } else {
          this.ui.showError(res.errorMessage || 'No se encontró información de link programado.');
        }
      },
      error: (err) => {
        this.isSearching.set(false);
        this.ui.showError('No se encontró información o la cuenta no tiene programaciones activas.');
      }
    });
  }

  onSearchCorrelativo() {
    if (this.correlativoForm.invalid) return;
    this.isSearching.set(true);
    this.linkInfo.set(null);

    const correlativo = this.correlativoForm.value.correlativo!;

    this.linkService.buscarParametro(correlativo).subscribe({
      next: (res) => {
        this.isSearching.set(false);
        if (res.success && res.data) {
          this.linkInfo.set(res.data);
        } else {
          this.ui.showError(res.errorMessage || 'No se encontró información de link programado.');
        }
      },
      error: (err) => {
        this.isSearching.set(false);
        this.ui.showError('No se encontró información o el código no tiene programaciones activas.');
      }
    });
  }

  deshabilitarLink() {
    const info = this.linkInfo();
    if (!info) return;

    this.isDisabling.set(true);
    this.linkService.updateEstadoLink(info.codParametro).subscribe({
      next: (res) => {
        this.isDisabling.set(false);
        if (res.success) {
          this.ui.showSuccess('Parámetro del Link Modificado Exitosamente.');
          this.linkInfo.set(null);
          this.cuentaForm.reset();
          this.correlativoForm.reset();
        } else {
          this.ui.showError('No se pudo desactivar el link programado.');
        }
      },
      error: () => {
        this.isDisabling.set(false);
        this.ui.showError('Error al desactivar el link programado.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/home']);
  }
}
