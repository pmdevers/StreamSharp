# StreamSharp.UI Development Guide

This guide covers developing and maintaining the Vue 3 + TypeScript frontend for StreamSharp.

## Getting Started

### Prerequisites

- **Node.js**: 18+ (or use Bun which handles its own runtime)
- **Bun**: 1.0+ (modern, faster package manager) — [Install Bun](https://bun.sh)
- **Text Editor/IDE**: VS Code recommended with Volar extension for Vue

### Initial Setup

```powershell
# Navigate to UI plugin
cd plugins\StreamSharp.UI

# Install dependencies (using Bun)
bun install

# Start development server
bun run dev
```

The app will be available at `http://localhost:5173` with hot module replacement (HMR).

## Development Workflow

### Day-to-Day Development

```bash
# Terminal 1: Start dev server
bun run dev

# Terminal 2: Type checking (optional but recommended)
bun run type-check --watch

# Terminal 3: Run tests in watch mode (optional)
bun run test:unit -- --watch
```

**Making Changes**:
1. Edit `.vue`, `.ts`, or `.css` files in `src/`
2. Changes hot-reload in browser instantly
3. Fix any type errors shown in IDE or terminal
4. Commit when feature is complete

### Before Committing

```bash
# Type check entire project
bun run type-check

# Lint and fix code style issues
bun run lint

# Run tests
bun run test:unit

# Build production bundle (same as `dotnet build` will do)
bun run build
```

**Commit only when**:
- ✅ `bun run type-check` passes
- ✅ `bun run test:unit` passes (if you added tests)
- ✅ Code follows ESLint rules
- ✅ New components are tested

## Project Structure Deep Dive

### `src/main.ts` — Entry Point

```typescript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())      // State management
app.use(router)              // Client-side routing
app.mount('#app')            // Mount to <div id="app">
```

### `src/App.vue` — Root Component

The global application shell:

```vue
<template>
  <div class="app">
    <!-- Navigation header -->
    <Header />

    <!-- Page content (route-driven) -->
    <main class="main">
      <RouterView />
    </main>

    <!-- Footer -->
    <Footer />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useHealthStore } from '@/stores/health'
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'

const healthStore = useHealthStore()

// Monitor server health on app load
onMounted(async () => {
  await healthStore.checkHealth()
})
</script>
```

### `src/router/index.ts` — Route Definitions

Define all application routes:

```typescript
import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'
import LibraryView from '@/views/LibraryView.vue'
import DetailsView from '@/views/DetailsView.vue'
import AdminView from '@/views/AdminView.vue'

const routes = [
  { path: '/', name: 'Home', component: HomeView },
  { path: '/library', name: 'Library', component: LibraryView },
  { path: '/details/:id', name: 'Details', component: DetailsView },
  { path: '/admin', name: 'Admin', component: AdminView },
  { path: '/:pathMatch(.*)*', name: 'NotFound', component: NotFoundView },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

export default router
```

### `src/stores/` — State Management (Pinia)

Each store manages a specific domain:

```typescript
// src/stores/mediaLibrary.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/api/client'

export const useMediaLibraryStore = defineStore('mediaLibrary', () => {
  // State
  const libraries = ref<Library[]>([])
  const selectedLibrary = ref<Library | null>(null)
  const items = ref<LibraryItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Computed (derived state)
  const itemsByLibrary = computed(() => 
    items.value.filter(i => i.libraryId === selectedLibrary.value?.id)
  )

  // Actions (async operations)
  async function fetchLibraries() {
    loading.value = true
    try {
      libraries.value = await api.get('/api/medialibrary')
      error.value = null
    } catch (e) {
      error.value = 'Failed to fetch libraries'
    } finally {
      loading.value = false
    }
  }

  async function selectLibrary(libraryId: string) {
    selectedLibrary.value = libraries.value.find(l => l.id === libraryId) || null
    if (selectedLibrary.value) {
      items.value = await api.get(`/api/medialibrary/${libraryId}/items`)
    }
  }

  return {
    // State
    libraries,
    selectedLibrary,
    items,
    loading,
    error,

    // Computed
    itemsByLibrary,

    // Actions
    fetchLibraries,
    selectLibrary,
  }
})
```

**Using in a component**:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'

const store = useMediaLibraryStore()

onMounted(async () => {
  await store.fetchLibraries()
})
</script>

<template>
  <div>
    <div v-if="store.loading">Loading libraries...</div>
    <div v-else-if="store.error" class="text-red-600">{{ store.error }}</div>
    <ul v-else>
      <li v-for="lib in store.libraries" :key="lib.id">
        {{ lib.name }}
      </li>
    </ul>
  </div>
</template>
```

### `src/api/client.ts` — API Communication

Centralized HTTP client:

```typescript
const baseURL = 'http://localhost:5111' // Or use relative paths /api

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
): Promise<T> {
  const response = await fetch(`${baseURL}${path}`, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: body ? JSON.stringify(body) : undefined,
  })

  if (!response.ok) {
    throw new Error(`API Error: ${response.status} ${response.statusText}`)
  }

  return response.json()
}

