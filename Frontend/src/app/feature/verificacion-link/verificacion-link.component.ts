import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LinkService, LinkVerificaItem, PagoRequest } from '../../core/services/link.service';
import { UiService } from '../../core/services/ui.service';

@Component({
  selector: 'app-verificacion-link',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="max-w-6xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4">
        <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Verificación y Conciliación de Links</h1>
        <p class="text-gray-500 text-sm mt-1">Monitoree el estatus de los pagos y concilie autorizaciones de Visa con el sistema central.</p>
      </div>

      <!-- Buscador y Filtros -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex-1 max-w-md relative">
          <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center text-gray-400">🔍</span>
          <input 
            type="text" 
            [(ngModel)]="searchQuery"
            (ngModelChange)="onSearchChange()"
            placeholder="Buscar por cuenta, correlativo o SKU..."
            class="w-full pl-10 pr-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-sm">
        </div>

        <div class="flex items-center gap-3">
          <label class="text-xs font-bold text-gray-400 uppercase tracking-wider">Mostrar</label>
          <select 
            [(ngModel)]="pageSize" 
            (change)="loadLinks()" 
            class="px-3 py-2 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-[#7bc342]">
            <option [value]="10">10 registros</option>
            <option [value]="25">25 registros</option>
            <option [value]="50">50 registros</option>
          </select>
          
          <button 
            (click)="loadLinks()" 
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
                  <th class="px-6 py-4">Correlativo</th>
                  <th class="px-6 py-4">Cuenta / Producto</th>
                  <th class="px-6 py-4">SKU Visa</th>
                  <th class="px-6 py-4">Autorización</th>
                  <th class="px-6 py-4">Movimiento Core</th>
                  <th class="px-6 py-4">Estatus Local</th>
                  <th class="px-6 py-4 text-right">Acciones</th>
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
                    <td class="px-6 py-4 text-right">
                      @if (item.edit !== 'Pagado') {
                        <button 
                          (click)="verificarEnVisa(item)" 
                          class="px-3.5 py-1.5 bg-[#007139] hover:bg-[#007139]/90 text-white text-xs font-bold rounded-lg transition-all shadow-sm">
                          🔎 Verificar Visa
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

      <!-- Modal de Conciliación / Detalles Visa -->
      @if (selectedItem()) {
        <div class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-in fade-in duration-300">
          <div class="bg-white rounded-2xl shadow-2xl border border-gray-100 max-w-lg w-full overflow-hidden animate-in zoom-in-95 duration-200">
            <div class="bg-gradient-to-r from-gray-800 to-gray-900 px-6 py-4 text-white flex justify-between items-center">
              <h3 class="font-bold text-sm uppercase tracking-wider">Resultado Consulta Visa</h3>
              <button (click)="closeModal()" class="text-white opacity-70 hover:opacity-100 text-xl font-bold">×</button>
            </div>

            <div class="p-6 space-y-6">
              @if (isCheckingVisa()) {
                <div class="py-8 text-center space-y-3">
                  <div class="animate-spin rounded-full h-8 w-8 border-4 border-[#7bc342] border-t-transparent mx-auto"></div>
                  <p class="text-gray-500 text-sm font-semibold">Consultando API de Visa...</p>
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
                      <span class="font-bold text-gray-800 mt-1 block font-mono">Q/$. {{ visaDetails()?.monto | number:'1.2-2' }}</span>
                    </div>
                    <div class="bg-gray-50 p-3 rounded-xl">
                      <span class="text-xs font-bold text-gray-400 block uppercase font-mono">Estatus Visa</span>
                      <span class="font-bold mt-1 block" [class]="(visaDetails()?.estado === 'PAID' || visaDetails()?.estado === 'Pagado') ? 'text-emerald-600' : 'text-amber-600'">
                        {{ visaDetails()?.estado === 'PAID' ? 'Pagado' : (visaDetails()?.estado === 'PENDING' ? 'Pendiente' : visaDetails()?.estado) }}
                      </span>
                    </div>
                  </div>

                  @if (visaDetails()?.autorizacion) {
                    <!-- Pago Autorizado por Visa -->
                    <div class="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl flex gap-3">
                      <span class="text-emerald-500 text-xl">✅</span>
                      <div>
                        <h4 class="font-bold text-emerald-800 text-sm">Pago Autorizado en Visa</h4>
                        <p class="text-emerald-700 text-xs mt-0.5">Código de autorización: <strong class="font-mono text-sm">{{ visaDetails()?.autorizacion }}</strong></p>
                        <p class="text-emerald-600 text-xs mt-1">El cobro ya se realizó en Visa. Puede aplicar el pago al core bancario presionando el botón de abajo.</p>
                      </div>
                    </div>
                  } @else {
                    <!-- Link no pagado o fallido -->
                    <div class="p-4 bg-amber-50 border border-amber-200 rounded-2xl flex gap-3">
                      <span class="text-amber-500 text-xl">⚠️</span>
                      <div>
                        <h4 class="font-bold text-amber-800 text-sm">Pago no completado</h4>
                        <p class="text-amber-700 text-xs mt-0.5">El link aún no posee un número de autorización registrado en Visa.</p>
                      </div>
                    </div>
                  }
                </div>
              }

              <!-- Botones Modal -->
              <div class="border-t pt-4 flex justify-end gap-2 text-sm font-semibold">
                <button 
                  (click)="closeModal()" 
                  [disabled]="isApplying()"
                  class="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl transition-all">
                  Cerrar
                </button>
                @if (visaDetails()?.autorizacion && !isCheckingVisa()) {
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
export class VerificacionLinkComponent implements OnInit {
  private readonly linkService = inject(LinkService);
  private readonly ui = inject(UiService);

  // States
  links = signal<LinkVerificaItem[]>([]);
  isLoading = signal(false);
  
  // Pagination
  currentPage = signal(1);
  pageSize = 10;
  totalRecords = signal(0);
  searchQuery = '';

  // Modal details
  selectedItem = signal<LinkVerificaItem | null>(null);
  isCheckingVisa = signal(false);
  isApplying = signal(false);
  visaDetails = signal<any | null>(null);

  constructor() {
    this.ui.title.set('Listado y Verificación de Links');
  }

  ngOnInit() {
    this.loadLinks();
  }

  loadLinks() {
    this.isLoading.set(true);
    const start = (this.currentPage() - 1) * this.pageSize;

    const request = {
      draw: this.currentPage(),
      start: start,
      length: this.pageSize,
      search: {
        value: this.searchQuery,
        regex: false
      },
      order: [
        {
          column: 0,
          dir: 'desc'
        }
      ],
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
        this.isLoading.set(false);
        this.links.set(res.data);
        this.totalRecords.set(res.recordsFiltered);
      },
      error: () => {
        this.isLoading.set(false);
        this.ui.showError('Error al cargar la lista de verificación.');
      }
    });
  }

  onSearchChange() {
    this.currentPage.set(1);
    this.loadLinks();
  }

  // Pagination getters
  startRecord() {
    return (this.currentPage() - 1) * this.pageSize + 1;
  }

  endRecord() {
    const end = this.currentPage() * this.pageSize;
    return end > this.totalRecords() ? this.totalRecords() : end;
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.loadLinks();
    }
  }

  nextPage() {
    if (this.endRecord() < this.totalRecords()) {
      this.currentPage.set(this.currentPage() + 1);
      this.loadLinks();
    }
  }

  // Verification & Action
  verificarEnVisa(item: LinkVerificaItem) {
    this.selectedItem.set(item);
    this.isCheckingVisa.set(true);
    this.visaDetails.set(null);

    this.linkService.validarYConsultaLink(item.codigoVisa).subscribe({
      next: (res) => {
        this.isCheckingVisa.set(false);
        if (res.success && res.data) {
          this.visaDetails.set(res.data);
        } else {
          this.ui.showError(res.errorMessage || 'Link no encontrado en Visa.');
          this.closeModal();
        }
      },
      error: () => {
        this.isCheckingVisa.set(false);
        this.ui.showError('Error de comunicación con el servicio de Visa.');
        this.closeModal();
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
      autVisa: details.autorizacion
    };

    this.linkService.aplicarPago(pago).subscribe({
      next: (res) => {
        this.isApplying.set(false);
        if (res.success) {
          this.ui.showSuccess(res.message || 'Se efectuó de forma exitosa el pago.');
          this.closeModal();
          this.loadLinks(); // Refresh table
        } else {
          this.ui.showError(res.errorMessage || 'No se pudo aplicar el pago en el Core.');
        }
      },
      error: (err) => {
        this.isApplying.set(false);
        this.ui.showError(err.error?.errorMessage || 'Error de comunicación al aplicar el pago.');
      }
    });
  }

  closeModal() {
    this.selectedItem.set(null);
    this.visaDetails.set(null);
  }

  // Export and Print features
  copyToClipboard() {
    if (!this.links() || this.links().length === 0) {
      this.ui.showError('No hay datos para copiar');
      return;
    }
    
    const headers = ['Correlativo', 'Cuenta / Producto', 'SKU Visa', 'Autorización', 'Movimiento Core', 'Estatus Local'];
    const rows = this.links().map(l => [
      l.correlativo, 
      l.producto, 
      l.codigoVisa, 
      l.numAuto, 
      l.numMov, 
      l.edit === 'Pagado' ? 'Conciliado' : 'Por verificar'
    ]);
    
    const tsvContent = [headers.join('\t'), ...rows.map(r => r.join('\t'))].join('\n');
    
    navigator.clipboard.writeText(tsvContent).then(() => {
      this.ui.showSuccess('Datos copiados al portapapeles');
    }).catch(() => {
      this.ui.showError('Error al copiar al portapapeles');
    });
  }

  exportToExcel() {
    if (!this.links() || this.links().length === 0) {
      this.ui.showError('No hay datos para exportar');
      return;
    }
    
    const headers = ['Correlativo', 'Cuenta / Producto', 'SKU Visa', 'Autorizacion', 'Movimiento Core', 'Estatus Local'];
    const rows = this.links().map(l => [
      l.correlativo, 
      l.producto, 
      l.codigoVisa, 
      l.numAuto, 
      l.numMov, 
      l.edit === 'Pagado' ? 'Conciliado' : 'Por verificar'
    ]);
    
    // Agregamos BOM para que Excel reconozca correctamente el UTF-8
    const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.map(cell => `"${(cell || '').toString().replace(/"/g, '""')}"`).join(','))].join('\n');
    
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `Verificacion_Links_${new Date().toISOString().split('T')[0]}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  print() {
    if (!this.links() || this.links().length === 0) {
      this.ui.showError('No hay datos para imprimir');
      return;
    }

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.ui.showError('Por favor, permita las ventanas emergentes para imprimir.');
      return;
    }

    const now = new Date();
    const dateStr = now.toLocaleDateString();
    const timeStr = now.toLocaleTimeString();

    let html = `
      <!DOCTYPE html>
      <html lang="es">
      <head>
        <meta charset="UTF-8">
        <title>Impresión - Verificación de Links</title>
        <style>
          body { font-family: Arial, sans-serif; margin: 20px; color: #333; }
          .header { text-align: center; margin-bottom: 20px; }
          h1 { color: #007139; margin-bottom: 5px; font-size: 24px; }
          .info { display: flex; justify-content: space-between; font-size: 14px; color: #666; margin-bottom: 15px; border-bottom: 1px solid #ccc; padding-bottom: 10px; }
          table { width: 100%; border-collapse: collapse; font-size: 12px; }
          th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
          th { background-color: #f2f2f2; color: #333; text-transform: uppercase; font-size: 11px; }
        </style>
      </head>
      <body>
        <div class="header">
          <h1>Verificación y Conciliación de Links</h1>
        </div>
        <div class="info">
          <span><strong>Fecha:</strong> ${dateStr}</span>
          <span><strong>Hora:</strong> ${timeStr}</span>
        </div>
        <table>
          <thead>
            <tr>
              <th>Correlativo</th>
              <th>Cuenta / Producto</th>
              <th>SKU Visa</th>
              <th>Autorización</th>
              <th>Movimiento Core</th>
              <th>Estatus Local</th>
            </tr>
          </thead>
          <tbody>
    `;

    this.links().forEach(item => {
      const estatus = item.edit === 'Pagado' ? 'Conciliado' : 'Por verificar';
      
      html += `
        <tr>
          <td>${item.correlativo}</td>
          <td>${item.producto}</td>
          <td>${item.codigoVisa}</td>
          <td>${item.numAuto}</td>
          <td>${item.numMov}</td>
          <td>${estatus}</td>
        </tr>
      `;
    });

    html += `
          </tbody>
        </table>
        <script>
          window.onload = function() {
            setTimeout(function() {
              window.print();
              window.onafterprint = function() { window.close(); }
            }, 250);
          }
        </script>
      </body>
      </html>
    `;

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();
  }
}
