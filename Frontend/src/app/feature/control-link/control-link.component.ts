import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { LinkService, DataTableResponse, LinkEntity } from '../../core/services/link.service';
import { UiService } from '../../core/services/ui.service';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, switchMap, tap, finalize, map, Observable } from 'rxjs';

export interface LinkListItem {
  correlativo: string;
  producto: string;
  monto: number;
  pago: string;
  emisionLink: string;
  usuario: string;
  envio: string;
  tipoLink: string;
}

@Component({
  selector: 'app-control-link',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  providers: [DecimalPipe],
  template: `
    <div class="max-w-6xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4">
        <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Control de Links</h1>
        <p class="text-gray-500 text-sm mt-1">Consulte y monitoree todos los links de cobro generados en el sistema.</p>
      </div>

      <!-- Buscador y Filtros -->
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex-1 max-w-md relative">
          <span class="absolute inset-y-0 left-0 pl-3.5 flex items-center text-gray-400">🔍</span>
          <input 
            type="text" 
            [formControl]="searchControl"
            placeholder="Buscar por cuenta, correlativo o producto..."
            class="w-full pl-10 pr-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#7bc342] focus:border-[#7bc342] transition-all text-sm">
        </div>

        <div class="flex items-center gap-3">
          <label class="text-xs font-bold text-gray-400 uppercase tracking-wider">Mostrar</label>
          <select 
            [(ngModel)]="pageSize" 
            (change)="manualLoad()" 
            class="px-3 py-2 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-[#7bc342]">
            <option [value]="10">10 registros</option>
            <option [value]="25">25 registros</option>
            <option [value]="50">50 registros</option>
            <option [value]="100">100 registros</option>
          </select>
          
          <button 
            (click)="manualLoad()" 
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
            <p class="text-gray-400 text-sm font-semibold">Cargando datos...</p>
          </div>
        } @else if (links().length === 0) {
          <div class="p-12 text-center text-gray-400">
            <span class="text-4xl block mb-2">📂</span>
            <p class="text-sm font-semibold">No se encontraron links con los filtros aplicados.</p>
          </div>
        } @else {
          <div class="overflow-x-auto print:overflow-visible print:w-full">
            <table class="w-full text-left border-collapse print:w-full print:table-fixed">
              <thead>
                <tr class="bg-gray-50 text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-100">
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('CORRELATIVO')">Correlativo</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('PRODUCTO')">Producto</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('MONTO')">Monto</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('PAGO')">Pago</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('EMISION_LINK')">Emisión</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('USUARIO')">Usuario</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('ENVIO')">Envío</th>
                  <th class="px-6 py-4 cursor-pointer hover:text-gray-600" (click)="sort('TIPO_LINK')">Tipo Link</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100 text-sm">
                @for (item of links(); track item.correlativo) {
                  <tr class="hover:bg-gray-50/50 transition-colors">
                    <td class="px-6 py-4 font-mono font-bold text-gray-600">#{{ item.correlativo }}</td>
                    <td class="px-6 py-4 font-mono font-semibold text-gray-700">{{ item.producto }}</td>
                    
                    <td class="px-6 py-4 font-mono text-gray-800 font-bold text-right">{{ item.pago.includes('Quetzales') ? 'Q.' : '$.' }} {{ item.monto | number:'1.2-2' }}</td>
                    <td class="px-6 py-4 text-gray-600">{{ item.pago }}</td>
                    <td class="px-6 py-4 font-mono text-gray-500 text-xs">{{ item.emisionLink }}</td>
                    <td class="px-6 py-4 text-gray-600">{{ item.usuario }}</td>
                    <td class="px-6 py-4 text-gray-600">{{ item.envio }}</td>
                    <td class="px-6 py-4">
                      @if (item.tipoLink === 'U') {
                        <span class="px-2 py-0.5 bg-blue-50 text-blue-700 border border-blue-100 rounded text-xs font-bold">Único</span>
                      } @else if (item.tipoLink === 'M') {
                        <span class="px-2 py-0.5 bg-purple-50 text-purple-700 border border-purple-100 rounded text-xs font-bold">Múltiple</span>
                      } @else {
                        <span class="px-2 py-0.5 bg-gray-100 text-gray-700 border border-gray-200 rounded text-xs font-bold">{{ item.tipoLink }}</span>
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
    </div>
  `
})
export class ControlLinkComponent implements OnInit, OnDestroy {
  private readonly linkService = inject(LinkService);
  private readonly ui = inject(UiService);
  private readonly destroy$ = new Subject<void>();

