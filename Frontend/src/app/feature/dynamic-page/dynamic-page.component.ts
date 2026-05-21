import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  standalone: true,
  template: `
    <div class="bg-white p-6 rounded-lg shadow-md">
      <h2 class="text-2xl font-bold text-[#007139] border-b pb-2 mb-4">
        {{ currentPage() }}
      </h2>
      <p class="text-gray-600">Contenido dinámico para la página proveniente de la base de datos.</p>
    </div>
  `
})
export class DynamicPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  readonly currentPage = signal('');

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.currentPage.set(params['pageId']);
    });
  }
}
