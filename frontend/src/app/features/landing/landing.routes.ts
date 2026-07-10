import { Routes } from '@angular/router';

export const LANDING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/landing-page.component').then(
        (componentModule) => componentModule.LandingPageComponent,
      ),
  },
];
