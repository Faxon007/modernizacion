import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UiService {
  readonly title = signal('Sistema de Pagos NeoLink');
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly info = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);
  private modalClosedSubject = new Subject<void>();

  showError(msg: string) { this.error.set(msg); setTimeout(() => this.error.set(null), 5000); }
  showSuccess(msg: string) { this.success.set(msg); setTimeout(() => this.success.set(null), 5000); }
  showInfo(msg: string) { this.info.set(msg); setTimeout(() => this.info.set(null), 5000); }

  showModal(msg: string): Observable<void> {
    this.modalError.set(msg);
    this.modalClosedSubject = new Subject<void>(); // Creamos un nuevo subject para esta instancia del modal
    return this.modalClosedSubject.asObservable();
  }

  closeModal() {
    this.modalError.set(null);
    this.modalClosedSubject.next(); // Notificamos que se cerró
    this.modalClosedSubject.complete(); // Completamos el observable
  }
}
