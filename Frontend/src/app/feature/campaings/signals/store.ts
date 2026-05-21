import { Injectable, signal, computed, inject } from '@angular/core';
import { Api } from '../services/api';
import { Campaign } from '../models/campaign';

@Injectable({ providedIn: 'root' })
export class Store {
    private readonly api = inject(Api);

    // Estado interno
    private readonly _data = signal<Campaign[]>([]);
    private readonly _loading = signal<boolean>(false);
    private readonly _error = signal<string | null>(null);

    // Estado público (Read-only)
    readonly campaigns = this._data.asReadonly();
    readonly isLoading = this._loading.asReadonly();
    readonly error = this._error.asReadonly();

    // Estado derivado (Computados)
    readonly activeCount = computed(() =>
        this._data().filter(c => c.statusInd === 'A').length
    );

    load(): void {
        this._loading.set(true);
        this.api.getAll().subscribe({
            next: (res) => {
                if (res.success) {
                    this._data.set(res.data);
                    this._error.set(null);
                } else {
                    this._error.set(res.errorMessage || 'Error desconocido');
                }
            },
            error: () => {
                this._error.set('Fallo de conexión con el servidor Promerica.');
            },
            complete: () => this._loading.set(false)
        });
    }

    removeCampaign(id: string, username: string): void {
        // Eliminación optimista de la UI
        const previousState = this._data();
        this._data.update(campaigns => campaigns.filter(c => c.campId !== id));

        this.api.delete(id, username).subscribe({
            error: () => {
                // Si falla, revertimos el estado y mostramos error
                this._data.set(previousState);
                this._error.set('No se pudo eliminar la campaña.');
            }
        });
    }
}