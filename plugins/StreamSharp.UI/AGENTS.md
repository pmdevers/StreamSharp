# StreamSharp Home Streaming Service

A production-ready home streaming service UI built with Vue.js, TypeScript, TailwindCSS 3, and modern tooling.

## Tech Stack

- **Frontend**: Vue 3.5.17 + TypeScript + TailwindCSS 3.4.11
- **Styling**: TailwindCSS 3 with PostCSS + Autoprefixer
- **Testing**: Vitest with Vue Test Utils for component testing
- **Build Tool**: Vite 7 with Vue plugin
- **Package Manager**: Bun
- **Linting**: ESLint 9 + Prettier
- **Type Checking**: TypeScript 5 + vue-tsc
- **Development Tools**: Vue DevTools integration

## Product Focus

- **Experience**: A Netflix and Jellyfin-inspired home streaming interface
- **Primary Surfaces**: Hero spotlight, continue-watching rails, media discovery rows, and live TV entry points
- **Goal**: Present a polished frontend foundation for browsing, resuming, and organizing home media playback

## Project Structure

```
src/                     # Vue application source
├── App.vue             # Global streaming shell, navigation, and layout frame
├── main.ts             # Application entry point
├── assets/             # Static assets and shared theme files
│   ├── base.css        # Base styles and shared streaming UI primitives
│   ├── logo.svg        # Logo asset
│   └── main.css        # Main styles with TailwindCSS imports
├── components/         # Reusable media and layout components
│   ├── __tests__/      # Component tests
│   └── icons/          # Icon components
├── router/             # Route definitions for home, movies, series, and live TV
├── views/              # View-level screens for streaming surfaces
├── env.d.ts            # TypeScript declarations
└── ...                 # Other source files

public/                 # Static assets
├── favicon.ico         # Site favicon
└── ...                 # Other static files
```

## Key Features

### Vue 3 Composition API

The application uses Vue 3's modern Composition API with TypeScript for streaming-focused UI state and view composition:

- `<script setup>` syntax for concise component logic
- TypeScript integration throughout
- Reactive state management with Vue's reactivity system
- Component-based architecture

### Styling System

- **Primary**: TailwindCSS 3.4.11 utility classes
- **PostCSS**: Autoprefixer for cross-browser compatibility
- **Configuration**: `tailwind.config.js` for custom streaming theming

```vue
<!-- Example of TailwindCSS usage in Vue components -->
<template>
  <div
    class="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-100 to-slate-200"
  >
    <div class="text-center">
      <h1 class="text-2xl font-semibold text-slate-800">Welcome to StreamSharp</h1>
    </div>
  </div>
</template>

<script setup lang="ts">
// Component logic here
</script>
```

### Development Commands

```bash
bun run dev          # Start development server
bun run build        # Production build
bun run preview      # Preview production build
bun run type-check   # Type check with vue-tsc
bun run test:unit    # Run tests with Vitest
bun run lint         # Lint with ESLint
bun run format       # Format with Prettier
```

## API Spec Refresh Workflow

Use this workflow when the user asks to `refresh based on api specs`, `refresh from openapi`, `sync stores to api`, or similar.

1. Fetch the local OpenAPI document from `http://localhost:5111/openapi/v1.json` and identify endpoint groups, operations, parameters, and request bodies.
2. If the OpenAPI responses are underspecified, inspect live responses from the local server for the relevant endpoints before editing client code.
3. Keep API integration split by backend surface. In this project that currently means separate Pinia stores for media library, plugins, server operations, health checks, and home-only UI state.
4. Update or create store actions first, then map backend payloads into the UI-facing types used by components.
5. Prefer deriving display-only fields in stores when the backend returns minimal data, rather than pushing raw API shapes directly into components.
6. If the refresh adds new operational features, expose them through routed admin screens rather than mixing them into the browsing views.
7. Update shared navigation and route definitions when new admin or API-backed screens are added.
8. After the first substantive edit, run `bun run type-check` before widening the change.
9. Follow typecheck with editor diagnostics for the touched files.
10. Record any confirmed backend response quirks or endpoint-contract gaps in repository memory so the next API refresh can reuse them.

Current API-aligned store layout:

- `src/stores/mediaLibrary.ts` owns `/api/medialibrary` endpoints and maps library and item payloads into UI-friendly types.
- `src/stores/plugins.ts` owns `/api/plugins` install, list, and uninstall flows.
- `src/stores/server.ts` owns `/api/server/restart` and `/api/server/shutdown`.
- `src/stores/health.ts` owns `/healthz`, `/readyz`, and `/livez`.
- `src/stores/home.ts` should stay limited to home-view UI state such as the currently selected item.

## Adding Features

Add features with the streaming product surface in mind: media rows, detail views, playback actions, continue-watching state, library browsing, and live channel experiences.

### New Components

1. Create component in `src/components/`:

```vue
<!-- src/components/MediaCard.vue -->
<template>
  <div class="rounded-lg bg-white p-4 shadow">
    <h2 class="text-xl font-bold text-gray-900">{{ title }}</h2>
    <p class="text-gray-600">{{ metadata }}</p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title?: string
  metadata?: string
}

withDefaults(defineProps<Props>(), {
  title: 'Featured title',
  metadata: 'Movie · 4K HDR',
})
</script>
```

2. Import in your app:

```vue
<template>
  <MediaCard title="Glass Harbor" metadata="Series · Thriller" />
</template>

<script setup lang="ts">
import MediaCard from '@/components/MediaCard.vue'
</script>
```

### Custom TailwindCSS Configuration

1. Update `tailwind.config.js` for custom theming:

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#3b82f6',
        secondary: '#64748b',
      },
    },
  },
  plugins: [],
}
```

2. Add custom styles in `src/assets/main.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .btn-primary {
    @apply rounded bg-blue-500 px-4 py-2 font-bold text-white hover:bg-blue-700;
  }
}
```

## Testing

The project includes comprehensive testing setup:

- **Unit Testing**: Vitest for fast unit tests
- **Component Testing**: Vue Test Utils for Vue component testing
- **Type Checking**: vue-tsc for TypeScript validation

Prioritize coverage for streaming-specific behavior such as media card rendering, route-level discovery views, and continue-watching state presentation.

```bash
bun run test:unit    # Run unit tests
```

## Production Deployment

- **Development**: `bun run dev` for local development
- **Build**: `bun run build` creates optimized production build
- **Preview**: `bun run preview` to preview production build
- **Type Check**: `bun run type-check` for TypeScript validation

## Architecture Notes

- Vue 3.5.17 with Composition API for modern reactive components
- TypeScript throughout the application
- TailwindCSS 3.4.11 for utility-first styling
- Vite 7 for fast development and optimized builds
- Vitest + Vue Test Utils for comprehensive testing
- ESLint + Prettier for code quality
- Vue DevTools integration for development debugging
- Single-page application (SPA) architecture
- Streaming-first information architecture with room for movies, series, live TV, and playback flows
