import { Component } from '@angular/core';

@Component({
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center min-h-[50vh] text-gray-400">
      <img src="assets/images/logoN.png" alt="Promerica" class="h-16 opacity-20 mb-4">
      <p class="text-xl">Bienvenido al Sistema de Pagos NeoLink</p>
      <p class="text-sm">Seleccione una opción del menú superior para comenzar.</p>
    </div>
  `
})
export class HomeComponent {}
