import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '', // Ruta base: /campaigns
        loadComponent: () => import('./views/campaigns').then(m => m.Campaigns)
    },
    {
        path: 'new', // Ruta crear: /campaigns/new
        loadComponent: () => import('./components/form').then(m => m.Form)
    },
    {
        path: 'edit/:id', // Ruta editar: /campaigns/edit/CB001
        loadComponent: () => import('./components/form').then(m => m.Form)
    }
];