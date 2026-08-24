import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { environment } from '../../../environments/environment';

export interface LinkEntity {
  numCuenta: string;
  tipCuenta: string;
  monto: number;
  tipPago: string;
  esDefault: string;
  tipEnvio: string;
  numTelefono: string;
  nomCorreo: string;
  tipLink: string;
  diaMes: string;
  urlLink?: string;
  urlCorto?: string;
  indEstado?: string;
  codSku?: string;
  nomProducto: string;
  codCliente: string;
  usuIngreso?: string;
}

export interface ClientEntity {
  codCliente: string;
  nomCliente: string;
}

export interface PrestamoInfo {
  numCuenta: string;
  moneda: string;
}

export interface LinkCtaInfo {
  codParametro: string;
  diaMes: string;
  proximaFecha: string;
}

export interface CarrierModel {
  codTranspo: string;
  nomTranspo: string;
  nit: string;
  representante: string;
  direccion: string;
  email: string;
  codAciCli: string;
  tipoAcceso: string;
  servidor: string;
  puerto: string;
  usuario: string;
  clave?: string;
  codBin?: string;
}

export interface PagoRequest {
  numCta: string;
  codSku: string;
  codLink: string;
  monPago?: string;
  autVisa: string;
}

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

export interface LinkVerificaItem {
  correlativo: string;
  producto: string;
  codigoVisa: string;
  numAuto: string;
  numMov: string;
  edit: string;
}

export interface DataTableResponse<T> {
  draw: number;
  recordsTotal: number;
  recordsFiltered: number;
  data: T[];
}

@Injectable({ providedIn: 'root' })
export class LinkService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBase;

  // --- Endpoints de Links ---

  getLinks(dtRequest: any): Observable<DataTableResponse<LinkListItem>> {
    return this.http.post<DataTableResponse<LinkListItem>>(`${this.baseUrl}/links/get-links`, dtRequest);
  }

  getLinksVerifica(dtRequest: any): Observable<DataTableResponse<LinkVerificaItem>> {
    return this.http.post<DataTableResponse<LinkVerificaItem>>(`${this.baseUrl}/links/get-links-verifica`, dtRequest);
  }

  emitirLink(link: LinkEntity, imgPublicitaria: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/links/emitir`, { link, imgPublicitaria });
  }

  validarYConsultaLink(sku: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.baseUrl}/links/validar/${sku}`);
  }

  cancelarLink(sku: string, nombre: string, precio: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/links/cancelar`, { sku, nombre, precio });
  }

  acortarLink(url: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/links/acortar`, { url });
  }

  acortarLinkPeriferico(codPeriferico: number, url: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/links/periferico`, { codPeriferico, url });
  }

  acortarLinkMasivo(): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/links/masivo`, {});
  }

  updateEstadoLink(codParametro: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/links/update-estado`, { codParametro });
  }

  buscarCta(numCta: string): Observable<ApiResponse<LinkCtaInfo>> {
    return this.http.get<ApiResponse<LinkCtaInfo>>(`${this.baseUrl}/links/buscar-cta/${numCta}`);
  }

  buscarParametro(codParametro: string): Observable<ApiResponse<LinkCtaInfo>> {
    return this.http.get<ApiResponse<LinkCtaInfo>>(`${this.baseUrl}/links/buscar-parametro/${codParametro}`);
  }

  aplicarPago(pago: PagoRequest): Observable<{ success: boolean; message: string; errorMessage?: string }> {
    return this.http.post<{ success: boolean; message: string; errorMessage?: string }>(`${this.baseUrl}/links/aplicar-pago`, pago);
  }

  // --- Endpoints de Clientes ---

  getClienteCta(numCta: string): Observable<ApiResponse<ClientEntity>> {
    return this.http.get<ApiResponse<ClientEntity>>(`${this.baseUrl}/clients/${numCta}`);
  }

  getTipoPrestamo(numCta: string): Observable<ApiResponse<PrestamoInfo>> {
    return this.http.get<ApiResponse<PrestamoInfo>>(`${this.baseUrl}/clients/${numCta}/prestamo`);
  }

  isClienteListaNegra(codEmpresa: string, codCliente: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.baseUrl}/clients/blacklist/${codEmpresa}/${codCliente}`);
  }

  getCorreoCliente(codCliente: string): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.baseUrl}/clients/${codCliente}/correo`);
  }

  getTelefonoCliente(codCliente: string): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.baseUrl}/clients/${codCliente}/telefono`);
  }

  getCuentasCliente(codCliente: string): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.baseUrl}/clients/${codCliente}/cuentas`);
  }

  getMontoPR(numCuenta: string): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(`${this.baseUrl}/clients/monto-pr/${numCuenta}`);
  }

  getMontoTC(numCuenta: string): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(`${this.baseUrl}/clients/monto-tc/${numCuenta}`);
  }

  // --- Endpoints de Transportadoras (Carriers) ---

  getCarriers(): Observable<ApiResponse<CarrierModel[]>> {
    return this.http.get<ApiResponse<CarrierModel[]>>(`${this.baseUrl}/carriers`);
  }

  getCarrier(usuario: string): Observable<ApiResponse<CarrierModel>> {
    return this.http.get<ApiResponse<CarrierModel>>(`${this.baseUrl}/carriers/${usuario}`);
  }

  getCarriersDropdown(codCliAci?: string): Observable<ApiResponse<any[]>> {
    const param = codCliAci ? `?codCliAci=${codCliAci}` : '';
    return this.http.get<ApiResponse<any[]>>(`${this.baseUrl}/carriers/dropdown${param}`);
  }

  createCarrier(carrier: CarrierModel): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/carriers`, carrier);
  }

  updateCarrier(carrier: CarrierModel): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.baseUrl}/carriers`, carrier);
  }

  createCarrierUser(carrier: CarrierModel): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/carriers/user`, carrier);
  }

  updateCarrierUser(carrier: CarrierModel): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.baseUrl}/carriers/user`, carrier);
  }
}
