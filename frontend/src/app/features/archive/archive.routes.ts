import { Routes } from "@angular/router";

import { ArchiveStore } from "./store/archive.store";

export const ARCHIVE_ROUTES: Routes = [
  {
    path: "",
    providers: [ArchiveStore],
    loadComponent: () =>
      import("./pages/archive-page.component").then(
        (componentModule) => componentModule.ArchivePageComponent,
      ),
  },
];
