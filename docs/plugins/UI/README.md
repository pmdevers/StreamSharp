# StreamSharp.UI Plugin

A production-ready, streaming-focused Vue 3 + TypeScript + TailwindCSS user interface embedded in the StreamSharp server. The UI provides a Netflix/Jellyfin-inspired experience for browsing, organizing, and discovering home media.

## Overview

The StreamSharp UI plugin is a **Vue 3 single-page application (SPA)** that is built and embedded into the main server assembly. Unlike traditional plugins loaded at runtime, the UI is compiled as a static asset during the build process and served directly by the ASP.NET Core server.

### Key Responsibilities

- **Media Browsing** — Browse libraries, items, and collections
- **State Management** — Pinia stores for media library, plugins, server operations
- **API Integration** — Communicate with backend `/api/*` endpoints
- **Responsive Design** — TailwindCSS-based responsive UI
- **Developer Experience** — Hot module replacement (HMR), type safety, dev tooling

## Tech Stack

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | Vue 3 | 3.5.17 | Reactive UI framework |
| **Language** | TypeScript | 5.7+ | Type-safe development |
| **Styling** | TailwindCSS | 3.4.11 | Utility-first CSS |
| **Build Tool** | Vite | 7.0+ | Ultra-fast bundler & dev server |
| **Package Manager** | Bun | 1.0+ | Fast, modern package manager |
| **State Management** | Pinia | Latest | Lightweight store library |
| **Testing** | Vitest | Latest | Unit testing framework |
| **Linting** | ESLint 9 + Prettier | Latest | Code quality & formatting |
| **Router** | Vue Router | 4+ | Client-side routing |

## Project Structure

```
plugins/StreamSharp.UI/
├── src/
│   ├── main.ts                  # Application entry point
│   ├── App.vue                  # Root component (layout, nav, router-view)
│   ├── style.css                # Global styles
│   │
│   ├── api/                     # API client & request utilities
│   │   └── client.ts            # HTTP client configuration
│   │
│   ├── assets/                  # Static assets
│   │   ├── base.css             # Base styles & primitives
│   │   ├── logo.svg             # Logo asset
│   │   └── main.css             # Main stylesheet (TailwindCSS imports)
│   │
│   ├── components/              # Reusable Vue components
│   │   ├── __tests__/           # Component unit tests
│   │   ├── icons/               # Icon components
│   │   ├── MediaCard.vue        # Media item card
│   │   ├── MediaRow.vue         # Horizontal media row
│   │   ├── Header.vue           # Navigation header
│   │   └── ...                  # Other reusable components
│   │
│   ├── router/                  # Vue Router configuration
│   │   ├── index.ts             # Route definitions
│   │   └── constants.ts         # Route constants
│   │
│   ├── views/                   # Page-level components (one per route)
│   │   ├── HomeView.vue         # Home/landing screen
│   │   ├── LibraryView.vue      # Media library browser
│   │   ├── DetailsView.vue      # Item details & actions
│   │   ├── AdminView.vue        # Plugin & server admin
│   │   └── ...                  # Other view pages
│   │
│   ├── stores/                  # Pinia state management
│   │   ├── mediaLibrary.ts      # /api/medialibrary store
│   │   ├── plugins.ts           # /api/plugins store
│   │   ├── server.ts            # /api/server store
│   │   ├── health.ts            # Health check store
│   │   ├── home.ts              # Home view local state
│   │   └── ...                  # Other stores
│   │
│   ├── data/                    # Static data, constants, enums
│   │   ├── types.ts             # TypeScript interfaces
│   │   └── constants.ts         # Global constants
│   │
│   └── env.d.ts                 # TypeScript type declarations
│
├── public/                      # Static assets served as-is
│   ├── favicon.ico              # Browser tab icon
│   ├── favicon.svg              # Modern SVG favicon
│   └── icons.svg                # Icon sprite
│
├── dist/                        # Production build output (generated)
│   └── assets/                  # Bundled JS, CSS, images
│
├── package.json                 # Dependencies and build scripts
├── vite.config.ts               # Vite configuration
├── tsconfig.json                # TypeScript configuration
├── tailwind.config.js           # TailwindCSS theming
├── eslint.config.js             # ESLint configuration
├── vitest.config.ts             # Vitest testing configuration
├── AGENTS.md                    # Development guidelines (existing)
└── README.md                    # This file
```

## Build & Development Workflow

### Development

```bash
cd plugins/StreamSharp.UI

# Install dependencies
bun install

# Start development server (with HMR)
bun run dev
```

The dev server runs on `http://localhost:5173` with hot module replacement enabled.

### Building

The UI is built as part of the main solution build:

```bash
# From solution root
dotnet build                    # Triggers: bun run build in StreamSharp.UI/
```

