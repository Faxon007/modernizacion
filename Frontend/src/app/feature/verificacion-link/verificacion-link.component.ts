import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LinkService, LinkVerificaItem, PagoRequest } from '../../core/services/link.service';
import { UiService } from '../../core/services/ui.service'; // Corregido
import { AuthService } from '../../core/services/auth.service'; // Corregido
import * as XLSX from 'xlsx';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, tap, switchMap, finalize, catchError, EMPTY, merge } from 'rxjs'; 
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-verificacion-link',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="max-w-6xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4">
        <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Verificación y Conciliación de Links</h1>
        <p class="text-gray-500 text-sm mt-1">Monitoree el estatus de los pagos y concilie autorizaciones de Neo con el sistema central.</p>
      </div>

      <!-- Buscador y Filtros -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex-1 max-w-md relative">
          <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center text-gray-400">🔍</span>
          <input 
            type="text"
            [formControl]="searchControl"
            placeholder="Buscar por cuenta, correlativo o SKU..."
            class="w-full pl-10 pr-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-sm">
        </div>

        <div class="flex items-center gap-3">
          <label class="text-xs font-bold text-gray-400 uppercase tracking-wider">Mostrar</label>
          <select 
            [formControl]="pageSizeControl"
            (change)="refreshData()" 
            class="px-3 py-2 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-[#7bc342]">
            <option [value]="10">10 registros</option>
            <option [value]="25">25 registros</option>
            <option [value]="50">50 registros</option>
            <option [value]="100">100 registros</option>
          </select>
          
          <button 
            (click)="refreshData()" 
            class="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold rounded-xl text-sm transition-all flex items-center gap-1.5 print:hidden">
            🔄 Actualizar
          </button>

          <div class="h-6 w-px bg-gray-200 hidden md:block print:hidden"></div>

          <button 
            (click)="copyToClipboard()" 
            class="px-4 py-2.5 bg-blue-50 text-blue-700 hover:bg-blue-100 font-bold rounded-xl text-sm transition-all flex items-center gap-1.5 print:hidden" title="Copiar al portapapeles">
            📋 Copiar
          </button>
          <button 
            (click)="exportToExcel()" 
            class="px-4 py-2.5 bg-green-50 text-green-700 hover:bg-green-100 font-bold rounded-xl text-sm transition-all flex items-center gap-1.5 print:hidden" title="Exportar a Excel">
            📊 Excel
          </button>
          <button 
            (click)="print()" 
            class="px-4 py-2.5 bg-gray-50 text-gray-700 hover:bg-gray-100 font-bold rounded-xl text-sm transition-all flex items-center gap-1.5 border border-gray-200 print:hidden" title="Imprimir">
            🖨️ Imprimir
          </button>
        </div>
      </div>

      <!-- Tabla Principal -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        @if (isLoading()) {
          <div class="p-12 text-center space-y-3">
            <div class="animate-spin rounded-full h-8 w-8 border-4 border-[#007139] border-t-transparent mx-auto"></div>
            <p class="text-gray-400 text-sm font-semibold">Cargando links...</p>
          </div>
        } @else if (links().length === 0) {
          <div class="p-12 text-center text-gray-400">
            <span class="text-4xl block mb-2">📂</span>
            <p class="text-sm font-semibold">No se encontraron links para verificar.</p>
          </div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-gray-50 text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-100">
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('CORRELATIVO')">Correlativo</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('PRODUCTO')">Cuenta / Producto</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('CODIGO_VISA')">SKU Neo</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('NUM_AUTO')">Autorización</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('NUM_MOV')">Movimiento Core</th>
                  <th class="px-6 py-4">Estatus Local</th>
                  <th class="px-6 py-4 text-right print:hidden">Acciones</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100 text-sm">
                @for (item of links(); track item.correlativo) {
                  <tr class="hover:bg-gray-50/50 transition-colors">
                    <td class="px-6 py-4 font-mono font-bold text-gray-600">#{{ item.correlativo }}</td>
                    <td class="px-6 py-4 font-mono font-semibold text-gray-700">{{ item.producto }}</td>
                    <td class="px-6 py-4 font-mono text-gray-500 text-xs">{{ item.codigoVisa }}</td>
                    <td class="px-6 py-4">
                      @if (item.numAuto === 'Pendiente') {
                        <span class="text-gray-400 italic text-xs">Pendiente</span>
                      } @else {
                        <span class="px-2 py-0.5 bg-emerald-50 text-emerald-700 border border-emerald-100 rounded text-xs font-bold font-mono">{{ item.numAuto }}</span>
                      }
                    </td>
                    <td class="px-6 py-4">
                      @if (item.numMov === 'Pendiente') {
                        <span class="text-gray-400 italic text-xs">Pendiente</span>
                      } @else {
                        <span class="font-mono text-gray-700 font-semibold">{{ item.numMov }}</span>
                      }
                    </td>
                    <td class="px-6 py-4">
                      @if (item.edit === 'Pagado') {
                        <span class="px-2.5 py-0.5 bg-emerald-100 text-emerald-800 rounded-full text-xs font-bold">Conciliado</span>
                      } @else {
                        <span class="px-2.5 py-0.5 bg-amber-100 text-amber-800 rounded-full text-xs font-bold">Por verificar</span>
                      }
                    </td>
                    <td class="px-6 py-4 text-right print:hidden">
                      @if (item.edit !== 'Pagado') {
                        <button 
                          (click)="verificarEnVisa(item)" 
                          class="px-3.5 py-1.5 bg-[#007139] hover:bg-[#007139]/90 text-white text-xs font-bold rounded-lg transition-all shadow-sm">
                          🔎 Verificar Neo
                        </button>
                      } @else {
                        <span class="text-gray-400 text-xs italic">Ninguna acción</span>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- Paginación -->
          <div class="px-6 py-4 border-t border-gray-100 flex items-center justify-between text-xs text-gray-500 font-bold uppercase tracking-wider">
            <span>Mostrando {{ startRecord() }} a {{ endRecord() }} de {{ totalRecords() }} registros</span>
            <div class="flex gap-2">
              <button 
                [disabled]="currentPage() === 1"
                (click)="prevPage()"
                class="px-3 py-1.5 bg-gray-50 hover:bg-gray-100 disabled:opacity-50 border rounded-lg transition-all">
                ◀ Ant
              </button>
              <button 
                [disabled]="endRecord() >= totalRecords()"
                (click)="nextPage()"
                class="px-3 py-1.5 bg-gray-50 hover:bg-gray-100 disabled:opacity-50 border rounded-lg transition-all">
                Sig ▶
              </button>
            </div>
          </div>
        }
      </div>

      <!-- Modal de Conciliación / Detalles Neo -->
      @if (selectedItem()) {
        <div class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-in fade-in duration-300">
          <div class="bg-white rounded-2xl shadow-2xl border border-gray-100 max-w-lg w-full overflow-hidden animate-in zoom-in-95 duration-200">
            <div class="bg-gradient-to-r from-gray-800 to-gray-900 px-6 py-4 text-white flex justify-between items-center">
              <h3 class="font-bold text-sm uppercase tracking-wider">Resultado Consulta Neo</h3>
              <button (click)="closeModal()" class="text-white opacity-70 hover:opacity-100 text-xl font-bold">×</button>
            </div>

            <div class="p-6 space-y-6">
              @if (isCheckingVisa()) {
                <div class="py-8 text-center space-y-3">
                  <div class="animate-spin rounded-full h-8 w-8 border-4 border-[#7bc342] border-t-transparent mx-auto"></div>
                  <p class="text-gray-500 text-sm font-semibold">Consultando API de Neo...</p>
                </div>
              } @else if (!visaDetails()) {
                <!-- Vista inicial del modal con detalles locales -->
                <div class="space-y-4">
                  <div class="grid grid-cols-2 gap-4 text-sm">
                    <div class="bg-gray-50 p-3 rounded-xl col-span-2">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Producto / Cuenta</span>
                      <span class="font-semibold text-gray-800 mt-1 block">{{ selectedItem()?.producto }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Código NeoLink</span>
                      <span class="font-semibold text-gray-800 mt-1 block break-all">{{ selectedItem()?.codigoVisa }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Correlativo</span>
                      <span class="font-semibold text-gray-800 mt-1 block">#{{ selectedItem()?.correlativo }}</span>
                    </div>
                  </div>
                  
                  <div class="p-4 bg-blue-50 border border-blue-200 rounded-2xl flex gap-3">
                    <span class="text-blue-500 text-xl">ℹ️</span>
                    <div>
                      <h4 class="font-bold text-blue-800 text-sm">Consulta de Estatus</h4>
                      <p class="text-blue-700 text-xs mt-0.5">Haga clic en 'Consultar Neo' para verificar el estado de este link en la plataforma de Neo en Link y buscar su número de autorización.</p>
                    </div>
                  </div>
                </div>
              } @else if (visaDetails()) {
                <div class="space-y-4">
                  <div class="grid grid-cols-2 gap-4 text-sm">
                    <div class="bg-gray-50 p-3 rounded-xl col-span-2">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Producto</span>
                      <span class="font-semibold text-gray-800 mt-1 block">{{ selectedItem()?.producto }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Código NeoLink</span>
                      <span class="font-semibold text-gray-800 mt-1 block break-all">{{ selectedItem()?.codigoVisa }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase">ID</span>
                      <span class="font-semibold text-gray-800 mt-1 block">{{ selectedItem()?.correlativo }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase">Monto</span>
                      <span class="font-bold text-gray-800 mt-1 block font-mono">
                        {{ visaDetails()?.moneda === 'Q' ? 'Q' : '$' }}. {{ visaDetails()?.monto | number:'1.2-2' }}
                      </span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase font-mono">Estatus Neo</span>
                      <span class="font-bold mt-1 block" [class]="(visaDetails()?.estado === 'PAID' || visaDetails()?.estado === 'Pagado') ? 'text-emerald-600' : 'text-amber-600'">
                        {{ visaDetails()?.estado === 'PAID' ? 'Pagado' : (visaDetails()?.estado === 'PENDING' ? 'Pendiente' : visaDetails()?.estado) }}
                      </span>
                    </div>
                  </div>

                  @if (visaDetails()?.ventas && visaDetails()?.ventas.length > 0) { 
                    <!-- Pago Autorizado por Neo -->
                    <div class="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl flex gap-3">
                      <span class="text-emerald-500 text-xl">✅</span>
                      <div>
                        <h4 class="font-bold text-emerald-800 text-sm">Pago Autorizado en Neo</h4>
                        <p class="text-emerald-700 text-xs mt-0.5">Código de autorización: <strong class="font-mono text-sm">{{ visaDetails()?.ventas[0]?.autorizacion }}</strong></p>
                        <p class="text-emerald-600 text-xs mt-1">El cobro ya se realizó en Neo. Puede aplicar el pago al core bancario presionando el botón de abajo.</p>
                      </div>
                    </div>
                  } @else {
                    <!-- Link no pagado o fallido -->
                    <div class="p-4 bg-amber-50 border border-amber-200 rounded-2xl flex gap-3">
                      <span class="text-amber-500 text-xl">⚠️</span>
                      <div>
                        <h4 class="font-bold text-amber-800 text-sm">Pago no completado</h4>
                        <p class="text-amber-700 text-xs mt-0.5">El link aún no posee un número de autorización registrado en Neo.</p>
                      </div>
                    </div>
                  }
                </div>
              }

              <!-- Botones Modal -->
              <div class="border-t pt-4 flex justify-end gap-2 text-sm font-semibold">
                <button 
                  (click)="closeModal()" 
                  [disabled]="isApplying() || isCheckingVisa()"
                  class="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl transition-all">
                  Cerrar
                </button>
                @if (!visaDetails() && !isCheckingVisa()) {
                  <button 
                    (click)="consultarEnVisa()" 
                    class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl transition-all shadow-md shadow-blue-600/10 flex items-center gap-1.5">
                    🔎 Consultar Neo
                  </button>
                }
                @if (visaDetails()?.ventas && visaDetails()?.ventas.length > 0 && !isCheckingVisa()) {
                  <button 
                    (click)="aplicarPagoCore()" 
                    [disabled]="isApplying()"
                    class="px-5 py-2 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl transition-all shadow-md shadow-[#007139]/10 flex items-center gap-1.5">
                    @if (isApplying()) {
                      <div class="animate-spin rounded-full h-3 w-4 border-2 border-white border-t-transparent"></div>
                      <span>Aplicando...</span>
                    } @else {
                      <span>⚡ Aplicar Pago al Core</span>
                    }
                  </button>
                }
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class VerificacionLinkComponent implements OnInit, OnDestroy {
  private readonly linkService = inject(LinkService);
  private readonly ui = inject(UiService);
  private readonly authService = inject(AuthService);
  private readonly destroy$ = new Subject<void>();
  private readonly refresh$ = new Subject<void>();

  // States
  links = signal<LinkVerificaItem[]>([]);
  isLoading = signal(false);
  
  // Pagination
  currentPage = signal(1);
  totalRecords = signal(0);
  sortColumn = signal('CORRELATIVO');
  sortDirection = signal<'asc' | 'desc'>('desc');
  
  searchControl = new FormControl('');
  pageSizeControl = new FormControl(10);

  // Modal details
  selectedItem = signal<LinkVerificaItem | null>(null);
  isCheckingVisa = signal(false);
  isApplying = signal(false);
  visaDetails = signal<any | null>(null);

  constructor() {
    this.ui.title.set('Listado y Verificación de Links');
  }

  ngOnInit() {
    const search$ = this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      tap(() => {
        this.currentPage.set(1); // Resetea a la primera página en cada nueva búsqueda
      })
    );

    const pageSize$ = this.pageSizeControl.valueChanges.pipe(
      distinctUntilChanged(),
      tap(() => {
        this.currentPage.set(1); // Resetea a la primera página cuando cambia el tamaño
      })
    );

    merge(search$, pageSize$, this.refresh$).pipe(
      tap(() => {
        this.isLoading.set(true);
      }),
      switchMap(() => this.loadLinks()),
      takeUntil(this.destroy$)
    ).subscribe();

    this.refreshData(); // Carga inicial
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLinks() {
    this.isLoading.set(true);
    const pageSize = this.pageSizeControl.value || 10;
    const start = (this.currentPage() - 1) * pageSize;
    const searchQuery = this.searchControl.value || '';

    const request = {
      draw: this.currentPage(),
      start: start,
      length: pageSize,
      search: {
        value: searchQuery,
        regex: false
      },
      order: [
        {
          column: 0, // El backend usa el 'name' de la columna, no el índice.
          dir: this.sortDirection()
        }
      ],
      columns: [
        { name: this.sortColumn(), searchable: true, orderable: true },
        { name: 'PRODUCTO', searchable: true, orderable: true },
        { name: 'CODIGO_VISA', searchable: true, orderable: true },
        { name: 'NUM_AUTO', searchable: false, orderable: false },
        { name: 'NUM_MOV', searchable: false, orderable: false }
      ]
    };

    return this.linkService.getLinksVerifica(request).pipe(
      tap(res => {
        this.links.set(res.data);
        this.totalRecords.set(res.recordsFiltered);
      }),
      finalize(() => this.isLoading.set(false)),
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401) {
          this.ui.showError('Su sesión ha expirado. Será redirigido al login.');
          this.authService.logout();
        } else {
          this.ui.showError('Error al cargar la lista de verificación.');
        }
        return EMPTY;
      })
    );
  }

  // Pagination getters
  startRecord() {
    const pageSize = this.pageSizeControl.value || 10;
    if (this.totalRecords() === 0) return 0;
    return (this.currentPage() - 1) * pageSize + 1;
  }

  endRecord() {
    const pageSize = this.pageSizeControl.value || 10;
    const end = this.currentPage() * pageSize;
    return end > this.totalRecords() ? this.totalRecords() : end;
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.refreshData();
    }
  }

  nextPage() {
    if (this.endRecord() < this.totalRecords()) {
      this.currentPage.set(this.currentPage() + 1);
      this.refreshData();
    }
  }

  refreshData() {
    this.refresh$.next();
  }

  sort(column: string) {
    if (this.sortColumn() === column) {
      // Si es la misma columna, invertir la dirección
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      // Si es una nueva columna, establecerla y ordenar ascendentemente por defecto
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
    this.refreshData();
  }

  // Verification & Action
  verificarEnVisa(item: LinkVerificaItem) {
    this.selectedItem.set(item);
    this.isCheckingVisa.set(false);
    this.visaDetails.set(null);
  }

  consultarEnVisa() {
    const item = this.selectedItem();
    if (!item) return;

    this.isCheckingVisa.set(true);
    this.visaDetails.set(null);

    this.linkService.validarYConsultaLink(item.codigoVisa).subscribe({
      next: (res) => {
        this.isCheckingVisa.set(false);
        if (res.success && res.data) {
          this.visaDetails.set(res.data);
        } else {
          this.ui.showModal(this.cleanOracleError(res.errorMessage || 'Link no encontrado en Neo.'));
        }
      },
      error: (err) => {
        this.isCheckingVisa.set(false);
        const msg = err.error?.message || err.error?.errorMessage || 'Error de comunicación con el servicio de Neo.';
        this.ui.showModal(this.cleanOracleError(msg));
      }
    });
  }

  aplicarPagoCore() {
    const item = this.selectedItem();
    const details = this.visaDetails();
    if (!item || !details) return;

    this.isApplying.set(true);

    const pago: PagoRequest = {
      numCta: item.producto,
      codSku: item.codigoVisa,
      codLink: item.correlativo,
      autVisa: details.ventas[0]?.autorizacion
    };

    this.linkService.aplicarPago(pago).subscribe({
      next: (res) => {
        this.isApplying.set(false);
        if (res.success) {
          this.ui.showSuccess(res.message || 'Se efectuó de forma exitosa el pago.');
          this.closeModal();
          this.refreshData(); // Usar refreshData() para disparar la recarga a través del pipe principal
        } else {
          const rawError = res.errorMessage || res.message || 'No se pudo aplicar el pago en el Core.';
          this.ui.showModal(this.cleanOracleError(rawError));
        }
      },
      error: (err) => {
        this.isApplying.set(false);
        const rawError = err.error?.message || err.error?.errorMessage || 'Error de comunicación al aplicar el pago.';
        this.ui.showModal(this.cleanOracleError(rawError));
      }
    });
  }

  private cleanOracleError(errorMsg: string): string {
    if (!errorMsg) return 'Error desconocido';
    
    const oraIndex = errorMsg.indexOf('ORA-');
    if (oraIndex > 0) {
      return errorMsg.substring(0, oraIndex).trim();
    } else if (oraIndex === 0) {
      const match = errorMsg.match(/ORA-\d+:\s*(.*)/);
      if (match && match[1]) {
        return match[1].split('ORA-')[0].split('\n')[0].trim();
      }
    }
    return errorMsg;
  }

  closeModal() {
    this.selectedItem.set(null);
    this.visaDetails.set(null);
  }

  // Export and Print features
  private getAllDataForExport(callback: (data: LinkVerificaItem[]) => void, onError?: () => void) {
    if (this.totalRecords() === 0) {
      this.ui.showError('No hay datos para exportar');
      if (onError) onError();
      return;
    }

    if (this.links().length === this.totalRecords()) {
      callback(this.links());
      return;
    }

    const request = {
      draw: 1,
      start: 0,
      length: -1, // -1 para obtener todos los registros
      search: { value: this.searchControl.value || '', regex: false },
      order: [{ column: 0, dir: 'desc' }],
      columns: [
        { name: 'CORRELATIVO', searchable: true, orderable: true },
        { name: 'PRODUCTO', searchable: true, orderable: true },
        { name: 'CODIGO_VISA', searchable: true, orderable: true },
        { name: 'NUM_AUTO', searchable: false, orderable: false },
        { name: 'NUM_MOV', searchable: false, orderable: false }
      ]
    };

    this.linkService.getLinksVerifica(request).subscribe({
      next: (res) => {
        callback(res.data);
      },
      error: () => {
        this.ui.showError('Error al obtener los datos completos para exportar.');
        if (onError) onError();
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

  async copyToClipboard() {
    this.getAllDataForExport(async (data) => {
      if (data.length === 0) {
        this.ui.showInfo('No hay datos para copiar.');
        return;
      }
      const headers = ['Correlativo', 'Cuenta / Producto', 'SKU Neo', 'Autorización', 'Movimiento Core', 'Estatus Local'];
      const rows = data.map(l => [l.correlativo, l.producto, l.codigoVisa, l.numAuto, l.numMov, l.edit === 'Pagado' ? 'Conciliado' : 'Por verificar'].join('\t'));
      const tsvContent = [headers.join('\t'), ...rows].join('\n');

      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(tsvContent).then(() => this.ui.showSuccess('Datos copiados al portapapeles'), () => this.ui.showError('Error al copiar al portapapeles'));
      } else if (this.fallbackCopyToClipboard(tsvContent)) {
        this.ui.showSuccess('Datos copiados (modo compatibilidad).');
      } else {
        this.ui.showError('La función de copiar no es compatible o fue bloqueada.');
      }
    });
  }

  exportToExcel() {
    this.getAllDataForExport((data) => {
      // Mapear los datos al formato deseado, tratando los números largos como texto.
      const dataToExport = data.map(item => ({
        'Correlativo': item.correlativo,
        'Cuenta / Producto': item.producto,
        'SKU Neo': item.codigoVisa,
        'Autorización': item.numAuto,
        'Movimiento Core': item.numMov,
        'Estatus Local': item.edit === 'Pagado' ? 'Conciliado' : 'Por verificar'
      }));

      // Crear la hoja de cálculo a partir de los datos JSON
      const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

      // Crear un nuevo libro de trabajo y añadir la hoja
      const wb: XLSX.WorkBook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'VerificacionLinks');

      const excelBuffer: any = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
      const blob = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.setAttribute('href', url);
      link.setAttribute('download', `Verificacion_Links_${new Date().toISOString().split('T')[0]}.xlsx`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    });
  }

  async print() {
    if (this.totalRecords() === 0) {
      this.ui.showError('No hay datos para imprimir');
      return;
    }

    this.ui.showInfo('Generando reporte para impresión...');

    this.getAllDataForExport((data) => {
      const iframe = document.createElement('iframe');
      iframe.style.position = 'absolute';
      iframe.style.width = '0';
      iframe.style.height = '0';
      iframe.style.border = '0';
      iframe.style.visibility = 'hidden';
      document.body.appendChild(iframe);

      const doc = iframe.contentWindow?.document;
      if (!doc) {
        this.ui.showError('No se pudo generar el documento para imprimir.');
        document.body.removeChild(iframe);
        return;
      }

      const now = new Date();
      const html = `
        <!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"><title>Verificación de Links</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 20px; color: #333; }
            h1 { color: #007139; font-size: 22px; }
            p { font-size: 12px; color: #666; }
            table { width: 100%; border-collapse: collapse; font-size: 10px; }
            th, td { border: 1px solid #ddd; padding: 6px; text-align: left; }
            th { background-color: #f2f2f2; text-transform: uppercase; }
            @media print { body { margin: 10px; } }
          </style>
        </head><body>
          <h1>Verificación y Conciliación de Links</h1>
          <p>Reporte generado el ${now.toLocaleDateString()} a las ${now.toLocaleTimeString()}</p>
          <table>
            <thead>
              <tr><th>Correlativo</th><th>Cuenta / Producto</th><th>SKU Neo</th><th>Autorización</th><th>Movimiento Core</th><th>Estatus Local</th></tr>
            </thead>
            <tbody>${data.map(item => `<tr><td>${item.correlativo}</td><td>${item.producto}</td><td>${item.codigoVisa}</td><td>${item.numAuto}</td><td>${item.numMov}</td><td>${item.edit === 'Pagado' ? 'Conciliado' : 'Por verificar'}</td></tr>`).join('')}</tbody>
          </table>
        </body></html>`;

      doc.open();
      doc.write(html);
      doc.close();

      iframe.contentWindow?.focus();
      iframe.contentWindow?.print();

      // Limpiar el iframe después de un tiempo prudencial
      setTimeout(() => document.body.removeChild(iframe), 1000);
    }, () => {
      // onError, no es necesario hacer nada extra ya que no hay ventana que cerrar
    });
  }
}
