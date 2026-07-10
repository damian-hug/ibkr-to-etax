# Repository Guidance

## Frontend Architecture

The Angular application under `frontend/src/app` uses a feature-oriented structure. Keep application infrastructure centralized, keep business workflows inside their owning feature, and move code into `shared` only when it is genuinely reusable.

```text
frontend/src/
├── styles.css
├── styles/
│   ├── theme.css
│   ├── tokens.css
│   └── overrides.css
└── app/
    ├── app.ts
    ├── app.config.ts
    ├── app.routes.ts
    ├── core/
    │   └── services/
    │       └── etax-api.service.ts
    ├── features/
    │   └── <feature-name>/
    │       ├── <feature-name>.routes.ts
    │       ├── pages/
    │       │   └── <page-name>.component.ts
    │       ├── components/
    │       │   └── <component-name>.component.ts
    │       ├── store/
    │       │   └── <feature-name>.store.ts
    │       └── models/
    │           └── <model-name>.ts
    └── shared/
        ├── components/
        ├── pipes/
        └── utils/
```

Create only the directories a feature actually needs. The tree describes ownership and placement; empty placeholder directories are unnecessary.

### Application shell

- `app.ts` is the root component and should remain a small application shell.
- `app.config.ts` contains application-wide providers.
- `app.routes.ts` defines top-level routes and lazy-loads feature routes where appropriate.

### Core services

- Put application-wide infrastructure in `core`.
- Use one root-provided `EtaxApiService` as the frontend's boundary to the backend while the API remains small and cohesive.
- The API service may expose multiple related request methods. Do not create a separate service merely because a method represents a different endpoint.
- Split the service only when a meaningful boundary emerges, such as a different backend, authentication mechanism, browser-only responsibility, or a class that has become difficult to maintain.
- Keep API services focused on transport, request/response mapping, and infrastructure concerns. They must not own feature UI state.
- Provide application-wide infrastructure with `providedIn: 'root'` unless a narrower lifetime is intentional.

### Features

- Each directory under `features` represents a cohesive user-facing capability.
- Feature code may depend on `core` and `shared`; `core` and `shared` must not depend on a feature.
- `pages` contains route-level components that compose the feature.
- `components` contains presentation components used only within that feature.
- `models` contains feature-specific types and view models. Backend-wide transport types may live next to the API service when they are shared by multiple features.
- Keep code inside its feature until there is a demonstrated need to share it.

### Signal stores

- Implement feature stores as NgRx SignalStores with `signalStore` from `@ngrx/signals`.
- Compose stores with NgRx SignalStore features such as `withState`, `withComputed`, and `withMethods` instead of creating hand-written injectable signal-store classes.
- Use `withResource()` from `@angular-architects/ngrx-toolkit` to integrate Angular resources for asynchronous reads. Prefer its resource `value`, `status`, `error`, `isLoading`, `hasValue`, and reload APIs over duplicating the same request state manually.
- Use named resources when a store owns multiple independent asynchronous reads; use an unnamed resource when it owns only one.
- Keep backend access behind `EtaxApiService`. Resource factories in a store may inject the service and use its resource-producing API, while store methods coordinate feature actions and state transitions.
- Treat `withResource()` as an experimental dependency: keep its usage inside feature stores, avoid leaking its implementation into presentational components, and consult the current toolkit documentation before changing or upgrading it.
- Put feature workflow state, derived state, and user actions in the feature's SignalStore. Expose the store's signals and methods to consumers rather than mutable state.
- Provide feature stores at the feature route or page level by default. This gives each feature visit an isolated lifetime and disposes of its state when the feature is left.
- Do not configure `providedIn: 'root'` on a feature SignalStore. Use root provisioning only when state is intentionally shared across unrelated routes and must survive feature navigation.
- Components should invoke store actions and render store state rather than calling the API service directly when an operation affects feature state.

The intended dependency flow is:

```text
page/component -> feature store -> root API service -> backend
```

### Shared code

- `shared/components` contains reusable, primarily presentational UI without feature-specific business rules.
- `shared/pipes` contains reusable display transformations.
- `shared/utils` contains small, stateless helpers.
- Shared code must not depend on feature code or become a dumping ground for code with unclear ownership.

### UI system and global styles

- Use PrimeNG and NGXUI as direct UI dependencies. Import their standalone components or modules in the application components that use them.
- Do not wrap library components solely to rename them, set trivial defaults, or hide their APIs.
- Do not create a shared module whose only purpose is to re-export PrimeNG or NGXUI.
- Put application-wide library providers and configuration in `app.config.ts`.
- Keep `styles.css` as the global style entry point and import the files under `styles` from it.
- Put application design tokens, such as colors, spacing, and typography variables, in `styles/tokens.css`.
- Put global theme composition and theme-level rules in `styles/theme.css`.
- Put narrowly scoped third-party style adjustments in `styles/overrides.css`. Avoid broad overrides that depend on library internals when a supported component API is available.
- Project-specific reusable UI compositions may live in `shared/components`; feature-specific compositions remain inside their owning feature.

### General conventions

- Use standalone Angular components and `ChangeDetectionStrategy.OnPush`.
- Keep route-level orchestration in pages and reusable rendering or interaction in components.
- Prefer explicit feature boundaries over grouping all components, models, or stores globally by technical type.
- Add abstractions when they represent a real shared concept, not in anticipation of possible future reuse.

### Frontend testing

- Do not create or run automated tests for the frontend.
- Validate frontend changes with a production build instead.
