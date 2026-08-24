import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom, forkJoin, of } from 'rxjs';
import { LinkService, LinkEntity, ClientEntity } from '../../core/services/link.service';
import { ParameterService } from '../../core/services/parameter.service';
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-emision-link',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="max-w-4xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado de Sección -->
      <div class="flex items-center justify-between border-b pb-4">
        <div>
          <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Emisión de Link de Pago</h1>
          <p class="text-gray-500 text-sm mt-1">Busque el producto del cliente y parametrice el link de cobro.</p>
        </div>
        <div class="px-3 py-1 bg-[#7bc342]/10 border border-[#7bc342]/30 rounded-full text-xs font-semibold text-[#007139]">
          VisaEnLink Activo
        </div>
      </div>

      @if (generatedUrl()) {
        <div class="bg-gradient-to-r from-emerald-50 to-[#7bc342]/10 border border-emerald-200 p-6 rounded-2xl flex flex-col md:flex-row items-center justify-between gap-4 shadow-sm animate-in zoom-in-95 duration-300">
          <div class="space-y-1 text-center md:text-left">
            <h4 class="font-bold text-[#007139] text-base">🎉 ¡Link Generado Exitosamente!</h4>
            <p class="text-gray-600 text-xs">El enlace de cobro ya está disponible para enviar al cliente.</p>
            <div class="mt-2 font-mono text-sm text-[#007139] bg-white border px-3 py-2 rounded-lg break-all select-all shadow-inner">
              {{ generatedUrl() }}
            </div>
          </div>
          <div class="flex gap-2">
            <button (click)="copyLink()" class="px-4 py-2 bg-[#007139] text-white hover:bg-[#007139]/90 text-sm font-semibold rounded-lg shadow transition-colors flex items-center gap-2">
              <span>📋</span> Copiar Link
            </button>
            <button (click)="resetForm()" class="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-700 text-sm font-semibold rounded-lg transition-colors">
              Emitir Otro
            </button>
          </div>
        </div>
      }

      <!-- Paso 1: Búsqueda del Producto -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
          <h3 class="font-bold text-lg flex items-center gap-2">
            <span class="opacity-70">01.</span> Información del Producto
          </h3>
        </div>
        <div class="p-6">
          <form [formGroup]="searchForm" (ngSubmit)="onSearch()" class="flex flex-col md:flex-row items-end gap-4">
            <div class="flex-1 space-y-1">
              <label for="numCuenta" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Número de cuenta TC / Préstamo</label>
              <div class="relative">
                <input 
                  id="numCuenta" 
                  type="text" 
                  formControlName="numCuenta"
                  class="w-full pl-4 pr-10 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium"
                  placeholder="Ingrese el número de tarjeta o préstamo"
                  [readonly]="isProductSelected()">
                @if (isProductSelected()) {
                  <span class="absolute right-3 top-3.5 text-emerald-500">✔</span>
                }
              </div>
            </div>
            <div class="flex gap-2 w-full md:w-auto">
              @if (!isProductSelected()) {
                <button 
                  type="submit" 
                  [disabled]="searchForm.invalid || isSearching()"
                  class="w-full md:w-auto px-6 py-3 bg-[#007139] text-white hover:bg-[#007139]/90 disabled:opacity-50 font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center justify-center gap-2">
                  @if (isSearching()) {
                    <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    <span>Buscando...</span>
                  } @else {
                    <span>🔎 Buscar Producto</span>
                  }
                </button>
              } @else {
                <button 
                  type="button" 
                  (click)="resetSearch()"
                  class="w-full md:w-auto px-6 py-3 bg-red-50 text-red-600 hover:bg-red-100 font-bold rounded-xl transition-all border border-red-200 flex items-center justify-center gap-2">
                  <span>🔄 Cambiar</span>
                </button>
              }
            </div>
          </form>

          <!-- Detalles del Cliente Seleccionado -->
          @if (isProductSelected() && selectedClient()) {
            <div class="mt-6 border-t pt-6 grid grid-cols-1 md:grid-cols-3 gap-4 animate-in fade-in duration-300">
              <div class="p-4 bg-gray-50 rounded-xl border border-gray-100">
                <span class="text-xs font-bold text-gray-400 uppercase tracking-wider block">Cliente</span>
                <span class="text-sm font-semibold text-gray-800 block mt-1">{{ selectedClient()?.nomCliente }}</span>
                <span class="text-xs text-gray-500 block">Cod: {{ selectedClient()?.codCliente }}</span>
              </div>
              <div class="p-4 bg-gray-50 rounded-xl border border-gray-100">
                <span class="text-xs font-bold text-gray-400 uppercase tracking-wider block">Correo Electrónico (Default)</span>
                <span class="text-sm font-semibold text-gray-800 block mt-1 break-all">{{ defaultEmail() || 'No registrado' }}</span>
              </div>
              <div class="p-4 bg-gray-50 rounded-xl border border-gray-100">
                <span class="text-xs font-bold text-gray-400 uppercase tracking-wider block">Teléfono (Default)</span>
                <span class="text-sm font-semibold text-gray-800 block mt-1">{{ defaultPhone() || 'No registrado' }}</span>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Paso 2: Detalles y Envío del Link -->
      @if (isProductSelected()) {
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden animate-in slide-in-from-bottom-6 duration-300">
          <div class="bg-gradient-to-r from-[#007139] to-[#007139]/95 px-6 py-4 text-white">
            <h3 class="font-bold text-lg flex items-center gap-2">
              <span class="opacity-70">02.</span> Parametrización y Notificación
            </h3>
          </div>
          
          <form [formGroup]="linkForm" (ngSubmit)="onSave()" class="p-6 space-y-6">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              
              <!-- Monto -->
              <div class="space-y-1">
                <label for="monto" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Monto o Cantidad</label>
                <div class="relative">
                  <span class="absolute left-3 top-3.5 text-gray-400 font-bold">
                    {{ isDolarPayment() ? '$' : 'Q' }}
                  </span>
                  <input 
                    id="monto" 
                    type="text" 
                    formControlName="monto"
                    (input)="onMontoInput($event)"
                    (blur)="onMontoBlur()"
                    class="w-full pl-8 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-semibold text-lg">
                </div>
                <p class="text-xs text-gray-400 mt-1">Límite máximo permitido: <span class="font-bold text-gray-700">{{ maxMontoFormatted() }}</span></p>
              </div>

              <!-- Pagar en Dólares -->
              <div class="flex items-center pt-6">
                <label class="relative inline-flex items-center cursor-pointer">
                  <input 
                    type="checkbox" 
                    formControlName="pagarDolares"
                    class="sr-only peer">
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-[#007139] rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-[#007139]"></div>
                  <span class="ml-3 text-sm font-semibold text-gray-700">Pagar en Dólares ($)</span>
                </label>
              </div>

              <!-- Programación o Enrolamiento -->
              <div class="space-y-1">
                <label for="tipLink" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Enrolamiento (Programado)</label>
                <select 
                  id="tipLink" 
                  formControlName="tipLink"
                  class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
                  <option value="2">No (Manual)</option>
                  <option value="1">Sí (Automático)</option>
                </select>
              </div>
            </div>

            <!-- Día de Mes (si es programado) -->
            @if (linkForm.value.tipLink === '1') {
              <div class="p-4 bg-[#7bc342]/5 border border-[#7bc342]/20 rounded-2xl grid grid-cols-1 md:grid-cols-2 gap-4 animate-in slide-in-from-top-2 duration-200">
                <div class="space-y-1">
                  <label for="diaMes" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Día del mes a generar link</label>
                  <select 
                    id="diaMes" 
                    formControlName="diaMes"
                    class="w-full px-4 py-3 bg-white border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#007139] focus:border-[#007139] transition-all text-gray-800 font-medium">
                    @for (day of daysList; track day) {
                      <option [value]="day">{{ day }}</option>
                    }
                  </select>
                </div>
                <div class="flex items-center text-xs text-[#007139] font-medium leading-normal">
                  💡 Los links programados se generarán y enviarán de forma automática los días seleccionados de cada mes.
                </div>
              </div>
            }

            <!-- Sección de Envío/Notificación -->
            <div class="border-t pt-6 space-y-4">
              <h4 class="font-bold text-gray-800 text-sm">Información de Envío</h4>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                <!-- Canal de Envío -->
                <div class="space-y-2">
                  <span class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Medio de Envío</span>
                  <div class="flex gap-2">
                    <button 
                      type="button"
                      (click)="setCanalEnvio('1')"
                      [class]="linkForm.value.tipEnvio === '1' ? 'flex-1 py-3 bg-[#007139] text-white border border-[#007139]' : 'flex-1 py-3 bg-gray-50 text-gray-600 border border-gray-200 hover:bg-gray-100'"
                      class="rounded-xl font-semibold text-sm transition-all flex items-center justify-center gap-2">
                      <span>💬</span> SMS
                    </button>
                    <button 
                      type="button"
                      (click)="setCanalEnvio('2')"
                      [class]="linkForm.value.tipEnvio === '2' ? 'flex-1 py-3 bg-[#007139] text-white border border-[#007139]' : 'flex-1 py-3 bg-gray-50 text-gray-600 border border-gray-200 hover:bg-gray-100'"
                      class="rounded-xl font-semibold text-sm transition-all flex items-center justify-center gap-2">
                      <span>📧</span> Correo
                    </button>
                  </div>
                </div>

                <!-- Tipo de datos -->
                <div class="space-y-1">
                  <label for="esDefault" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Datos del Remitente</label>
                  <select 
                    id="esDefault" 
                    formControlName="esDefault"
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
                    <option value="1">Utilizar Datos Default</option>
                    <option value="2">Editar información de envío</option>
                  </select>
                </div>

                <!-- Campo dinámico Correo / Telefono -->
                @if (linkForm.value.esDefault === '2') {
                  <div class="space-y-1 animate-in zoom-in-95 duration-200">
                    @if (linkForm.value.tipEnvio === '1') {
                      <label for="customTelefono" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Número de Teléfono a Enviar</label>
                      <input 
                        id="customTelefono"
                        type="text"
                        formControlName="customTelefono"
                        placeholder="Ej: 55555555"
                        class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
                        @if (linkForm.get('customTelefono')?.invalid && (linkForm.get('customTelefono')?.dirty || linkForm.get('customTelefono')?.touched)) {
                          <div class="text-red-500 text-xs mt-1">
                            El número de teléfono debe tener 8 dígitos.
                          </div>
                        }
                    } @else {
                      <label for="customEmail" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Correo Electrónico a Enviar</label>
                      <input 
                        id="customEmail"
                        type="email"
                        formControlName="customEmail"
                        placeholder="cliente@ejemplo.com"
                        class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
                        @if (linkForm.get('customEmail')?.invalid && (linkForm.get('customEmail')?.dirty || linkForm.get('customEmail')?.touched)) {
                          <div class="text-red-500 text-xs mt-1">
                            Debe ingresar un correo válido.
                          </div>
                        }
                    }
                  </div>
                }
              </div>
            </div>

            <!-- Botones de Acción -->
            <div class="border-t pt-6 flex justify-end gap-3">
              <button 
                type="button" 
                (click)="cancel()"
                class="px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold rounded-xl transition-all">
                Cancelar
              </button>
              <button 
                type="submit" 
                [disabled]="linkForm.invalid || isSaving() || !isMontoValido()"
                class="px-8 py-3 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center justify-center gap-2">
                @if (isSaving()) {
                  <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                  <span>Generando y Guardando...</span>
                } @else {
                  <span>💾 Guardar Link</span>
                }
              </button>
            </div>
          </form>
        </div>
      }
    </div>

    <!-- Modal de Búsqueda y Selección de Cuentas -->
    @if (showCuentasModal()) {
      <div class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-in fade-in duration-200">
        <div class="bg-white rounded-3xl shadow-2xl max-w-2xl w-full border border-gray-100 overflow-hidden animate-in zoom-in-95 duration-200">
          <div class="bg-[#007139] text-white px-6 py-4 flex justify-between items-center">
            <h3 class="font-bold text-lg">Cuentas Disponibles del Cliente</h3>
            <button (click)="closeCuentasModal()" class="text-white hover:text-gray-200 text-2xl font-bold">&times;</button>
          </div>
          <div class="p-6">
            <p class="text-xs text-gray-500 mb-4">
              Seleccione la cuenta (Tarjeta de Crédito o Préstamo) a la cual desea asociar el link de cobro.
            </p>
            <div class="border rounded-2xl overflow-hidden shadow-inner max-h-[300px] overflow-y-auto">
              <table class="w-full text-left border-collapse">
                <thead>
                  <tr class="bg-gray-50 border-b text-xs font-bold text-gray-500 uppercase">
                    <th class="p-4">Número de Cuenta</th>
                    <th class="p-4">Tipo</th>
                    <th class="p-4">Estado</th>
                    <th class="p-4 text-center">Acción</th>
                  </tr>
                </thead>
                <tbody class="divide-y text-sm text-gray-700">
                  @for (cta of customerAccounts(); track cta.numCuenta) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="p-4 font-mono font-semibold">{{ cta.numCuenta }}</td>
                      <td class="p-4">
                        <span [class]="cta.tipo === 'Tarjeta' ? 'bg-purple-50 text-purple-700 border-purple-200' : 'bg-blue-50 text-blue-700 border-blue-200'" class="px-2 py-1 rounded text-xs font-bold border">
                          {{ cta.tipo }}
                        </span>
                      </td>
                      <td class="p-4">
                        <span class="px-2 py-1 rounded text-xs bg-emerald-50 text-emerald-700 font-bold border border-emerald-200">
                          {{ cta.estado }}
                        </span>
                      </td>
                      <td class="p-4 text-center">
                        <button 
                          (click)="selectAccount(cta)"
                          class="px-3 py-1.5 bg-[#007139] hover:bg-[#007139]/90 text-white font-semibold text-xs rounded-lg shadow-sm transition-colors">
                          Seleccionar
                        </button>
                      </td>
                    </tr>
                  } @empty {
                    <tr>
                      <td colspan="4" class="p-8 text-center text-gray-400">
                        No se encontraron cuentas activas para este cliente.
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            <div class="text-right mt-6">
              <button 
                (click)="closeCuentasModal()" 
                class="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold rounded-xl transition-colors">
                Cancelar
              </button>
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class EmisionLinkComponent {
  private readonly fb = inject(FormBuilder);
  private readonly linkService = inject(LinkService);
  private readonly parameterService = inject(ParameterService);
  private readonly ui = inject(UiService);
  private readonly router = inject(Router);

  // States
  isSearching = signal(false);
  isSaving = signal(false);
  isProductSelected = signal(false);
  showCuentasModal = signal(false);
  localError = signal<string | null>(null);

  selectedClient = signal<ClientEntity | null>(null);
  customerAccounts = signal<any[]>([]);
  selectedAccountInfo = signal<any | null>(null);

  defaultEmail = signal('');
  defaultPhone = signal('');

  maxMonto = signal<number>(0);
  isMontoValido = signal(true);
  generatedUrl = signal<string | null>(null);
  systemImageBase64 = signal<string>('');

  daysList = Array.from({ length: 31 }, (_, i) => i + 1);

  // Reactive Forms
  readonly searchForm = this.fb.group({
    numCuenta: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]]
  });

  readonly linkForm = this.fb.group({
    // Cambiamos el control para que maneje el string formateado. La validación numérica se hará manualmente.
    monto: ['0.00', [Validators.required]],
    pagarDolares: [false],
    tipLink: ['2'], // '2' = No, '1' = Si
    diaMes: [new Date().getDate()],
    tipEnvio: ['2'], // '1' = SMS, '2' = Correo
    esDefault: ['1'], // '1' = Default, '2' = Editado
    // Los validadores se añadirán dinámicamente
    customTelefono: [''],
    customEmail: ['']
  });

  constructor() {
    this.ui.title.set('Emisión de Links de Pago');
    this.loadSystemImage();
    this.setupDynamicValidators();
    // Inicializar el campo de monto con el formato correcto
    this.linkForm.get('monto')?.setValue(this.formatCurrency(0));
  }

  loadSystemImage() {
    this.parameterService.getParameters().subscribe({
      next: (res) => {
        if (res.success && res.data?.apiImagenBase64) {
          this.systemImageBase64.set(res.data.apiImagenBase64);
        }
      }
    });
  }

  private setupDynamicValidators() {
    const esDefaultControl = this.linkForm.get('esDefault');
    const tipEnvioControl = this.linkForm.get('tipEnvio');
    const customTelefonoControl = this.linkForm.get('customTelefono');
    const customEmailControl = this.linkForm.get('customEmail');

    if (!esDefaultControl || !tipEnvioControl || !customTelefonoControl || !customEmailControl) return;

    // Escuchar cambios en ambos controles
    esDefaultControl.valueChanges.subscribe(esDefault => {
      if (esDefault !== null && tipEnvioControl.value !== null) {
        this.updateValidators(esDefault, tipEnvioControl.value);
      }
    });

    tipEnvioControl.valueChanges.subscribe(tipEnvio => {
      if (esDefaultControl.value !== null && tipEnvio !== null) {
        this.updateValidators(esDefaultControl.value, tipEnvio);
      }
    });
  }

  private updateValidators(esDefault: string, tipEnvio: string) {
    const customTelefonoControl = this.linkForm.get('customTelefono');
    const customEmailControl = this.linkForm.get('customEmail');

    if (!customTelefonoControl || !customEmailControl) return;

    // Limpiar ambos campos y sus validadores primero
    customTelefonoControl.clearValidators();
    customTelefonoControl.setValue('');
    customEmailControl.clearValidators();
    customEmailControl.setValue('');

    if (esDefault === '2') { // Si es "Editar"
      if (tipEnvio === '1') { // y es SMS
        customTelefonoControl.setValidators([Validators.required, Validators.pattern(/^[0-9]{8}$/)]);
      } else { // y es Correo
        customEmailControl.setValidators([Validators.required, Validators.email]);
      }
    }
    customTelefonoControl.updateValueAndValidity();
    customEmailControl.updateValueAndValidity();
  }

  private formatCurrency(value: any): string {
    if (value === null || value === undefined || value === '') {
      return '0.00';
    }
    const num = typeof value === 'string' ? this.parseCurrency(value) : value;
    if (isNaN(num)) {
      return '0.00';
    }
    // Usamos 'en-US' para obtener el formato ###,###,###.## (coma para miles, punto para decimal)
    return num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  private parseCurrency(value: string | null | undefined): number {
    if (!value) {
      return 0;
    }
    // Para el formato ###,###,###.##, removemos las comas de miles antes de convertir a número.
    const cleanedValue = String(value).replace(/,/g, '');
    const num = parseFloat(cleanedValue);
    return isNaN(num) ? 0 : num;
  }

  onMontoInput(event: Event) {
    const inputElement = event.target as HTMLInputElement;
    let value = inputElement.value.replace(/[^0-9.]/g, '');

    const parts = value.split('.');
    if (parts.length > 2) {
      value = parts[0] + '.' + parts.slice(1).join('');
    }
    
    let integerPart = parts[0];
    let decimalPart = parts.length > 1 ? parts[1] : '';

    if (integerPart) {
      // Parse as integer to remove leading zeros, then format with commas
      const num = parseInt(integerPart, 10);
      integerPart = isNaN(num) ? '' : num.toString();
      integerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }
    
    if (decimalPart.length > 2) {
      decimalPart = decimalPart.substring(0, 2);
    }

    let finalValue = integerPart;
    if (parts.length > 1) {
      finalValue += '.' + decimalPart;
    }

    // Set the formatted value back to the control and the input element
    this.linkForm.get('monto')?.setValue(finalValue, { emitEvent: false });
    inputElement.value = finalValue;
  }

  isDolarPayment(): boolean {
    return this.linkForm.value.pagarDolares || false;
  }

  maxMontoFormatted(): string {
    const symbol = this.isDolarPayment() ? '$' : 'Q';
    return `${symbol} ${this.maxMonto().toLocaleString('es-GT', { minimumFractionDigits: 2 })}`;
  }

  async onSearch() {
    if (this.searchForm.invalid) return;
    this.isSearching.set(true);
    this.localError.set(null);

    const cta = this.searchForm.value.numCuenta!;

    try {
      // 1. Get client by account
      const clientRes = await firstValueFrom(this.linkService.getClienteCta(cta));

      if (!clientRes.success || !clientRes.data) {
        this.showError(clientRes.errorMessage || 'El número de cuenta ingresado no pertenece a ningún cliente.');
        this.isSearching.set(false);
        return;
      }

      const client = clientRes.data;

      // 2. Check blacklist (Empresa "1")
      const blacklistRes = await firstValueFrom(this.linkService.isClienteListaNegra('1', client.codCliente));
      if (blacklistRes.success && blacklistRes.data === true) {
        this.showError(`Tu transacción no pudo ser procesada debido a que el método de pago utilizado fue rechazado. Por favor, acércate a una agencia para realizar el pago.)`);
        this.isSearching.set(false);
        return;
      }

      // 3. Not blacklisted. Set client and get all customer accounts
      this.selectedClient.set(client);

      const accountsRes = await firstValueFrom(this.linkService.getCuentasCliente(client.codCliente));
      this.isSearching.set(false);

      if (accountsRes.success && accountsRes.data) {
        // Cuentas maps as array of items, typically serialized by Oracle.
        // We expect items like: { numCuenta: string, tipo: string, estado: string }
        // Map correctly if it's strings or objects
        const accountsList = accountsRes.data.map((ac: any) => {
          if (typeof ac === 'string') {
            // fallback if only account numbers list
            return {
              numCuenta: ac,
              tipo: ac.length > 10 ? 'Tarjeta' : 'Prestamo',
              estado: 'ACTIVA'
            };
          }
          return {
            numCuenta: ac.numCuenta || ac.NUM_CUENTA || ac.num_cuenta || '',
            tipo: ac.tipo || ac.TIPO || '',
            estado: ac.estado || ac.ESTADO || 'ACTIVA'
          };
        });

        this.customerAccounts.set(accountsList);
        this.showCuentasModal.set(true);
      } else {
        this.showError(accountsRes.errorMessage || 'No se pudieron recuperar las cuentas asociadas al cliente.');
      }
    } catch (err) {
      this.isSearching.set(false);
      let errorMessage = 'Error de comunicación con el servidor al buscar cliente.';
      if (err instanceof HttpErrorResponse) {
        if (err.error && (err.error.errorMessage || err.error.message)) {
          errorMessage = err.error.errorMessage || err.error.message;
        }
      } else if (err instanceof Error) {
        errorMessage = err.message;
      }
      this.showError(errorMessage);
    }
  }

  selectAccount(ac: any) {
    this.selectedAccountInfo.set(ac);
    this.searchForm.patchValue({ numCuenta: ac.numCuenta });
    this.isProductSelected.set(true);
    this.closeCuentasModal();

    // Retrieve default phone & email
    this.loadAccountDetails(ac);
  }

  private loadAccountDetails(account: any) {
    const client = this.selectedClient()!;
    const isLoan = account.tipo !== 'Tarjeta';

    // Define all API calls to be made
    const email$ = this.linkService.getCorreoCliente(client.codCliente);
    const phone$ = this.linkService.getTelefonoCliente(client.codCliente);
    const limit$ = isLoan 
      ? this.linkService.getMontoPR(account.numCuenta) 
      : this.linkService.getMontoTC(account.numCuenta);
    const loanType$ = isLoan 
      ? this.linkService.getTipoPrestamo(account.numCuenta) 
      : of(null); // Return an empty observable if not a loan

    // Execute all calls in parallel
    forkJoin({
      email: email$,
      phone: phone$,
      limit: limit$,
      loanType: loanType$
    }).subscribe({
      next: (results) => {
        // Set email and phone
        if (results.email.success && results.email.data) this.defaultEmail.set(results.email.data);
        if (results.phone.success && results.phone.data) this.defaultPhone.set(results.phone.data);

        // Set max amount
        if (results.limit.success && results.limit.data) {
          this.maxMonto.set(results.limit.data);
        } else {
          this.maxMonto.set(0); // Set to 0 on failure
          this.ui.showError('No se pudo obtener el límite de monto para este producto.');
        }

        // Set currency for loans
        if (results.loanType && results.loanType.success && results.loanType.data) {
          const isUSD = results.loanType.data.moneda === '840';
          this.linkForm.patchValue({ pagarDolares: isUSD });
        } else {
          this.linkForm.patchValue({ pagarDolares: false });
        }
      },
      error: () => {
        // Handle any critical failure in forkJoin
        this.maxMonto.set(0);
        this.ui.showError('Error de comunicación al obtener los detalles de la cuenta.');
      }
    });
  }

  onMontoBlur() {
    this.localError.set(null);
    const control = this.linkForm.get('monto');
    if (!control) return;

    const val = this.parseCurrency(control.value);
    const max = this.maxMonto();

    if (val > max) {
      this.isMontoValido.set(false);
      this.showError(`El monto ingresado (${val}) excede el límite máximo permitido para este producto (${max}).`);
    } else if (val <= 0) {
      this.isMontoValido.set(false);
      this.showError(`El monto a pagar debe ser mayor a cero.`);
    } else {
      this.isMontoValido.set(true);
    }

    // Formatear el valor en el control para que se muestre correctamente en el input
    const formattedValue = this.formatCurrency(val);
    control.setValue(formattedValue);
  }

  setCanalEnvio(channel: string) {
    this.linkForm.patchValue({ tipEnvio: channel });
  }

  showError(msg: string) {
    this.localError.set(msg);
    this.ui.showModal(msg);
  }

  onSave() {
    if (this.linkForm.invalid || !this.isMontoValido()) return;
    
    const formVal = this.linkForm.value;
    const amountVal = this.parseCurrency(formVal.monto);
    if (amountVal <= 0) {
      this.isMontoValido.set(false);
      this.showError('El monto a pagar debe ser mayor a cero.');
      return;
    }
    
    this.isSaving.set(true);
    this.localError.set(null);

    const ac = this.selectedAccountInfo()!;
    const client = this.selectedClient()!;

    // Perform specific Loan Currency checks
    if (ac.tipo === 'Prestamo') {
      this.linkService.getTipoPrestamo(ac.numCuenta).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            if (res.data.moneda === '840' && !formVal.pagarDolares) {
              this.showError('Error: Debe seleccionar la opción de -Pagar en Dólares- para préstamos en dólares.');
              this.isSaving.set(false);
              return;
            }
            if (res.data.moneda === '320' && formVal.pagarDolares) {
              this.showError('Error: Solo se permite la opción de -Pagar en Dólares- para préstamos en dólares.');
              this.isSaving.set(false);
              return;
            }
            this.proceedSaveLink(formVal, ac, client);
          } else {
            this.showError('Inconsistencia: La cuenta se identificó como Préstamo, pero no se encontró información detallada del préstamo.'); // Mensaje para data: null
            this.isSaving.set(false);
          }
        }, // Captura errores HTTP (ej. 404, 500)
        error: (err: any) => {
          this.isSaving.set(false);
          let errMsg = 'Error al obtener detalles del préstamo.';

          if (err instanceof HttpErrorResponse) {
            if (err.status === 404) {
              errMsg = `La cuenta ${ac.numCuenta} no es de tipo Préstamo o no se encontraron sus detalles.`;
            } else if (err.error?.errorMessage) {
              errMsg = err.error.errorMessage;
            } else {
              errMsg = `Error de comunicación con el servidor (${err.status}): ${err.message}`;
            }
          } else if (err instanceof Error) {
            errMsg = err.message;
          }
          this.showError(`Error al validar préstamo: ${errMsg}`);
        }
      });
    } else {
      this.proceedSaveLink(formVal, ac, client);
    }
  }

  private proceedSaveLink(formVal: any, ac: any, client: ClientEntity) {
    // 20240312 Email & phone validation
    let phone = this.defaultPhone();
    let email = this.defaultEmail().toUpperCase();;

    if (formVal.esDefault === '2') {
      if (formVal.tipEnvio === '1') {
        phone = formVal.customTelefono;
        if (!phone || !/^[0-9+]+$/.test(phone)) {
          this.showError('ERROR: Se debe ingresar un valor de teléfono válido.');
          this.isSaving.set(false);
          return;
        }
        email = '';
      } else {
        email = formVal.customEmail.toUpperCase();
        if (!email || !/\S+@\S+\.\S+/.test(email)) {
          this.showError('ERROR: Se debe ingresar un correo electrónico válido.');
          this.isSaving.set(false);
          return;
        }
        phone='';
      }
    } else {
      // Default
      if (formVal.tipEnvio === '1'){
        if(!phone || phone.startsWith('Dato default no')) {
          this.showError('Se debe seleccionar otro medio de envío o digitar un número de teléfono.');
          this.isSaving.set(false);
          return;
        }else
          email = '';
      } else{
          if (!email || email.startsWith('Dato default no')) {
          this.showError('Se debe seleccionar otro medio de envío o digitar un correo electrónico.');
          this.isSaving.set(false);
          return;
        } else
            phone='';
      }
  }

    const payload: LinkEntity = {
      numCuenta: ac.numCuenta,
      tipCuenta: ac.tipo === 'Tarjeta' ? 'TC' : 'PR',
      monto: this.parseCurrency(formVal.monto) || 0,
      tipPago: formVal.pagarDolares ? '1' : '0',
      esDefault: formVal.esDefault,
      tipEnvio: formVal.tipEnvio,
      numTelefono: phone,
      nomCorreo: email,
      tipLink: formVal.tipLink,
      diaMes: formVal.tipLink === '1' ? String(formVal.diaMes) : '',
      nomProducto: ac.tipo === 'Tarjeta' ? 'Pago Tarjeta Promerica' : 'Pago Préstamo Promerica',
      codCliente: client.codCliente
    };

    // Remove prefix for base64 from system image if it exists
    const cleanImg = this.systemImageBase64().replace(/^data:image\/\w+;base64,/, '');

    this.linkService.emitirLink(payload, cleanImg).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        if (res.success && res.data) {
          this.generatedUrl.set(res.data);
          this.ui.showSuccess('Link generado y guardado exitosamente.');
        } else {
          this.showError(res.errorMessage || 'Ocurrió un error al registrar el link en Neo.');
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errMsg = err.error?.errorMessage || err.error?.message || err.message || 'Error en el servidor al intentar emitir el link.';
        this.showError(`Error al emitir el link: ${errMsg}`);
      }
    });
  }

  private fallbackCopyToClipboard(text: string): boolean {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.top = '0';
    textArea.style.left = '0';
    textArea.style.opacity = '0';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    try {
      return document.execCommand('copy');
    } catch (err) {
      return false;
    } finally {
      document.body.removeChild(textArea);
    }
  }

  async copyLink() {
    const url = this.generatedUrl();
    if (!url) return;

    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(url).then(() => this.ui.showSuccess('¡Enlace copiado al portapapeles!'), () => this.ui.showError('No se pudo copiar el enlace.'));
    } else if (this.fallbackCopyToClipboard(url)) {
      this.ui.showSuccess('¡Enlace copiado al portapapeles! (modo compatibilidad)');
    } else {
      this.ui.showError('La función de copiar no es compatible o fue bloqueada por su navegador.');
    }
  }

  resetForm() {
    this.generatedUrl.set(null);
    this.linkForm.reset({
      monto: this.formatCurrency(0),
      pagarDolares: false,
      tipLink: '2',
      diaMes: new Date().getDate(),
      tipEnvio: '2',
      esDefault: '1',
      customTelefono: '',
      customEmail: ''
    });
    this.resetSearch();
  }

  resetSearch() {
    this.isProductSelected.set(false);
    this.selectedClient.set(null);
    this.customerAccounts.set([]);
    this.selectedAccountInfo.set(null);
    this.defaultEmail.set('');
    this.defaultPhone.set('');
    this.maxMonto.set(999999);
    this.isMontoValido.set(true);
    this.searchForm.reset();
    this.linkForm.reset({
      monto: this.formatCurrency(0),
      pagarDolares: false,
      tipLink: '2',
      diaMes: new Date().getDate(),
      tipEnvio: '2',
      esDefault: '1',
      customTelefono: '',
      customEmail: ''
    });
    this.generatedUrl.set(null);
  }

  closeCuentasModal() {
    this.showCuentasModal.set(false);
  }

  closeCuentasModalCancel() {
    this.closeCuentasModal();
    this.resetSearch();
  }

  cancel() {
    this.router.navigate(['/home']);
  }
}
