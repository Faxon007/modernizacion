import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ParameterService, SystemParameters } from '../../core/services/parameter.service';
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-parametros',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="max-w-5xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado de Sección -->
      <div class="flex items-center justify-between border-b pb-4">
        <div>
          <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Configuración de Parámetros</h1>
          <p class="text-gray-500 text-sm mt-1">Configure las cuentas contables, frecuencias de revisión, plantillas de mensajería e imagen publicitaria.</p>
        </div>
        <div class="px-3 py-1 bg-[#7bc342]/10 border border-[#7bc342]/30 rounded-full text-xs font-semibold text-[#007139]">
          Configuración General
        </div>
      </div>

      <!-- Alertas Locales -->
      @if (localError()) {
        <div class="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-xl flex items-start gap-3">
          <span class="text-red-500 mt-0.5">⚠️</span>
          <div class="flex-1">
            <h4 class="font-bold text-red-800 text-sm">Error</h4>
            <p class="text-red-700 text-xs mt-0.5">{{ localError() }}</p>
          </div>
          <button (click)="localError.set(null)" class="text-red-400 hover:text-red-600 text-lg font-bold">&times;</button>
        </div>
      }

      @if (isLoading()) {
        <div class="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-gray-100 shadow-sm">
          <div class="animate-spin rounded-full h-12 w-12 border-4 border-[#007139] border-t-transparent mb-4"></div>
          <span class="text-gray-500 font-medium">Cargando parámetros del sistema...</span>
        </div>
      } @else {
        <form [formGroup]="paramForm" (ngSubmit)="onSave()" class="space-y-6">
          
          <!-- SECCIÓN 1: Frecuencias de Procesamiento -->
          <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
              <h3 class="font-bold text-lg flex items-center gap-2">
                <span>⏰</span> Frecuencias de Tareas Automáticas
              </h3>
            </div>
            <div class="p-6 grid grid-cols-1 md:grid-cols-2 gap-6">
              
              <!-- Frecuencia de Revisión -->
              <div class="space-y-4 p-4 bg-gray-50 rounded-xl border border-gray-100">
                <h4 class="font-bold text-gray-800 text-sm border-b pb-2">Revisión de Autorizaciones</h4>
                
                <div class="space-y-2">
                  <span class="block text-xs font-bold text-gray-500 uppercase tracking-wider">Frecuencia</span>
                  <div class="flex gap-4">
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freRevAutorizacion" value="U" class="text-[#007139] focus:ring-[#007139]">
                      Única
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freRevAutorizacion" value="D" class="text-[#007139] focus:ring-[#007139]">
                      Diario
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freRevAutorizacion" value="S" class="text-[#007139] focus:ring-[#007139]">
                      Semanal
                    </label>
                  </div>
                </div>

                <div class="space-y-1">
                  <label for="freRevHrsRepetir" class="block text-xs font-bold text-gray-500 uppercase tracking-wider">Horas a Repetir</label>
                  <select 
                    id="freRevHrsRepetir" 
                    formControlName="freRevHrsRepetir"
                    class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium text-sm">
                    @for (hour of hoursList; track hour) {
                      <option [value]="hour">{{ hour }} hrs</option>
                    }
                  </select>
                </div>
              </div>

              <!-- Frecuencia de Generación -->
              <div class="space-y-4 p-4 bg-gray-50 rounded-xl border border-gray-100">
                <h4 class="font-bold text-gray-800 text-sm border-b pb-2">Generación Automática de Links</h4>
                
                <div class="space-y-2">
                  <span class="block text-xs font-bold text-gray-500 uppercase tracking-wider">Frecuencia</span>
                  <div class="flex gap-4">
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freGenLink" value="U" class="text-[#007139] focus:ring-[#007139]">
                      Única
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freGenLink" value="D" class="text-[#007139] focus:ring-[#007139]">
                      Diario
                    </label>
                    <label class="flex items-center gap-2 cursor-pointer text-sm font-semibold text-gray-700">
                      <input type="radio" formControlName="freGenLink" value="S" class="text-[#007139] focus:ring-[#007139]">
                      Semanal
                    </label>
                  </div>
                </div>

                <div class="space-y-1">
                  <label for="freGenHora" class="block text-xs font-bold text-gray-500 uppercase tracking-wider">Hora de Generación</label>
                  <input 
                    id="freGenHora" 
                    type="time" 
                    formControlName="freGenHora"
                    (keydown)="preventManualInput($event)"
                    class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-semibold font-mono text-sm">
                </div>
              </div>

            </div>
          </div>

          <!-- SECCIÓN 2: Cuentas y Entidades Contables -->
          <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
              <h3 class="font-bold text-lg flex items-center gap-2">
                <span>🏦</span> Cuentas Contables y Estructura
              </h3>
            </div>
            <div class="p-6 grid grid-cols-1 md:grid-cols-4 gap-6">
              
              <!-- Cta QTZ -->
              <div class="space-y-1">
                <label for="numCtaContaQtz" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Cta Contable Quetzales (GTQ)</label>
                <input 
                  id="numCtaContaQtz" 
                  type="text" 
                  formControlName="numCtaContaQtz"
                  class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium font-mono">
              </div>

              <!-- Cta USD -->
              <div class="space-y-1">
                <label for="numCtaContaDol" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Cta Contable Dólares (USD)</label>
                <input 
                  id="numCtaContaDol" 
                  type="text" 
                  formControlName="numCtaContaDol"
                  class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium font-mono">
              </div>

              <!-- Agencia -->
              <div class="space-y-1">
                <label for="codAgencia" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Código de Agencia</label>
                <input 
                  id="codAgencia" 
                  type="text" 
                  formControlName="codAgencia"
                  class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium">
              </div>

              <!-- Departamento -->
              <div class="space-y-1">
                <label for="codDepartamento" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Departamento</label>
                <input 
                  id="codDepartamento" 
                  type="text" 
                  formControlName="codDepartamento"
                  class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium">
              </div>

            </div>
          </div>

          <!-- SECCIÓN 3: Configuración de Transacciones core (TC / PR) -->
          <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
              <h3 class="font-bold text-lg flex items-center gap-2">
                <span>💳</span> Configuración de Transacciones Core
              </h3>
            </div>
            <div class="p-6 space-y-6">
              
              <!-- Tarjeta de Crédito (TC) -->
              <div class="p-4 bg-purple-50/50 border border-purple-100 rounded-xl space-y-4">
                <h4 class="font-bold text-purple-900 text-sm flex items-center gap-2">
                  <span>💳</span> Tarjeta de Crédito (TC)
                </h4>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div class="space-y-1">
                    <label for="tcTipTransac" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Tipo Transacción TC</label>
                    <input id="tcTipTransac" type="text" formControlName="tcTipTransac" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                  <div class="space-y-1">
                    <label for="tcSubtipTrans" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Subtipo Transacción TC</label>
                    <input id="tcSubtipTrans" type="text" formControlName="tcSubtipTrans" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                  <div class="space-y-1">
                    <label for="desTransaccion" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Descripción de Pago TC</label>
                    <input id="desTransaccion" type="text" formControlName="desTransaccion" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div class="space-y-1">
                    <label for="codTipoTc" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Código Tipo TC (Bitacora)</label>
                    <input id="codTipoTc" type="text" formControlName="codTipoTc" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                  <div class="space-y-1">
                    <label for="codSubtipoTc" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Código Subtipo TC (Bitacora)</label>
                    <input id="codSubtipoTc" type="text" formControlName="codSubtipoTc" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                </div>
              </div>

              <!-- Préstamos (PR) -->
              <div class="p-4 bg-blue-50/50 border border-blue-100 rounded-xl space-y-4">
                <h4 class="font-bold text-blue-900 text-sm flex items-center gap-2">
                  <span>📅</span> Préstamos / Créditos (PR)
                </h4>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div class="space-y-1">
                    <label for="codTipoPr" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Código Tipo PR</label>
                    <input id="codTipoPr" type="text" formControlName="codTipoPr" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                  <div class="space-y-1">
                    <label for="codSubtipoPr" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Código Subtipo PR</label>
                    <input id="codSubtipoPr" type="text" formControlName="codSubtipoPr" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                  <div class="space-y-1">
                    <label for="codDeptoPr" class="block text-xs font-bold text-gray-600 uppercase tracking-wider">Campaña / Depto PR</label>
                    <input id="codDeptoPr" type="text" formControlName="codDeptoPr" class="w-full px-3 py-2 bg-white border border-gray-200 rounded-lg focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 text-sm">
                  </div>
                </div>
              </div>

            </div>
          </div>

          <!-- SECCIÓN 4: Plantillas de Mensajería -->
          <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
              <h3 class="font-bold text-lg flex items-center gap-2">
                <span>📧</span> Plantillas de Correo y SMS
              </h3>
            </div>
            <div class="p-6 space-y-6">
              
              <!-- Remitente y SMS -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-1">
                  <label for="msgRemitente" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Remitente del Correo (Email Sender)</label>
                  <input 
                    id="msgRemitente" 
                    type="text" 
                    formControlName="msgRemitente"
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium">
                </div>

                <div class="space-y-1">
                  <div class="flex justify-between items-center">
                    <label for="msgSms" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Plantilla Mensaje SMS</label>
                    <span [class]="smsCharCount() > 160 ? 'text-red-500 font-bold' : 'text-gray-400 font-semibold'" class="text-xs">
                      {{ smsCharCount() }}/160 caracteres
                    </span>
                  </div>
                  <input 
                    id="msgSms" 
                    type="text" 
                    formControlName="msgSms"
                    (input)="updateSmsCharCount()"
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium">
                  <p class="text-[10px] text-gray-400 mt-1">
                    Nota: El SMS debe contener variables legibles y el link de pago se adjuntará al final.
                  </p>
                </div>
              </div>

              <!-- Header y Footer del Correo -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6 border-t pt-6">
                <div class="space-y-1">
                  <label for="msgHeader" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Encabezado de Correo (Header HTML/Texto)</label>
                  <textarea 
                    id="msgHeader" 
                    rows="6"
                    formControlName="msgHeader"
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium font-mono text-xs"></textarea>
                </div>

                <div class="space-y-1">
                  <label for="msgFooter" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Pie de Página de Correo (Footer HTML/Texto)</label>
                  <textarea 
                    id="msgFooter" 
                    rows="6"
                    formControlName="msgFooter"
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] text-gray-800 font-medium font-mono text-xs"></textarea>
                </div>
              </div>

            </div>
          </div>

          <!-- SECCIÓN 5: Imagen Publicitaria de Neo -->
          <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
            <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
              <h3 class="font-bold text-lg flex items-center gap-2">
                <span>🖼️</span> Imagen Promocional Publicitaria (Neo)
              </h3>
            </div>
            <div class="p-6 grid grid-cols-1 md:grid-cols-3 gap-6">
              
              <!-- Upload control -->
              <div class="col-span-1 space-y-4">
                <span class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Cargar Nueva Imagen</span>
                
                <div 
                  class="border-2 border-dashed border-gray-200 rounded-2xl p-6 text-center hover:border-[#7bc342] transition-colors cursor-pointer relative"
                  (dragover)="$event.preventDefault()"
                  (drop)="onImageDropped($event)">
                  
                  <input 
                    type="file" 
                    accept="image/*" 
                    class="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                    (change)="onImageSelected($event)">
                  
                  <div class="space-y-2">
                    <span class="text-3xl block">📤</span>
                    <span class="text-xs font-bold text-gray-500 block">
                      Haga clic o arrastre un archivo
                    </span>
                    <span class="text-[10px] text-gray-400 block">
                      PNG o JPG. Recomendado: 800x400 px, Máx. 1.5MB
                    </span>
                  </div>
                </div>

                @if (imagePreview()) {
                  <button 
                    type="button" 
                    (click)="removeImage()"
                    class="w-full py-2 bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 text-xs font-bold rounded-xl transition-all">
                    Eliminar Imagen Promocional
                  </button>
                }
              </div>

              <!-- Preview -->
              <div class="col-span-2 bg-gray-50 border border-gray-100 rounded-2xl p-4 flex flex-col items-center justify-center min-h-[180px]">
                <span class="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-2 self-start">Vista Previa de la Publicidad</span>
                @if (imagePreview()) {
                  <div class="relative w-full max-h-[220px] overflow-hidden rounded-xl border bg-white flex items-center justify-center">
                    <img [src]="imagePreview()" alt="Publicidad Neo" class="max-w-full max-h-[220px] object-contain">
                  </div>
                } @else {
                  <div class="text-center py-8 text-gray-400 space-y-2">
                    <span class="text-4xl block">🖼️</span>
                    <span class="text-xs font-medium">No hay imagen publicitaria cargada</span>
                  </div>
                }
              </div>

            </div>
          </div>

          <!-- Botones de Acción -->
          <div class="border-t pt-6 flex justify-end gap-3">
            <button 
              type="button" 
              (click)="cancel()"
              [disabled]="isSaving()"
              class="px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold rounded-xl transition-all">
              Cancelar
            </button>
            <button 
              type="submit" 
              [disabled]="isSaving()"
              class="px-8 py-3 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center justify-center gap-2">
              @if (isSaving()) {
                <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                <span>Guardando Parámetros...</span>
              } @else {
                <span>💾 Guardar Cambios</span>
              }
            </button>
          </div>

        </form>
      }
    </div>
  `
})
export class ParametrosComponent {
  private readonly fb = inject(FormBuilder);
  private readonly paramService = inject(ParameterService);
  private readonly ui = inject(UiService);
  private readonly router = inject(Router);

  // States
  isLoading = signal(false);
  isSaving = signal(false);
  localError = signal<string | null>(null);
  imagePreview = signal<string | null>(null);
  smsCharCount = signal(0);

  hoursList = Array.from({ length: 24 }, (_, i) => i);

  // Reactive Form
  readonly paramForm: FormGroup = this.fb.group({
    freRevAutorizacion: ['U', [Validators.required]],
    freRevHrsRepetir: ['0', [Validators.required]],
    freGenLink: ['U', [Validators.required]],
    freGenHora: ['', [Validators.required]],
    tcTipTransac: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
    tcSubtipTrans: ['', [Validators.required]],
    numCtaContaQtz: ['', [Validators.required]],
    numCtaContaDol: ['', [Validators.required]],
    codAgencia: ['', [Validators.required]],
    codTipoTc: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
    codSubtipoTc: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
    codTipoPr: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
    codSubtipoPr: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
    codDepartamento: ['', [Validators.required]],
    codDeptoPr: ['', [Validators.required]],
    desTransaccion: ['', [Validators.required]],
    msgRemitente: [''],
    msgHeader: ['', [Validators.required]],
    msgFooter: ['', [Validators.required]],
    msgSms: ['', [Validators.required, Validators.maxLength(160)]],
    apiImagenBase64: ['']
  });

  constructor() {
    this.ui.title.set('Configuración General del Sistema');
    this.loadParameters();
  }

  preventManualInput(event: KeyboardEvent) {
    // Permite teclas de navegación (Tab, flechas) y de borrado, pero bloquea la escritura de números y letras.
    const allowedKeys = ['Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Delete', 'Backspace'];
    if (allowedKeys.includes(event.key)) {
      return; // Permite la acción
    }
    // Para cualquier otra tecla, previene la acción por defecto (escribir en el input).
    event.preventDefault();
  }

  private convertTo24HourFormat(time12h: string): string {
    if (!time12h || !time12h.includes(' ')) {
      // Si ya está en formato HH:mm o está vacío, lo devolvemos.
      if (/^([01]\d|2[0-3]):([0-5]\d)$/.test(time12h)) {
        return time12h;
      }
      return '';
    }

    const [time, modifier] = time12h.split(' ');
    let [hours, minutes] = time.split(':');

    if (hours === '12') {
      hours = '00';
    }
    if (modifier.toUpperCase() === 'PM') {
      hours = (parseInt(hours, 10) + 12).toString();
    }
    return `${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}`;
  }

  private convertTo12HourFormat(time24h: string): string {
    if (!time24h || !time24h.includes(':')) {
      return ''; // Devuelve vacío si no es un formato de hora válido
    }

    const [hoursStr, minutes] = time24h.split(':');
    let hours = parseInt(hoursStr, 10);

    const ampm = hours >= 12 ? 'PM' : 'AM'; // Determina si es AM o PM
    let hours12 = hours % 12;
    hours12 = hours12 ? hours12 : 12; // La hora '0' (medianoche) se convierte en '12'

    const finalHours = hours12.toString();
    const finalMinutes = minutes.padStart(2, '0');

    return `${finalHours}:${finalMinutes} ${ampm}`;
  }

  loadParameters() {
    this.isLoading.set(true);
    this.localError.set(null);

    this.paramService.getParameters().subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success && res.data) {
          const data = res.data;
          
          // Patch form values
          this.paramForm.patchValue({
            freRevAutorizacion: data.freRevAutorizacion || 'U',
            freRevHrsRepetir: data.freRevHrsRepetir ? String(parseInt(data.freRevHrsRepetir, 10)) : '0',
            freGenLink: data.freGenLink || 'U',
            freGenHora: this.convertTo24HourFormat(data.freGenHora || ''),
            tcTipTransac: data.tcTipTransac || '',
            tcSubtipTrans: data.tcSubtipTrans || '',
            numCtaContaQtz: data.numCtaContaQtz || '',
            numCtaContaDol: data.numCtaContaDol || '',
            codAgencia: data.codAgencia || '',
            codTipoTc: data.codTipoTc || '',
            codSubtipoTc: data.codSubtipoTc || '',
            codTipoPr: data.codTipoPr || '',
            codSubtipoPr: data.codSubtipoPr || '',
            codDepartamento: data.codDepartamento || '',
            codDeptoPr: data.codDeptoPr || '',
            desTransaccion: data.desTransaccion || '',
            msgRemitente: data.msgRemitente || '',
            msgHeader: data.msgHeader || '',
            msgFooter: data.msgFooter || '',
            msgSms: data.msgSms || '',
            apiImagenBase64: data.apiImagenBase64 || ''
          });

          // Set image preview
          if (data.apiImagenBase64) {
            const prefix = data.apiImagenBase64.startsWith('data:image') ? '' : 'data:image/png;base64,';
            this.imagePreview.set(prefix + data.apiImagenBase64);
          } else {
            this.imagePreview.set(null);
          }

          this.updateSmsCharCount();
        } else {
          this.localError.set(res.errorMessage || 'No se pudieron cargar los parámetros.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.localError.set('Error de comunicación al obtener los parámetros del sistema.');
      }
    });
  }

  updateSmsCharCount() {
    const text = this.paramForm.value.msgSms || '';
    this.smsCharCount.set(text.length);
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      if (file.size > 1500 * 1024) {
        this.ui.showError('La imagen excede el límite máximo de 1.5MB.');
        return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        const base64Str = reader.result as string;
        this.imagePreview.set(base64Str);
        this.paramForm.patchValue({ apiImagenBase64: base64Str });
      };
      reader.readAsDataURL(file);
    }
  }

  onImageDropped(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
      const file = event.dataTransfer.files[0];
      if (!file.type.startsWith('image/')) {
        this.ui.showError('El archivo debe ser una imagen válida.');
        return;
      }
      if (file.size > 1500 * 1024) {
        this.ui.showError('La imagen excede el límite máximo de 1.5MB.');
        return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        const base64Str = reader.result as string;
        this.imagePreview.set(base64Str);
        this.paramForm.patchValue({ apiImagenBase64: base64Str });
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage() {
    this.imagePreview.set(null);
    this.paramForm.patchValue({ apiImagenBase64: '' });
  }

  onSave() {
    if (this.paramForm.invalid) {
      const invalidFields = [];
      for (const controlName in this.paramForm.controls) {
        if (this.paramForm.controls[controlName].invalid) {
          invalidFields.push(controlName);
        }
      }
      this.localError.set('Faltan campos obligatorios o el formato es incorrecto. Revise: ' + invalidFields.join(', '));
      return;
    }
    this.isSaving.set(true);
    this.localError.set(null);

    const formVal = this.paramForm.value;

    const payload: SystemParameters = {
      freRevAutorizacion: formVal.freRevAutorizacion,
      freRevHrsRepetir: String(formVal.freRevHrsRepetir),
      freGenLink: formVal.freGenLink,
      freGenHora: this.convertTo12HourFormat(formVal.freGenHora),
      tcTipTransac: String(formVal.tcTipTransac),
      tcSubtipTrans: formVal.tcSubtipTrans,
      numCtaContaQtz: formVal.numCtaContaQtz,
      numCtaContaDol: formVal.numCtaContaDol,
      codAgencia: formVal.codAgencia,
      codTipoTc: String(formVal.codTipoTc),
      codSubtipoTc: String(formVal.codSubtipoTc),
      codTipoPr: String(formVal.codTipoPr),
      codSubtipoPr: String(formVal.codSubtipoPr),
      codDepartamento: formVal.codDepartamento,
      codDeptoPr: formVal.codDeptoPr,
      desTransaccion: formVal.desTransaccion,
      msgRemitente: formVal.msgRemitente,
      msgHeader: formVal.msgHeader,
      msgFooter: formVal.msgFooter,
      msgSms: formVal.msgSms,
      apiImagenBase64: formVal.apiImagenBase64 || ''
    };

    this.paramService.updateParameters(payload).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success) {
          this.ui.showSuccess('Parámetros guardados y actualizados exitosamente.');
          this.router.navigate(['/home']);
        } else {
          this.localError.set(res.errorMessage || 'No se pudieron guardar los parámetros.');
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.localError.set('Error en el servidor al intentar guardar los parámetros.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/home']);
  }
}