export const api = {
  get<T>(path: string) {
    return request<T>('GET', path)
  },

  post<T>(path: string, body: unknown) {
    return request<T>('POST', path, body)
  },

  put<T>(path: string, body: unknown) {
    return request<T>('PUT', path, body)
  },

  delete<T>(path: string) {
    return request<T>('DELETE', path)
  },
}
```

## Adding Features

### Feature: New Media Card Component

**Step 1**: Create component (`src/components/MediaCard.vue`):

```vue
<template>
  <div
    class="group relative rounded-lg overflow-hidden bg-gradient-to-br from-slate-700 to-slate-900 hover:shadow-lg transition-shadow cursor-pointer"
  >
    <!-- Image -->
    <img
      :src="imageUrl"
      :alt="title"
      class="w-full h-64 object-cover group-hover:scale-105 transition-transform"
    />

    <!-- Overlay -->
    <div
      class="absolute inset-0 bg-gradient-to-t from-black to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex flex-col justify-end p-4"
    >
      <h3 class="text-white font-bold truncate">{{ title }}</h3>
      <p class="text-gray-300 text-sm">{{ year }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  id: string
  title: string
  imageUrl: string
  year?: number
}

defineProps<Props>()

const emit = defineEmits<{
  click: [id: string]
}>()
</script>
```

**Step 2**: Test it (`src/components/__tests__/MediaCard.spec.ts`):

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MediaCard from '../MediaCard.vue'

describe('MediaCard', () => {
  it('renders title and year', () => {
    const wrapper = mount(MediaCard, {
      props: {
        id: '1',
        title: 'Inception',
        imageUrl: '/inception.jpg',
        year: 2010,
      },
    })

    expect(wrapper.text()).toContain('Inception')
    expect(wrapper.text()).toContain('2010')
  })

  it('emits click event', async () => {
    const wrapper = mount(MediaCard, {
      props: {
        id: '1',
        title: 'Inception',
        imageUrl: '/inception.jpg',
      },
    })

    await wrapper.trigger('click')
    expect(wrapper.emitted('click')).toBeTruthy()
  })
})
```

Run: `bun run test:unit`

**Step 3**: Use in LibraryView:

```vue
<!-- src/views/LibraryView.vue -->
<template>
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
    <MediaCard
      v-for="item in store.items"
      :key="item.id"
      :id="item.id"
      :title="item.name"
      :imageUrl="item.posterUrl"
      :year="item.releaseYear"
      @click="goToDetails"
    />
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'
import MediaCard from '@/components/MediaCard.vue'

const router = useRouter()
const store = useMediaLibraryStore()

function goToDetails(id: string) {
  router.push(`/details/${id}`)
}
</script>
```

### Feature: New Admin Panel Store

**Step 1**: Create store (`src/stores/admin.ts`):

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface AdminSettings {
  autoUpdateEnabled: boolean
  maxConcurrentStreams: number
  enableDebugLogging: boolean
}

