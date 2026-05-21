import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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

      <!-- Alertas de Error/Éxito Locales -->
      @if (localError()) {
        <div class="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-xl flex items-start gap-3">
          <span class="text-red-500 mt-0.5">⚠️</span>
          <div class="flex-1">
            <h4 class="font-bold text-red-800 text-sm">Error de Validación</h4>
            <p class="text-red-700 text-xs mt-0.5">{{ localError() }}</p>
          </div>
          <button (click)="localError.set(null)" class="text-red-400 hover:text-red-600 text-lg font-bold">&times;</button>
        </div>
      }

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
                    type="number" 
                    step="0.01" 
                    formControlName="monto"
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
                  <option value="2">No (Emisión Única)</option>
                  <option value="1">Sí (Mensual Programado)</option>
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
                        placeholder="Ej: 50255555555"
                        class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
                    } @else {
                      <label for="customEmail" class="block text-xs font-bold text-gray-700 uppercase tracking-wider">Correo Electrónico a Enviar</label>
                      <input 
                        id="customEmail"
                        type="email"
                        formControlName="customEmail"
                        placeholder="cliente@ejemplo.com"
                        class="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-gray-800 font-medium">
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

  maxMonto = signal<number>(999999);
  isMontoValido = signal(true);
  generatedUrl = signal<string | null>(null);
  systemImageBase64 = signal<string>('');

  daysList = Array.from({ length: 31 }, (_, i) => i + 1);

  // Reactive Forms
  readonly searchForm = this.fb.group({
    numCuenta: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]]
  });

  readonly linkForm = this.fb.group({
    monto: [0, [Validators.required, Validators.min(0.01)]],
    pagarDolares: [false],
    tipLink: ['2'], // '2' = No, '1' = Si
    diaMes: [new Date().getDate().toString()],
    tipEnvio: ['2'], // '1' = SMS, '2' = Correo
    esDefault: ['1'], // '1' = Default, '2' = Editado
    customTelefono: [''],
    customEmail: ['']
  });

  constructor() {
    this.ui.title.set('Emisión de Links de Pago');
    this.loadSystemImage();
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

  isDolarPayment(): boolean {
    return this.linkForm.value.pagarDolares || false;
  }

  maxMontoFormatted(): string {
    const symbol = this.isDolarPayment() ? '$' : 'Q';
    return `${symbol} ${this.maxMonto().toLocaleString('es-GT', { minimumFractionDigits: 2 })}`;
  }

  onSearch() {
    if (this.searchForm.invalid) return;
    this.isSearching.set(true);
    this.localError.set(null);

    const cta = this.searchForm.value.numCuenta!;

    // 1. Get client by account
    this.linkService.getClienteCta(cta).subscribe({
      next: (clientRes) => {
        if (clientRes.success && clientRes.data) {
          const client = clientRes.data;
          
          // 2. Check blacklist (Empresa "1")
          this.linkService.isClienteListaNegra('1', client.codCliente).subscribe({
            next: (blacklistRes) => {
              if (blacklistRes.success && blacklistRes.data === true) {
                this.localError.set(`El cliente ${client.nomCliente} (${client.codCliente}) se encuentra en la Lista de Exclusiones/Lista Negra.`);
                this.isSearching.set(false);
                return;
              }

              // 3. Not blacklisted. Set client and get all customer accounts
              this.selectedClient.set(client);

              this.linkService.getCuentasCliente(client.codCliente).subscribe({
                next: (accountsRes) => {
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
                    this.localError.set('No se pudieron recuperar las cuentas asociadas al cliente.');
                  }
                },
                error: (err) => {
                  this.isSearching.set(false);
                  this.localError.set('Error al recuperar las cuentas del cliente.');
                }
              });
            },
            error: () => {
              this.isSearching.set(false);
              this.localError.set('Error al validar la lista negra del cliente.');
            }
          });

        } else {
          this.isSearching.set(false);
          this.localError.set(clientRes.errorMessage || 'El número de cuenta ingresado no pertenece a ningún cliente.');
        }
      },
      error: (err) => {
        this.isSearching.set(false);
        this.localError.set('Error de comunicación con el servidor al buscar cliente.');
      }
    });
  }

  selectAccount(ac: any) {
    this.selectedAccountInfo.set(ac);
    this.searchForm.patchValue({ numCuenta: ac.numCuenta });
    this.isProductSelected.set(true);
    this.closeCuentasModal();

    // Retrieve default phone & email
    const client = this.selectedClient()!;
    this.linkService.getCorreoCliente(client.codCliente).subscribe({
      next: (res) => { if (res.success && res.data) this.defaultEmail.set(res.data); }
    });
    this.linkService.getTelefonoCliente(client.codCliente).subscribe({
      next: (res) => { if (res.success && res.data) this.defaultPhone.set(res.data); }
    });

    // Check maximum limits
    if (ac.tipo === 'Tarjeta') {
      this.linkService.getMontoTC(ac.numCuenta).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.maxMonto.set(res.data);
          }
        }
      });
    } else {
      this.linkService.getMontoPR(ac.numCuenta).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.maxMonto.set(res.data);
          }
        }
      });
      
      // Auto USD check depending on loan currency
      this.linkService.getTipoPrestamo(ac.numCuenta).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            // Moneda '840' = USD
            if (res.data.moneda === '840') {
              this.linkForm.patchValue({ pagarDolares: true });
            } else {
              this.linkForm.patchValue({ pagarDolares: false });
            }
          }
        }
      });
    }
  }

  onMontoBlur() {
    this.localError.set(null);
    const val = this.linkForm.value.monto || 0;
    const max = this.maxMonto();

    if (val > max) {
      this.isMontoValido.set(false);
      this.localError.set(`El monto ingresado (${val}) excede el límite máximo permitido para este producto (${max}).`);
    } else {
      this.isMontoValido.set(true);
    }
  }

  setCanalEnvio(channel: string) {
    this.linkForm.patchValue({ tipEnvio: channel });
  }

  onSave() {
    if (this.linkForm.invalid || !this.isMontoValido()) return;
    this.isSaving.set(true);
    this.localError.set(null);

    const formVal = this.linkForm.value;
    const ac = this.selectedAccountInfo()!;
    const client = this.selectedClient()!;

    // Perform specific Loan Currency checks
    if (ac.tipo === 'Prestamo') {
      this.linkService.getTipoPrestamo(ac.numCuenta).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            if (res.data.moneda === '840' && !formVal.pagarDolares) {
              this.localError.set('Error: Debe seleccionar la opción de -Pagar en Dólares- para préstamos en dólares.');
              this.isSaving.set(false);
              return;
            }
            if (res.data.moneda === '320' && formVal.pagarDolares) {
              this.localError.set('Error: Solo se permite la opción de -Pagar en Dólares- para préstamos en dólares.');
              this.isSaving.set(false);
              return;
            }
            this.proceedSaveLink(formVal, ac, client);
          } else {
            this.proceedSaveLink(formVal, ac, client);
          }
        },
        error: () => this.proceedSaveLink(formVal, ac, client)
      });
    } else {
      this.proceedSaveLink(formVal, ac, client);
    }
  }

  private proceedSaveLink(formVal: any, ac: any, client: ClientEntity) {
    // 20240312 Email & phone validation
    let phone = this.defaultPhone();
    let email = this.defaultEmail();

    if (formVal.esDefault === '2') {
      if (formVal.tipEnvio === '1') {
        phone = formVal.customTelefono;
        if (!phone || !/^[0-9+]+$/.test(phone)) {
          this.localError.set('ERROR: Se debe ingresar un valor de teléfono válido.');
          this.isSaving.set(false);
          return;
        }
      } else {
        email = formVal.customEmail;
        if (!email || !/\S+@\S+\.\S+/.test(email)) {
          this.localError.set('ERROR: Se debe ingresar un correo electrónico válido.');
          this.isSaving.set(false);
          return;
        }
      }
    } else {
      // Default
      if (formVal.tipEnvio === '1' && (!phone || phone.startsWith('Dato default no'))) {
        this.localError.set('Se debe seleccionar otro medio de envío o digitar un número de teléfono.');
        this.isSaving.set(false);
        return;
      }
      if (formVal.tipEnvio === '2' && (!email || email.startsWith('Dato default no'))) {
        this.localError.set('Se debe seleccionar otro medio de envío o digitar un correo electrónico.');
        this.isSaving.set(false);
        return;
      }
    }

    const payload: LinkEntity = {
      numCuenta: ac.numCuenta,
      tipCuenta: ac.tipo === 'Tarjeta' ? 'TC' : 'PR',
      monto: formVal.monto || 0,
      tipPago: formVal.pagarDolares ? '1' : '0',
      esDefault: formVal.esDefault,
      tipEnvio: formVal.tipEnvio,
      numTelefono: phone,
      nomCorreo: email,
      tipLink: formVal.tipLink,
      diaMes: formVal.tipLink === '1' ? formVal.diaMes : '',
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
          this.localError.set(res.errorMessage || 'Ocurrió un error al registrar el link en Visa.');
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.localError.set('Error en el servidor al intentar emitir el link.');
      }
    });
  }

  copyLink() {
    if (this.generatedUrl()) {
      navigator.clipboard.writeText(this.generatedUrl()!);
      this.ui.showSuccess('¡Enlace copiado al portapapeles!');
    }
  }

  resetForm() {
    this.generatedUrl.set(null);
    this.linkForm.reset({
      monto: 0,
      pagarDolares: false,
      tipLink: '2',
      diaMes: new Date().getDate().toString(),
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
