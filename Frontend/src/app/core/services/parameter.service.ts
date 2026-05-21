import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { environment } from '../../../environments/environment';

export interface SystemParameters {
  freRevAutorizacion: string;
  freRevHrsRepetir: string;
  freGenLink: string;
  freGenHora: string;
  tcTipTransac: string;
  tcSubtipTrans: string;
  numCtaContaQtz: string;
  numCtaContaDol: string;
  codAgencia: string;
  codTipoTc: string;
  codSubtipoTc: string;
  codTipoPr: string;
  codSubtipoPr: string;
  codDepartamento: string;
  codDeptoPr: string;
  desTransaccion: string;
  apiImagenBase64?: string;
  msgRemitente: string;
  msgHeader: string;
  msgFooter: string;
  msgSms: string;
}

@Injectable({ providedIn: 'root' })
export class ParameterService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBase;

  getParameters(): Observable<ApiResponse<SystemParameters>> {
    return this.http.get<ApiResponse<SystemParameters>>(`${this.baseUrl}/parameters`);
  }

  updateParameters(parameters: SystemParameters): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.baseUrl}/parameters`, parameters);
  }
}
