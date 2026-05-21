import { Component, ChangeDetectionStrategy, inject, input, OnInit, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Api } from '../services/api';
import { Auth } from '../../../core/services/auth';

@Component({
    selector: 'app-campaign-form',
    standalone: true,
    imports: [ReactiveFormsModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <section class="min-h-screen bg-gray-50 p-8">
      <div class="max-w-2xl mx-auto bg-white rounded-xl shadow-sm border border-gray-200 p-8 border-t-4 border-t-green-800">
        
        <header class="mb-6 flex justify-between items-center">
          <h2 class="text-2xl font-bold text-gray-900">
            {{ isEditMode() ? 'Editar Campaña' : 'Nueva Campaña' }}
          </h2>
          <button type="button" (click)="goBack()" class="text-gray-500 hover:text-gray-700 font-medium">
            Volver
          </button>
        </header>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Código de Campaña (ID)</label>
            <input 
              type="text" 
              formControlName="campId"
              [readonly]="isEditMode()"
              class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-green-800 focus:border-green-800 read-only:bg-gray-100 read-only:text-gray-500 transition-colors uppercase">
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
            <input 
              type="text" 
              formControlName="campDesc"
              class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-green-800 focus:border-green-800 transition-colors">
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Estado Inicial</label>
            <select 
              formControlName="statusInd"
              class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-green-800 focus:border-green-800 transition-colors bg-white">
              <option value="A">Activa</option>
              <option value="I">Inactiva</option>
            </select>
          </div>

          @if (errorMsg()) {
            <div class="bg-red-50 text-red-700 p-3 rounded-lg text-sm border border-red-200">
              {{ errorMsg() }}
            </div>
          }

          <div class="pt-4 border-t border-gray-100 flex justify-end">
            <button 
              type="submit" 
              [disabled]="form.invalid || isSaving()"
              class="bg-green-800 text-white font-bold py-2 px-6 rounded-lg hover:bg-green-900 focus:ring-4 focus:ring-green-300 disabled:opacity-50 transition-all">
              {{ isSaving() ? 'Guardando...' : 'Guardar Campaña' }}
            </button>
          </div>

        </form>
      </div>
    </section>
  `
})
export class Form implements OnInit {
    // Magia de Angular 20+: Recibimos el parámetro ':id' de la ruta directamente como un Signal
    readonly id = input<string>();

    private readonly fb = inject(NonNullableFormBuilder);
    private readonly api = inject(Api);
    private readonly router = inject(Router);
    private readonly auth = inject(Auth); // Para sacar el username de quien crea/edita

    readonly isEditMode = signal(false);
    readonly isSaving = signal(false);
    readonly errorMsg = signal<string | null>(null);

    // Formulario Tipado
    readonly form = this.fb.group({
        campId: ['', [Validators.required, Validators.maxLength(10)]],
        campDesc: ['', [Validators.required]],
        statusInd: this.fb.control<'A' | 'I'>('A', Validators.required)
    });

    ngOnInit() {
        // Si la ruta nos pasó un ID, estamos en modo edición
        if (this.id()) {
            this.isEditMode.set(true);
            this.form.controls.campId.disable(); // El ID no se puede cambiar
            this.loadCampaign(this.id()!);
        }
    }

    loadCampaign(id: string) {
        this.api.getById(id).subscribe({
            next: (res) => {
                if (res.success) {
                    this.form.patchValue({
                        campId: res.data.campId,
                        campDesc: res.data.campDesc,
                        statusInd: res.data.statusInd
                    });
                }
            }
        });
    }

    onSubmit() {
        if (this.form.invalid) return;
        this.isSaving.set(true);
        this.errorMsg.set(null);

        const rawValue = this.form.getRawValue();
        const currentUser = 'JOSIAE_AC'; // En un entorno real: this.auth.tokenPayload().username

        if (this.isEditMode()) {
            this.api.update(rawValue.campId, {
                campDesc: rawValue.campDesc,
                statusInd: rawValue.statusInd,
                updatedBy: currentUser
            }).subscribe({
                next: () => this.goBack(),
                error: () => this.handleError()
            });
        } else {
            this.api.create({
                campId: rawValue.campId.toUpperCase(),
                campDesc: rawValue.campDesc,
                statusInd: rawValue.statusInd,
                createdBy: currentUser // Ajusta aquí si el backend pide createdfBy
            }).subscribe({
                next: (res) => {
                    if (res.success) this.goBack();
                    else {
                        this.errorMsg.set(res.errorMessage);
                        this.isSaving.set(false);
                    }
                },
                error: () => this.handleError()
            });
        }
    }

    handleError() {
        this.errorMsg.set('Ocurrió un error en el servidor.');
        this.isSaving.set(false);
    }

    goBack() {
        this.router.navigate(['/campaigns']);
    }
}