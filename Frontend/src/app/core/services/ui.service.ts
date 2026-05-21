import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UiService {
  readonly title = signal('Sistema de Pagos NeoLink');
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);

  showError(msg: string) { this.error.set(msg); setTimeout(() => this.error.set(null), 5000); }
  showSuccess(msg: string) { this.success.set(msg); setTimeout(() => this.success.set(null), 5000); }
  showModal(msg: string) { this.modalError.set(msg); }
  closeModal() { this.modalError.set(null); }
}