export const useAdminStore = defineStore('admin', () => {
  const settings = ref<AdminSettings | null>(null)
  const loading = ref(false)

  async function fetchSettings() {
    loading.value = true
    try {
      settings.value = await api.get('/api/admin/settings')
    } finally {
      loading.value = false
    }
  }

  async function updateSettings(newSettings: Partial<AdminSettings>) {
    if (!settings.value) return

    const updated = { ...settings.value, ...newSettings }
    await api.put('/api/admin/settings', updated)
    settings.value = updated
  }

  return {
    settings,
    loading,
    fetchSettings,
    updateSettings,
  }
})
```

**Step 2**: Add to AdminView:

```vue
<template>
  <div class="admin-panel">
    <h1 class="text-3xl font-bold mb-6">Admin Settings</h1>

    <div v-if="store.loading">Loading settings...</div>
    <form v-else @submit.prevent="save">
      <label class="block mb-4">
        <input v-model="settings.autoUpdateEnabled" type="checkbox" />
        Auto Update Enabled
      </label>

      <label class="block mb-4">
        Max Concurrent Streams:
        <input v-model.number="settings.maxConcurrentStreams" type="number" />
      </label>

      <button type="submit" class="bg-blue-600 text-white px-4 py-2 rounded">
        Save Settings
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAdminStore } from '@/stores/admin'

const store = useAdminStore()
const settings = ref({ ...store.settings })

onMounted(() => {
  store.fetchSettings()
})

async function save() {
  await store.updateSettings(settings.value)
}
</script>
```

## Styling Best Practices

### Use Tailwind Utility Classes

```vue
<!-- ✅ Good: Utility classes -->
<div class="flex items-center justify-between p-4 bg-white rounded-lg shadow">
  <h2 class="text-xl font-bold text-gray-900">Title</h2>
  <button class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
    Action
  </button>
</div>

<!-- ❌ Avoid: Custom CSS unless necessary -->
<div class="card">
  <style>
    .card { /* ... */ }
  </style>
</div>
```

### Responsive Design

```vue
<template>
  <!-- Mobile: 1 column, Tablet: 2, Desktop: 4 -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
    <div v-for="item in items" :key="item.id" class="rounded-lg p-4 bg-white">
      {{ item.name }}
    </div>
  </div>
</template>
```

### Dark Mode

```vue
<template>
  <!-- Auto adapts to system dark mode preference -->
  <div class="bg-white dark:bg-slate-900 text-slate-900 dark:text-white">
    Content adapts to light/dark mode
  </div>
</template>
```

Configure in `tailwind.config.js`:

```javascript
export default {
  darkMode: 'class', // or 'media' for system preference
  // ...
}
```

## TypeScript Best Practices

### Define Types Early

```typescript
// src/data/types.ts
export interface Library {
  id: string
  name: string
  path: string
  createdAt: Date
}

export interface LibraryItem {
  id: string
  libraryId: string
  name: string
  path: string
  type: 'movie' | 'series' | 'episode'
}

// Use in stores and components
import type { Library, LibraryItem } from '@/data/types'
```

### Use `defineProps` and `defineEmits`

```vue
<script setup lang="ts">
import type { PropType } from 'vue'

interface Item {
  id: string
  name: string
}

// Props with types
const props = defineProps<{
  items: Item[]
  selected?: Item
  maxItems?: number
}>()

// Emits with types
const emit = defineEmits<{
  'item-selected': [item: Item]
  'delete': [id: string]
}>()

function selectItem(item: Item) {
  emit('item-selected', item)
}
</script>
```

## Testing Strategy

### Unit Test Components

```typescript
// Component tests verify behavior in isolation
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MyComponent from '../MyComponent.vue'

describe('MyComponent', () => {
  it('renders slots correctly', () => {
    const wrapper = mount(MyComponent, {
      slots: {
        default: 'Slot content',
      },
    })
    expect(wrapper.text()).toContain('Slot content')
  })

  it('emits events', async () => {
    const wrapper = mount(MyComponent)
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('submit')).toBeTruthy()
  })
})
```

### Mock API Calls

```typescript
import { describe, it, expect, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'

vi.mock('@/api/client', () => ({
  api: {
    get: vi.fn().mockResolvedValue([
      { id: '1', name: 'Library 1' },
      { id: '2', name: 'Library 2' },
    ]),
  },
}))

describe('MediaLibraryStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('fetches libraries', async () => {
    const store = useMediaLibraryStore()
    await store.fetchLibraries()
    expect(store.libraries).toHaveLength(2)
  })
})
```

## Performance Tips

### Lazy Load Routes

Routes are code-split automatically:

```typescript
// router/index.ts - Routes are automatically lazy-loaded
const routes = [
  { path: '/', component: () => import('@/views/HomeView.vue') },
  { path: '/admin', component: () => import('@/views/AdminView.vue') },
]
```

### Use `computed` for Derived State

```vue
<script setup lang="ts">
import { computed } from 'vue'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'

