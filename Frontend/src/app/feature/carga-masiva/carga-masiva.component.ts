import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LinkService, LinkEntity } from '../../core/services/link.service';
import { ParameterService } from '../../core/services/parameter.service';
import { UiService } from '../../core/services/ui.service';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { ApiResponse } from '../../core/models/api-response';

interface BulkRecord {
  index: number;
  tipoCuenta: 'PR' | 'TC';
  numCuenta: string;
  monto: number;
  tipoPago: '0' | '1'; // 0: Quetzal, 1: Dolar
  tipoLink: '1' | '2'; // 1: Automatico, 2: Manual
  diaMes: string;
  tipoEnvio: '1' | '2'; // 1: SMS, 2: Correo
  datoEnvio: string; // Correo o Telefono
  estado: 'Pendiente' | 'Procesando' | 'Exitoso' | 'Error';
  resultado: string;
  codSku?: string;
  urlCorto?: string;
}

@Component({
  selector: 'app-carga-masiva',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="max-w-6xl mx-auto space-y-6 animate-in fade-in duration-300">
      <!-- Encabezado -->
      <div class="border-b pb-4 flex justify-between items-center">
        <div>
          <h1 class="text-3xl font-extrabold text-[#007139] tracking-tight">Carga Masiva de Links</h1>
          <p class="text-gray-500 text-sm mt-1">Cargue plantillas CSV para emitir enlaces en lote y automatizar cobros.</p>
        </div>
        <button 
          (click)="downloadTemplate()" 
          class="px-4 py-2 bg-white border border-gray-200 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 transition-all flex items-center gap-2 text-sm shadow-sm">
          📥 Descargar Plantilla CSV
        </button>
      </div>

      <!-- Zona de Carga / Subida -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 flex flex-col justify-between space-y-4">
          <div class="space-y-2">
            <h3 class="font-bold text-gray-800 text-lg">Paso 1: Subir Archivo</h3>
            <p class="text-gray-500 text-xs">Cargue el archivo en formato CSV delimitado por comas con las columnas correctas.</p>
          </div>

          <!-- Input File Drag & Drop -->
          <div class="border-2 border-dashed border-gray-200 rounded-xl p-6 text-center hover:border-[#7bc342] transition-colors relative cursor-pointer bg-gray-50 group">
            <input 
              type="file" 
              accept=".csv"
              (change)="onFileSelected($event)"
              class="absolute inset-0 opacity-0 cursor-pointer">
            <div class="space-y-2">
              <span class="text-3xl block group-hover:scale-110 transition-transform">📄</span>
              <span class="text-xs font-bold text-gray-600 block">
                {{ selectedFileName() || 'Seleccione o arrastre un archivo CSV' }}
              </span>
              @if (records().length > 0) {
                <span class="text-[10px] bg-green-100 text-green-800 font-bold px-2 py-0.5 rounded-full">
                  {{ records().length }} registros cargados
                </span>
              }
            </div>
          </div>

          <div class="flex gap-2">
            <button 
              (click)="clearFile()"
              [disabled]="records().length === 0 || isProcessing()"
              class="w-1/3 py-2.5 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 text-gray-600 font-semibold rounded-xl text-sm transition-all">
              Limpiar
            </button>
            <button 
              (click)="procesarCarga()"
              [disabled]="records().length === 0 || isProcessing()"
              class="flex-1 py-2.5 bg-[#007139] hover:bg-[#007139]/90 disabled:opacity-50 text-white font-bold rounded-xl text-sm transition-all shadow-md shadow-[#007139]/10 flex items-center justify-center gap-2">
              @if (isProcessing()) {
                <div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                <span>Procesando...</span>
              } @else {
                <span>⚡ Procesar Lote</span>
              }
            </button>
          </div>
        </div>

        <!-- Progreso y Resumen -->
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-6 col-span-2 space-y-6">
          <h3 class="font-bold text-gray-800 text-lg">Resumen de Procesamiento</h3>
          
          <div class="grid grid-cols-4 gap-4 text-center">
            <div class="bg-gray-50 p-4 rounded-xl">
              <span class="text-xs font-bold text-gray-400 block uppercase">Total</span>
              <span class="text-2xl font-black text-gray-800 mt-1 block font-mono">{{ totalRecords() }}</span>
            </div>
            <div class="bg-blue-50 p-4 rounded-xl border border-blue-100">
              <span class="text-xs font-bold text-blue-400 block uppercase">Procesados</span>
              <span class="text-2xl font-black text-blue-700 mt-1 block font-mono">{{ processedCount() }}</span>
            </div>
            <div class="bg-emerald-50 p-4 rounded-xl border border-emerald-100">
              <span class="text-xs font-bold text-emerald-400 block uppercase">Exitosos</span>
              <span class="text-2xl font-black text-emerald-700 mt-1 block font-mono">{{ successCount() }}</span>
            </div>
            <div class="bg-rose-50 p-4 rounded-xl border border-rose-100">
              <span class="text-xs font-bold text-rose-400 block uppercase">Errores</span>
              <span class="text-2xl font-black text-rose-700 mt-1 block font-mono">{{ errorCount() }}</span>
            </div>
          </div>

          <!-- Barra de Progreso -->
          @if (isProcessing() || processedCount() > 0) {
            <div class="space-y-1">
              <div class="flex justify-between text-xs font-bold text-gray-500 uppercase tracking-wider">
                <span>Progreso General</span>
                <span>{{ progressPercentage() }}%</span>
              </div>
              <div class="w-full bg-gray-100 h-3 rounded-full overflow-hidden">
                <div 
                  class="bg-gradient-to-r from-[#7bc342] to-[#007139] h-full transition-all duration-300"
                  [style.width.%]="progressPercentage()"></div>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Tabla de Detalles de Registros -->
      @if (records().length > 0) {
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
          <div class="bg-gray-50 px-6 py-4 border-b flex justify-between items-center">
            <h3 class="font-bold text-gray-800">Registros del Archivo</h3>
            <div class="flex gap-2">
              <span class="px-2 py-1 bg-gray-100 rounded text-xs text-gray-600 font-semibold">Total: {{ records().length }}</span>
            </div>
          </div>

          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-gray-50/50 text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-100">
                  <th class="px-6 py-4">Fila</th>
                  <th class="px-6 py-4">Producto</th>
                  <th class="px-6 py-4">Cuenta</th>
                  <th class="px-6 py-4">Monto</th>
                  <th class="px-6 py-4">Moneda/Tipo</th>
                  <th class="px-6 py-4">Notificación</th>
                  <th class="px-6 py-4">Estatus</th>
                  <th class="px-6 py-4">Detalle / Resultado</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100 text-sm">
                @for (rec of records(); track rec.index) {
                  <tr class="hover:bg-gray-50/50 transition-colors">
                    <td class="px-6 py-4 font-mono font-bold text-gray-400">#{{ rec.index }}</td>
                    <td class="px-6 py-4">
                      <span [class]="rec.tipoCuenta === 'PR' ? 'bg-sky-50 text-sky-700 border-sky-200' : 'bg-purple-50 text-purple-700 border-purple-200'" class="px-2.5 py-0.5 border rounded-full text-xs font-bold">
                        {{ rec.tipoCuenta === 'PR' ? 'Préstamo' : 'Tarjeta' }}
                      </span>
                    </td>
                    <td class="px-6 py-4 font-mono font-semibold text-gray-700">{{ rec.numCuenta }}</td>
                    <td class="px-6 py-4 font-mono font-bold text-gray-800">
                      {{ rec.tipoPago === '1' ? '$' : 'Q' }} {{ rec.monto | number:'1.2-2' }}
                    </td>
                    <td class="px-6 py-4">
                      <div class="flex flex-col">
                        <span class="font-semibold text-xs text-gray-600">{{ rec.tipoPago === '1' ? 'Dólares' : 'Quetzales' }}</span>
                        <span class="text-[10px] text-gray-400">{{ rec.tipoLink === '1' ? 'Automático (Día ' + rec.diaMes + ')' : 'Manual (Único)' }}</span>
                      </div>
                    </td>
                    <td class="px-6 py-4">
                      <div class="flex flex-col text-xs">
                        <span class="font-bold text-gray-600">{{ rec.tipoEnvio === '1' ? 'SMS' : 'Correo' }}</span>
                        <span class="text-gray-400 font-mono">{{ rec.datoEnvio }}</span>
                      </div>
                    </td>
                    <td class="px-6 py-4">
                      @switch (rec.estado) {
                        @case ('Pendiente') {
                          <span class="px-2.5 py-0.5 bg-gray-100 text-gray-600 rounded-full text-xs font-bold uppercase">Pendiente</span>
                        }
                        @case ('Procesando') {
                          <span class="px-2.5 py-0.5 bg-blue-100 text-blue-700 rounded-full text-xs font-bold uppercase animate-pulse">Procesando</span>
                        }
                        @case ('Exitoso') {
                          <span class="px-2.5 py-0.5 bg-emerald-100 text-emerald-800 rounded-full text-xs font-bold uppercase">Exitoso</span>
                        }
                        @case ('Error') {
                          <span class="px-2.5 py-0.5 bg-rose-100 text-rose-800 rounded-full text-xs font-bold uppercase">Error</span>
                        }
                      }
                    </td>
                    <td class="px-6 py-4">
                      <div class="text-xs max-w-xs">
                        @if (rec.estado === 'Exitoso') {
                          <div class="flex flex-col">
                            <span class="text-emerald-700 font-semibold font-mono text-[11px]">SKU: {{ rec.codSku }}</span>
                            <a [href]="rec.urlCorto" target="_blank" class="text-blue-600 hover:underline text-[10px] truncate font-mono mt-0.5">{{ rec.urlCorto }}</a>
                          </div>
                        } @else if (rec.estado === 'Error') {
                          <span class="text-rose-600 font-semibold leading-tight block">{{ rec.resultado }}</span>
                        } @else {
                          <span class="text-gray-400 italic">Esperando procesamiento...</span>
                        }
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      }
    </div>
  `
})
export class CargaMasivaComponent implements OnInit {
  private readonly linkService = inject(LinkService);
  private readonly parameterService = inject(ParameterService);
  private readonly ui = inject(UiService);
  private readonly router = inject(Router);

  selectedFileName = signal<string | null>(null);
  records = signal<BulkRecord[]>([]);
  isProcessing = signal(false);
  
  // Resumen counters
  totalRecords = signal(0);
  processedCount = signal(0);
  successCount = signal(0);
  errorCount = signal(0);
  progressPercentage = signal(0);

  // Variable to store loaded ad image
  private apiImagenBase64 = '';

  constructor() {
    this.ui.title.set('Carga Masiva Parámetros');
  }

  ngOnInit() {
    // Cargar parámetros iniciales para obtener la imagen publicitaria
    this.parameterService.getParameters().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.apiImagenBase64 = res.data.apiImagenBase64 || '';
        }
      },
      error: () => {
        this.ui.showError('No se pudo cargar la imagen publicitaria configurada.');
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    this.selectedFileName.set(file.name);
    this.records.set([]);
    this.resetStats();

    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      this.parseCsvText(text);
    };
    reader.readAsText(file);
  }

  private parseCsvText(text: string) {
    const lines = text.split('\n');
    if (lines.length <= 1) {
      this.ui.showError('El archivo CSV está vacío o no contiene registros.');
      return;
    }

    const tempRecords: BulkRecord[] = [];
    let recordIndex = 1;

    for (let i = 1; i < lines.length; i++) {
      const line = lines[i].trim();
      if (!line) continue;

      // Split by comma
      const cols = line.split(',');
      if (cols.length < 8) {
        continue; // skip malformed lines
      }

      const tipoCuenta = cols[0].trim() as 'PR' | 'TC';
      const numCuenta = cols[1].trim();
      const monto = parseFloat(cols[2].trim()) || 0;
      const tipoPago = cols[3].trim() as '0' | '1';
      const tipoLink = cols[4].trim() as '1' | '2';
      const diaMes = cols[5].trim();
      const tipoEnvio = cols[6].trim() as '1' | '2';
      const datoEnvio = cols[7].trim();

      tempRecords.push({
        index: recordIndex++,
        tipoCuenta,
        numCuenta,
        monto,
        tipoPago,
        tipoLink,
        diaMes,
        tipoEnvio,
        datoEnvio,
        estado: 'Pendiente',
        resultado: ''
      });
    }

    this.records.set(tempRecords);
    this.totalRecords.set(tempRecords.length);
  }

  procesarCarga() {
    if (this.records().length === 0) return;
    this.isProcessing.set(true);
    this.resetStats();
    this.totalRecords.set(this.records().length);

    // Start sequential processing
    this.procesarFila(0);
  }

  private procesarFila(index: number) {
    const list = [...this.records()];
    if (index >= list.length) {
      this.isProcessing.set(false);
      this.ui.showSuccess('Procesamiento de carga masiva finalizado.');
      return;
    }

    const rec = list[index];
    rec.estado = 'Procesando';
    this.records.set(list);

    // 1. Validar cuenta y obtener cliente
    this.linkService.getClienteCta(rec.numCuenta).pipe(
      switchMap((clientRes) => {
        if (!clientRes.success || !clientRes.data) {
          throw new Error('La cuenta no existe en el sistema.');
        }

        const codCliente = clientRes.data.codCliente;
        // 2. Validar lista negra
        return this.linkService.isClienteListaNegra('1', codCliente).pipe(
          switchMap((blacklistRes) => {
            if (blacklistRes.success && blacklistRes.data) {
              throw new Error('El cliente de la cuenta se encuentra en lista negra.');
            }
            return of(clientRes.data);
          })
        );
      }),
      switchMap((clientData) => {
        // 3. Validar tipo de préstamo si aplica
        if (rec.tipoCuenta === 'PR') {
          return this.linkService.getTipoPrestamo(rec.numCuenta).pipe(
            switchMap((loanRes) => {
              if (loanRes.success && loanRes.data) {
                const mon = loanRes.data.moneda;
                if (mon === '840' && rec.tipoPago === '0') {
                  throw new Error('La opción de pago (GTQ) es para un préstamo en dólares.');
                }
                if (mon === '320' && rec.tipoPago === '1') {
                  throw new Error('La opción de pago (USD) es para un préstamo en quetzales.');
                }
              }
              return of(clientData);
            })
          );
        }
        return of(clientData);
      }),
      switchMap((clientData) => {
        // 4. Validar montos límites
        const limitCall = rec.tipoCuenta === 'PR' 
          ? this.linkService.getMontoPR(rec.numCuenta) 
          : this.linkService.getMontoTC(rec.numCuenta);

        return limitCall.pipe(
          switchMap((limitRes) => {
            if (limitRes.success && limitRes.data !== undefined) {
              if (rec.monto > limitRes.data) {
                throw new Error(`El monto supera el límite permitido (${rec.tipoPago === '1' ? '$' : 'Q'}. ${limitRes.data.toFixed(2)}).`);
              }
            }
            return of(clientData);
          })
        );
      }),
      switchMap((clientData) => {
        // 5. Preparar objeto y emitir link
        const link: LinkEntity = {
          numCuenta: rec.numCuenta,
          tipCuenta: rec.tipoCuenta,
          monto: rec.monto,
          tipPago: rec.tipoPago,
          esDefault: 'S',
          tipEnvio: rec.tipoEnvio,
          numTelefono: rec.tipoEnvio === '1' ? rec.datoEnvio : '',
          nomCorreo: rec.tipoEnvio === '2' ? rec.datoEnvio : '',
          tipLink: rec.tipoLink,
          diaMes: rec.diaMes,
          nomProducto: rec.tipoCuenta === 'PR' ? 'PRESTAMO' : 'TARJETA DE CREDITO',
          codCliente: clientData.codCliente
        };

        return this.linkService.emitirLink(link, this.apiImagenBase64);
      }),
      catchError((err) => {
        return of({ success: false, errorMessage: err.message || 'Error en las validaciones de negocio.', data: '' } as ApiResponse<string>);
      })
    ).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          rec.estado = 'Exitoso';
          rec.codSku = res.data; // El API retorna el SKU o link acortado
          rec.urlCorto = res.data; // En backend emitirLink retorna la URL acortada
          this.successCount.set(this.successCount() + 1);
        } else {
          rec.estado = 'Error';
          rec.resultado = res.errorMessage || 'Error al emitir el link de pago.';
          this.errorCount.set(this.errorCount() + 1);
        }

        this.updateProgress(index + 1, list.length);
        this.records.set(list);
        
        // Process next row
        this.procesarFila(index + 1);
      },
      error: (err) => {
        rec.estado = 'Error';
        rec.resultado = err.message || 'Error inesperado.';
        this.errorCount.set(this.errorCount() + 1);
        
        this.updateProgress(index + 1, list.length);
        this.records.set(list);

        // Process next row
        this.procesarFila(index + 1);
      }
    });
  }

  private updateProgress(processed: number, total: number) {
    this.processedCount.set(processed);
    const pct = Math.round((processed / total) * 100);
    this.progressPercentage.set(pct);
  }

  private resetStats() {
    this.processedCount.set(0);
    this.successCount.set(0);
    this.errorCount.set(0);
    this.progressPercentage.set(0);
  }

  clearFile() {
    this.selectedFileName.set(null);
    this.records.set([]);
    this.resetStats();
    this.totalRecords.set(0);
  }

  downloadTemplate() {
    const csvContent = 
      "TIPO_CUENTA,NUM_CUENTA,MONTO,TIPO_PAGO,TIPO_LINK,DIA_MES,TIPO_ENVIO,DATO_ENVIO\n" +
      "TC,1234567890123456,150.00,0,2,,1,50212345678\n" +
      "PR,9876543210987654,500.00,1,1,15,2,cliente@correo.com\n";

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", "plantilla_carga_masiva.csv");
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
