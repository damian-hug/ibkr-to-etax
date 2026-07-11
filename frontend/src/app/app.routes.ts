import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'archive',
    loadChildren: () =>
      import('./features/archive/archive.routes').then(
        (routesModule) => routesModule.ARCHIVE_ROUTES,
      ),
  },
  {
    path: '',
    loadChildren: () =>
      import('./features/landing/landing.routes').then(
        (routesModule) => routesModule.LANDING_ROUTES,
      ),
  },
  { path: '**', redirectTo: '' },
];