const store = useMediaLibraryStore()

// ✅ Computed properties are cached and only re-compute when dependencies change
const filteredItems = computed(() =>
  store.items.filter(i => i.rating > 7.5)
)

// ❌ Avoid: Methods re-run on every render
const badFilter = () => store.items.filter(i => i.rating > 7.5)
</script>
```

### Optimize Images

```vue
<template>
  <!-- Vite optimizes images automatically -->
  <img src="@/assets/logo.svg" alt="Logo" class="w-32 h-auto" />

  <!-- Use responsive images for different screen sizes -->
  <picture>
    <source srcset="@/assets/hero-lg.jpg" media="(min-width: 1024px)" />
    <source srcset="@/assets/hero-md.jpg" media="(min-width: 768px)" />
    <img src="@/assets/hero-sm.jpg" alt="Hero" class="w-full" />
  </picture>
</template>
```

## Debugging

### Vue DevTools

1. Install [Vue DevTools](https://devtools.vuejs.org/) browser extension
2. Open DevTools → Vue tab
3. Inspect components, watch state changes
4. Time-travel debug with Pinia history

### Console Debugging

```typescript
// Log store state
const store = useMediaLibraryStore()
console.log('Store state:', store.$state)

// Watch for state changes
store.$subscribe((mutation, state) => {
  console.log('State changed:', mutation, state)
})

// Debug computed values
import { watchEffect } from 'vue'
watchEffect(() => {
  console.log('Filtered items:', store.itemsByLibrary)
})
```

### TypeScript Errors

```bash
# Type check the entire project
bun run type-check

# Show errors in editor (VS Code with Volar extension)
# ESLint will also catch type issues
```

## Common Issues & Solutions

### HMR Not Working

**Symptom**: Changes don't reflect in browser

**Solution**:
1. Check `vite.config.ts` HMR config
2. Ensure dev server is running (`bun run dev`)
3. Browser might have cache — do hard refresh (Ctrl+Shift+R)
4. Restart dev server

### Build Fails on `dotnet build`

**Symptom**: `dotnet build` fails with Bun/Vite errors

**Solution**:
```bash
# From plugins/StreamSharp.UI/
bun install           # Reinstall deps
bun run type-check    # Check for TS errors
bun run build         # Test build locally

# Then try from solution root
cd ../..
dotnet build
```

### Store State Not Updating

**Symptom**: Store state changes but component doesn't re-render

**Solution**:
1. Ensure you're using store properly: `const store = useStore()`
2. Modify state through store actions, not directly
3. Use Pinia DevTools to verify state changes
4. Check for async/await issues in actions

## Further Reading

- [Vue 3 Composition API](https://vuejs.org/guide/extras/composition-api-faq.html)
- [Pinia State Management](https://pinia.vuejs.org/core-concepts/)
- [Vite Configuration](https://vitejs.dev/config/)
- [TailwindCSS Utilities](https://tailwindcss.com/docs)
- [Vitest Testing](https://vitest.dev/guide/)
- [Existing AGENTS.md](../AGENTS.md) — API refresh workflow and development guidelines

## Quick Reference

```bash
# Development
bun run dev                    # Start dev server
bun run type-check --watch    # Watch for type errors
bun run test:unit -- --watch  # Watch for test failures

# Before committing
bun run type-check             # Full type check
bun run lint                   # Lint and fix code
bun run test:unit              # Run all tests
bun run build                  # Production build (catches build errors)

# Project structure
src/main.ts                    # Entry point
src/App.vue                    # Root component
src/router/                    # Routes
src/views/                     # Page components
src/components/                # Reusable components
src/stores/                    # Pinia state
src/api/client.ts              # HTTP client
src/data/types.ts              # TypeScript interfaces
```
