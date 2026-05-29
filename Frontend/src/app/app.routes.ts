import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth';
import { LayoutComponent } from './feature/shared/components/layout/layout.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./feature/auth/views/auth').then(m => m.Auth) },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { 
        path: 'home', 
        loadComponent: () => import('./feature/home/home.component').then(m => m.HomeComponent) 
      },
      {
        path: 'frmEmisionLink',
        title: 'Emisión de Links | Banco Promerica',
        loadComponent: () => import('./feature/emision-link/emision-link.component').then(m => m.EmisionLinkComponent)
      },
      {
        path: 'frmActivacion',
        title: 'Activación de Links | Banco Promerica',
        loadComponent: () => import('./feature/activacion/activacion.component').then(m => m.ActivacionComponent)
      },
      {
        path: 'frmCancelarLink',
        title: 'Cancelar Programación | Banco Promerica',
        loadComponent: () => import('./feature/cancelar-link/cancelar-link.component').then(m => m.CancelarLinkComponent)
      },
      {
        path: 'frmCargaMasiva',
        title: 'Carga Masiva | Banco Promerica',
        loadComponent: () => import('./feature/carga-masiva/carga-masiva.component').then(m => m.CargaMasivaComponent)
      },
      {
        path: 'frmControlLink',
        title: 'Control de Links | Banco Promerica',
        loadComponent: () => import('./feature/control-link/control-link.component').then(m => m.ControlLinkComponent)
      },
      {
        path: 'frmVerificacionLink',
        title: 'Verificación de Links | Banco Promerica',
        loadComponent: () => import('./feature/verificacion-link/verificacion-link.component').then(m => m.VerificacionLinkComponent)
      },
      {
        path: 'frmParametros',
        title: 'Parámetros del Sistema | Banco Promerica',
        loadComponent: () => import('./feature/parametros/parametros.component').then(m => m.ParametrosComponent)
      },
      { path: '', redirectTo: 'home', pathMatch: 'full' }
    ]
  },
  {
    path: 'campaigns',
    title: 'Campañas | Banco Promerica',
    canActivate: [authGuard],
    loadChildren: () => import('./feature/campaings/routes').then(m => m.routes)
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: ':pageId',
        loadComponent: () => import('./feature/dynamic-page/dynamic-page.component').then(m => m.DynamicPageComponent)
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    // Comodín para rutas no encontradas
    path: '**',
    redirectTo: 'login'
  }
];