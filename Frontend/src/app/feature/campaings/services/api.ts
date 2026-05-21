import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Campaign } from '../models/campaign';
import { ApiResponse } from '../../../core/models/api-response';
import { environment } from '../../../../environments/environment';

export interface CreateCampaignPayload {
    campId: string;
    campDesc: string;
    statusInd: 'A' | 'I';
    createdBy: string; // Nota: En tu Postman dice "createdfBy", ajusta si el backend es estricto con el typo
}

export interface UpdateCampaignPayload {
    campDesc: string;
    statusInd: 'A' | 'I';
    updatedBy: string;
}

@Injectable({ providedIn: 'root' })
export class Api {
    private readonly http = inject(HttpClient);
    private readonly endpoint = `${environment.apiBase}/Campaigns`;

    getAll(): Observable<ApiResponse<Campaign[]>> {
        return this.http.get<ApiResponse<Campaign[]>>(this.endpoint);
    }

    getById(id: string): Observable<ApiResponse<Campaign>> {
        return this.http.get<ApiResponse<Campaign>>(`${this.endpoint}/${id}`);
    }

    create(payload: CreateCampaignPayload): Observable<ApiResponse<any>> {
        return this.http.post<ApiResponse<any>>(this.endpoint, payload);
    }

    update(id: string, payload: UpdateCampaignPayload): Observable<any> {
        // El PATCH en tu Postman devuelve 204 No Content, por lo que no tipamos respuesta
        return this.http.patch(`${this.endpoint}/${id}`, payload);
    }

    delete(id: string, updatedBy: string): Observable<any> {
        // El DELETE requiere query params según tu Postman
        return this.http.delete(`${this.endpoint}/${id}?updatedBy=${updatedBy}&inactiveValue=I`);
    }
}