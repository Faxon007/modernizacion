import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { Campaign } from '../models/campaign';
import { DatePipe } from '@angular/common';

@Component({
    selector: 'app-campaign-card',
    standalone: true,
    imports: [DatePipe],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <article class="bg-white rounded-xl border border-gray-100 shadow-sm hover:shadow-md transition-shadow p-5 flex flex-col h-full border-t-4 border-t-green-800">
      <div class="flex justify-between items-start mb-4">
        <div>
          <span class="text-xs font-bold text-gray-400 tracking-wider">{{ data().campId }}</span>
          <h3 class="text-lg font-semibold text-gray-800 leading-tight mt-1">{{ data().campDesc }}</h3>
        </div>
        
        <span [class]="badgeClasses()">
          {{ data().statusInd === 'A' ? 'Activa' : 'Inactiva' }}
        </span>
      </div>

      <div class="mt-auto pt-4 border-t border-gray-50 text-sm text-gray-500 flex justify-between mb-4">
        <span>Por: <strong class="text-gray-700">{{ data().createdBy }}</strong></span>
        <span>{{ data().createdAt | date:'dd MMM yyyy' }}</span>
      </div>

      <div class="mt-auto flex gap-2">
        <button 
          (click)="edit.emit(data().campId)"
          class="flex-1 py-2 bg-gray-50 hover:bg-green-50 text-green-800 text-sm font-medium rounded-lg transition-colors border border-gray-200 hover:border-green-200">
          Gestionar
        </button>
        <button 
          (click)="delete.emit(data().campId)"
          class="px-4 py-2 bg-white hover:bg-red-50 text-red-600 text-sm font-medium rounded-lg transition-colors border border-gray-200 hover:border-red-200"
          title="Eliminar Campaña">
          Eliminar
        </button>
      </div>
    </article>
  `
})
export class Card {
    readonly data = input.required<Campaign>();
    readonly edit = output<string>();
    // NUEVO: Declaramos la función output para emitir la acción de eliminar
    readonly delete = output<string>();

    readonly badgeClasses = computed(() => {
        const base = 'px-2.5 py-0.5 rounded-full text-xs font-medium';
        return this.data().statusInd === 'A'
            ? `${base} bg-green-100 text-green-800`
            : `${base} bg-gray-100 text-gray-600`;
    });
}