**Build Process**:
1. `package.json` build script calls: `bun run type-check && bun run build-only`
2. Type checking via `vue-tsc --build`
3. Vite bundling: `vite build`
4. Output: `dist/assets/*` (JS, CSS, images)
5. MSBuild copies `dist/` to server's static files

### Commands

```bash
bun run dev            # Dev server with HMR (localhost:5173)
bun run build          # Type-check + production build
bun run preview        # Preview production build locally
bun run type-check     # TypeScript type checking (vue-tsc)
bun run test:unit      # Run Vitest unit tests
bun run lint           # Lint and fix with ESLint
bun run format         # Format with Prettier
```

## Architecture

### Component Hierarchy

```
App.vue (root)
├── Header (navigation, search)
├── RouterView (per-route content)
│   ├── HomeView
│   │   ├── HeroSpotlight
│   │   ├── ContinueWatching (MediaRow)
│   │   └── DiscoveryRows (MediaRow*)
│   ├── LibraryView
│   │   ├── LibraryFilter
│   │   └── MediaGrid (MediaCard*)
│   ├── DetailsView
│   │   ├── DetailsBanner
│   │   ├── ActionsBar
│   │   ├── DescriptionPanel
│   │   └── RelatedMedia (MediaRow)
│   └── AdminView
│       ├── PluginManager
│       └── ServerControls
└── Footer
```

### State Management (Pinia)

Each **API surface** gets its own store:

#### `stores/mediaLibrary.ts`
- **Owns**: `/api/medialibrary/*` endpoints
- **State**: Libraries, items, search results
- **Actions**: `getLibraries()`, `searchItems()`, `getItemDetails()`
- **Usage**: HomeView, LibraryView, DetailsView

#### `stores/plugins.ts`
- **Owns**: `/api/plugins/*` endpoints
- **State**: Installed plugins, available plugins
- **Actions**: `listPlugins()`, `installPlugin()`, `uninstallPlugin()`
- **Usage**: AdminView

#### `stores/server.ts`
- **Owns**: `/api/server/*` endpoints
- **State**: Server configuration, operations
- **Actions**: `restartServer()`, `shutdownServer()`
- **Usage**: AdminView

#### `stores/health.ts`
- **Owns**: `/healthz`, `/readyz`, `/livez` endpoints
- **State**: Health status, readiness, liveness
- **Actions**: `checkHealth()`, `checkReadiness()`
- **Usage**: App.vue (health monitoring)

#### `stores/home.ts`
- **Owns**: Local UI state only (NO API calls)
- **State**: Selected item, filter state, current view
- **Usage**: HomeView local interactions

### API Communication

All HTTP requests go through a centralized client in `api/client.ts`:

```typescript
// api/client.ts
export const api = {
  get<T>(path: string): Promise<T>,
  post<T>(path: string, data: any): Promise<T>,
  put<T>(path: string, data: any): Promise<T>,
  delete<T>(path: string): Promise<T>,
};

// Store usage
const libraryStore = useMediaLibraryStore();
async function loadLibraries() {
  const data = await api.get('/api/medialibrary');
  libraryStore.setLibraries(data);
}
```

### Routing

Routes are defined in `router/index.ts`:

```typescript
const routes = [
  { path: '/', name: 'Home', component: HomeView },
  { path: '/library', name: 'Library', component: LibraryView },
  { path: '/details/:id', name: 'Details', component: DetailsView },
  { path: '/admin', name: 'Admin', component: AdminView },
];
```

**Navigation**:
```vue
<RouterLink to="/library">Browse Library</RouterLink>
<router-push('/details/' + itemId)>
```

## Development Guide

### Adding a New Component

1. **Create component file** (`src/components/MyComponent.vue`):

```vue
<template>
  <div class="my-component">
    <h2 class="text-xl font-bold">{{ title }}</h2>
    <p class="text-gray-600">{{ description }}</p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  description?: string
}

withDefaults(defineProps<Props>(), {
  description: 'No description provided',
})
</script>

<style scoped>
.my-component {
  /* Component-specific styles (if needed) */
}
</style>
```

2. **Use in a view or parent component**:

```vue
<template>
  <MyComponent title="Featured Media" description="Recently added items" />
</template>

<script setup lang="ts">
import MyComponent from '@/components/MyComponent.vue'
</script>
```

3. **Test the component** (`src/components/__tests__/MyComponent.spec.ts`):

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MyComponent from '../MyComponent.vue'

describe('MyComponent', () => {
  it('renders props correctly', () => {
    const wrapper = mount(MyComponent, {
      props: {
        title: 'Test Title',
        description: 'Test description',
      },
    })
    expect(wrapper.text()).toContain('Test Title')
    expect(wrapper.text()).toContain('Test description')
  })
})
```

### Adding a New API Store

1. **Create store file** (`src/stores/myApi.ts`):

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export const useMyApiStore = defineStore('myApi', () => {
  const data = ref<MyDataType[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchData() {
    loading.value = true
    error.value = null
    try {
      data.value = await api.get('/api/myendpoint')
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      loading.value = false
    }
  }

  return {
    data,
    loading,
    error,
    fetchData,
  }
})
```

