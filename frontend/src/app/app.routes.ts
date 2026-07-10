import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./features/landing/landing.routes').then(
        (routesModule) => routesModule.LANDING_ROUTES,
      ),
  },
  { path: '**', redirectTo: '' },
];