  // States
  links = signal<LinkListItem[]>([]);
  isLoading = signal(true);
  
  // Search and Filtering
  searchControl = new FormControl('');

  // Pagination and sorting
  currentPage = signal(1);
  pageSize = 25;
  totalRecords = signal(0);
  sortColumn = 'EMISION_LINK';
  sortDirection: 'asc' | 'desc' = 'desc';

  constructor() {
    this.ui.title.set('Control de Links');
  }

  ngOnInit() {
    this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      tap(() => {
        this.currentPage.set(1);
        this.isLoading.set(true);
      }),
      switchMap((query) => 
        this.loadLinks(query ?? '').pipe(
          map((res: any): any => ({
            ...res,
            data: (res.data || []).map((link: any) => ({
                                  correlativo: link.correlativo || 'N/A',
                    producto: link.producto  || 'N/A',
                    monto: link.monto || link.Monto || 0,
                    pago: link.pago || 'N/A',
                    emisionLink: link.emisionLink || 'N/A',
                    usuario: link.usuario || 'N/A',
                    envio: link.envio || link.envio || 'N/A',
                    tipoLink: link.tipLink || link.TipLink || 'U'
            }))
          }))
        )
      ),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (res) => {
        this.links.set(res.data);
        this.totalRecords.set(res.recordsFiltered);
      },
      error: () => this.ui.showError('Error al buscar la lista de links.'),
      complete: () => this.isLoading.set(false)
    });
    
    // Initial load
    this.searchControl.setValue('', { emitEvent: true });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLinks(query: string, length?: number): Observable<DataTableResponse<LinkListItem>> {
    this.isLoading.set(true);
    const pageSize = length !== undefined ? length : this.pageSize;
    const start = pageSize === -1 ? 0 : (this.currentPage() - 1) * this.pageSize;

    const request = {
      draw: this.currentPage(),
      start: start,
      length: pageSize,
      search: {
        value: query,
        regex: false
      },
      order: [{ column: 0, dir: this.sortDirection }],
      columns: [{ name: this.sortColumn, searchable: true, orderable: true }]
    };

     return (this.linkService.getLinks(request) as Observable<any>).pipe(
        map((res: any): DataTableResponse<LinkListItem> => {
            return {
                ...res,
                data: (res.data || []).map((link: any): LinkListItem => ({
                    correlativo: link.correlativo || 'N/A',
                    producto: link.producto  || 'N/A',
                    monto: link.monto || link.Monto || 0,
                    pago: link.pago || 'N/A',
                    emisionLink: link.emisionLink || 'N/A',
                    usuario: link.usuario || 'N/A',
                    envio: link.envio || link.envio || 'N/A',
                    tipoLink: link.tipLink || link.TipLink || 'U'
                }))
            };
        }),
        finalize(() => this.isLoading.set(false))
    );
  }
  
    manualLoad() {
    this.loadLinks(this.searchControl.value ?? '').pipe(
      takeUntil(this.destroy$) 
    ).subscribe({
        next: (res) => {
            this.links.set(res.data);
            this.totalRecords.set(res.recordsFiltered);
        },
        error: () => this.ui.showError('Error al recargar los links.')
    });
  }

  sort(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.manualLoad();
  }

  // Pagination getters
  startRecord() {
    if (this.totalRecords() === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize + 1;
  }

  endRecord() {
    const end = this.currentPage() * this.pageSize;
    return end > this.totalRecords() ? this.totalRecords() : end;
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.manualLoad();
    }
  }

  nextPage() {
    if (this.endRecord() < this.totalRecords()) {
      this.currentPage.set(this.currentPage() + 1);
      this.manualLoad();
    }
  }

  // Export and Print features... (rest of the file is unchanged)
  copyToClipboard() {
    const data = this.links();
    if (data.length === 0) {
      this.ui.showInfo('No hay datos para copiar.');
      return;
    }

    const headers = ['Correlativo', 'Producto', 'Monto', 'Pago', 'Emisión', 'Usuario', 'Envío', 'Tipo Link'];
    const rows = data.map(item => 
      [
        item.correlativo,
        item.producto,
        item.monto.toFixed(2),
        item.pago,
        item.emisionLink,
        item.usuario,
        item.envio,
        item.tipoLink
      ].join('\t')
    );

    const clipboardText = [headers.join('\t'), ...rows].join('\n');
    navigator.clipboard.writeText(clipboardText).then(() => {
      this.ui.showSuccess('Datos copiados al portapapeles.');
    }).catch(err => {
      this.ui.showError('No se pudo copiar al portapapeles.');
      console.error('Error al copiar: ', err);
    });
  }

  exportToExcel() {
    const data = this.links();
    if (data.length === 0) {
      this.ui.showInfo('No hay datos para exportar.');
      return;
    }

    const headers = 'Correlativo,Producto,Monto,Pago,Emision,Usuario,Envio,TipoLink\n';
    const rows = data.map(item => 
      [
        `"${item.correlativo}"`,
        `"${item.producto}"`,
        item.monto.toFixed(2),
        `"${item.pago}"`,
        `"${item.emisionLink}"`,
        `"${item.usuario}"`,
        `"${item.envio}"`,
        `"${item.tipoLink}"`
      ].join(',')
    ).join('\n');

    const csvContent = headers + rows;
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", `control_links_${new Date().toISOString().slice(0,10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  private getAllDataForExport(callback: (data: LinkListItem[]) => void, onError?: () => void) {
    if (this.totalRecords() === 0) {
      this.ui.showError('No hay datos para exportar');
      if (onError) onError();
      return;
    }

    // Si ya tenemos todos los datos cargados en la vista actual y coinciden con el total, los usamos.
    if (this.links().length === this.totalRecords()) {
      callback(this.links());
      return;
    }

    // Si no, hacemos una llamada para obtener todos los registros que coinciden con el filtro.
    this.loadLinks(this.searchControl.value || '', -1) // Usamos -1 para indicar que queremos todos los registros
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          callback(response.data);
        },
        error: () => {
          this.ui.showError('Error al obtener los datos completos para la operación.');
          if (onError) onError();
        }
      });
  }

  print() {
    if (this.totalRecords() === 0) {
      this.ui.showError('No hay datos para imprimir');
      return;
    }

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.ui.showError('Por favor, permita las ventanas emergentes para imprimir.');
      return;
    }

    printWindow.document.write('<html><head><title>Imprimiendo...</title></head><body><h2>Cargando datos para impresión...</h2></body></html>');

    this.getAllDataForExport((data: LinkListItem[]) => {
      const now = new Date();
      let html = `
        <!DOCTYPE html><html lang="es"><head><meta charset="UTF-8"><title>Control de Links</title>
        <style>
          body { font-family: Arial, sans-serif; margin: 20px; color: #333; }
          h1 { color: #007139; font-size: 22px; }
          p { font-size: 12px; color: #666; }
          table { width: 100%; border-collapse: collapse; font-size: 10px; }
          th, td { border: 1px solid #ddd; padding: 6px; text-align: left; }
          th { background-color: #f2f2f2; text-transform: uppercase; }
        </style>
        </head><body>
        <h1>Control de Links</h1>
        <p>Reporte generado el ${now.toLocaleDateString()} a las ${now.toLocaleTimeString()}</p>
        <table>
          <thead>
            <tr>
              <th>Correlativo</th><th>Producto</th><th>Monto</th><th>Pago</th><th>Emisión</th><th>Usuario</th><th>Envío</th><th>Tipo</th>
            </tr>
          </thead>
          <tbody>`;

      data.forEach((item: LinkListItem) => {
        const monto = `${item.pago.includes('Quetzales') ? 'Q' : '$'} ${item.monto.toFixed(2)}`;
        html += `<tr><td>${item.correlativo}</td><td>${item.producto}</td><td>${monto}</td><td>${item.pago}</td><td>${item.emisionLink}</td><td>${item.usuario}</td><td>${item.envio}</td><td>${item.tipoLink}</td></tr>`;
      });

      html += `
          </tbody>
        </table>
        <script>window.onload=function(){setTimeout(function(){window.print();window.onafterprint=function(){window.close()}},250)}</script>
        </body></html>`;

      printWindow.document.open();
      printWindow.document.write(html);
      printWindow.document.close();
    }, () => {
      // Si hay un error al obtener los datos, cerramos la ventana emergente.
      printWindow.close();
    });
  }
}