2. **Use in a component**:

```vue
<script setup lang="ts">
import { useMyApiStore } from '@/stores/myApi'

const store = useMyApiStore()

onMounted(async () => {
  await store.fetchData()
})
</script>

<template>
  <div v-if="store.loading">Loading...</div>
  <div v-else-if="store.error" class="text-red-600">Error: {{ store.error }}</div>
  <div v-else>
    <div v-for="item in store.data" :key="item.id">{{ item.name }}</div>
  </div>
</template>
```

### Adding a New Route/View

1. **Create view** (`src/views/MyNewView.vue`):

```vue
<template>
  <div class="view-container">
    <h1 class="text-3xl font-bold">My New Page</h1>
    <!-- View content -->
  </div>
</template>

<script setup lang="ts">
// View logic
</script>
```

2. **Add route** (in `router/index.ts`):

```typescript
import MyNewView from '@/views/MyNewView.vue'

const routes = [
  // ... existing routes ...
  { path: '/mynewpage', name: 'MyNewPage', component: MyNewView },
]
```

3. **Link to route** (in components):

```vue
<RouterLink to="/mynewpage">Go to My New Page</RouterLink>
```

## Styling

### TailwindCSS

All styling is done with Tailwind utility classes:

```vue
<template>
  <!-- Responsive design -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
    <!-- Dark mode support -->
    <div class="bg-white dark:bg-slate-800 text-slate-900 dark:text-white">
      <!-- Hover states -->
      <button class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded">
        Action
      </button>
    </div>
  </div>
</template>
```

### Custom Tailwind Theme

Customize theme in `tailwind.config.js`:

```javascript
export default {
  theme: {
    extend: {
      colors: {
        streaming: {
          50: '#f5f3ff',
          900: '#1a0033',
        },
      },
      spacing: {
        '128': '32rem',
      },
    },
  },
}
```

## Testing

### Component Tests

```typescript
// src/components/__tests__/MediaCard.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MediaCard from '../MediaCard.vue'

describe('MediaCard', () => {
  it('displays media information', () => {
    const wrapper = mount(MediaCard, {
      props: {
        title: 'Inception',
        year: 2010,
        rating: 8.8,
      },
    })
    expect(wrapper.text()).toContain('Inception')
    expect(wrapper.text()).toContain('2010')
  })
})
```

### Running Tests

```bash
bun run test:unit               # Run all tests
bun run test:unit -- --watch   # Watch mode
```

## Performance Optimization

### Code Splitting

Vite automatically code-splits routes:

```typescript
// router/index.ts - lazy-loaded routes
const routes = [
  { path: '/', component: () => import('@/views/HomeView.vue') },
  { path: '/library', component: () => import('@/views/LibraryView.vue') },
]
```

### Image Optimization

Place images in `src/assets/` for optimization:

```vue
<template>
  <!-- Vite optimizes these -->
  <img src="@/assets/logo.svg" alt="Logo" />
</template>
```

### Bundle Analysis

```bash
# Add to vite.config.ts and run
import { visualizer } from 'rollup-plugin-visualizer'

export default {
  plugins: [visualizer()],
}
```

## Troubleshooting

### Development Server Issues

**Port 5173 already in use**:
```bash
bun run dev -- --port 5174
```

**HMR not working**:
- Check firewall rules
- Verify `vite.config.ts` HMR configuration
- Restart dev server

### Build Issues

**Dependencies not installed**:
```bash
rm -r node_modules
bun install
```

**Build fails during `dotnet build`**:
1. Check for TypeScript errors: `bun run type-check`
2. Ensure Bun is installed and in PATH
3. Clear `dist/` directory and rebuild

### Runtime Issues

**API requests failing**:
1. Verify backend is running on expected port
2. Check CORS configuration in server
3. Inspect browser Network tab

**Store not updating**:
1. Verify store actions are being called
2. Use Vue DevTools Pinia plugin
3. Check for console errors

## Further Reading

- [Vue 3 Documentation](https://vuejs.org/)
- [Pinia State Management](https://pinia.vuejs.org/)
- [Vite Guide](https://vitejs.dev/)
- [TailwindCSS Documentation](https://tailwindcss.com/)
- [Vitest Documentation](https://vitest.dev/)
- [Existing AGENTS.md](./AGENTS.md) — Development workflows and API refresh procedures

## Related Documentation

- [agents.md - Plugin System](../../agents.md#plugin-system)
- [agents.md - Build & Test](../../agents.md#build--test)
- [PostgreSQL Plugin Documentation](../PostgreSQL/README.md) — Backend persistence
- [StreamSharp Architecture](../../agents.md#architecture)
