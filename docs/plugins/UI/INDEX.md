# StreamSharp.UI Documentation Index

A comprehensive guide to the StreamSharp Vue 3 + TypeScript frontend plugin.

## Quick Links

| Document | Purpose |
|---|---|
| [README](README.md) | Plugin overview, architecture, tech stack |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Development workflow, adding features, debugging |
| [INDEX.md](INDEX.md) | This navigation guide |

## For Different Roles

### 👨‍💼 Product Managers / Tech Leads
1. Read [README - Overview](README.md#overview) — Understand what the UI does
2. Skim [README - Tech Stack](README.md#tech-stack) — Know the tools used
3. Check [README - Key Responsibilities](README.md#overview) — Understand scope

**Goal**: Understand the frontend's role in the system

---

### 👨‍💻 Frontend Developers (Adding Features)
1. Start with [DEVELOPMENT.md - Getting Started](DEVELOPMENT.md#getting-started) — Set up dev environment
2. Review [README - Project Structure](README.md#project-structure) — Understand code organization
3. Follow [DEVELOPMENT.md - Feature Workflows](DEVELOPMENT.md#adding-features) — Add your feature
4. Reference [README - Component Example](README.md#adding-a-new-component) — For code patterns

**Focus**: Features, components, state management, styling

---

### 🔧 DevOps / Build Engineers
1. Read [README - Build & Development Workflow](README.md#build--development-workflow) — Understand build process
2. Check [DEVELOPMENT.md - Before Committing](DEVELOPMENT.md#before-committing) — Quality gates
3. Reference [README - Development Commands](README.md#commands) — Available commands

**Focus**: Build integration, deployment, CI/CD

---

### 🧪 QA / Test Engineers
1. Review [README - Architecture - State Management](README.md#state-management-pinia) — Understand how data flows
2. Read [DEVELOPMENT.md - Testing Strategy](DEVELOPMENT.md#testing-strategy) — How to write tests
3. Check [README - Testing](README.md#testing) — Testing practices

**Focus**: Component behavior, API integration, state changes

---

### 📚 Onboarding New Developers
Follow this sequence (~3 hours):
1. **Setup** (30 min): [DEVELOPMENT.md - Initial Setup](DEVELOPMENT.md#initial-setup)
2. **Architecture** (45 min): [README - Architecture](README.md#architecture)
3. **First Feature** (90 min): [DEVELOPMENT.md - Adding a Feature](DEVELOPMENT.md#adding-features)
4. **Deep Dive** (45 min): [README - Project Structure](README.md#project-structure)

---

## Common Tasks

### I need to... 🎯

**...set up development environment**
→ [DEVELOPMENT.md - Getting Started](DEVELOPMENT.md#getting-started)

**...understand the component structure**
→ [README - Component Hierarchy](README.md#component-hierarchy)

**...add a new component**
→ [DEVELOPMENT.md - Feature: New Media Card Component](DEVELOPMENT.md#feature-new-media-card-component)

**...add a new page/route**
→ [README - Adding a New Route/View](README.md#adding-a-new-routeview)

**...work with the state management (Pinia)**
→ [DEVELOPMENT.md - Project Structure Deep Dive - stores/](DEVELOPMENT.md#srcstores--state-management-pinia)

**...make API calls**
→ [README - API Communication](README.md#api-communication)

**...write component tests**
→ [DEVELOPMENT.md - Testing Strategy](DEVELOPMENT.md#testing-strategy)

**...debug state issues**
→ [DEVELOPMENT.md - Debugging](DEVELOPMENT.md#debugging)

**...style a component**
→ [DEVELOPMENT.md - Styling Best Practices](DEVELOPMENT.md#styling-best-practices)

**...optimize performance**
→ [DEVELOPMENT.md - Performance Tips](DEVELOPMENT.md#performance-tips)

## Key Concepts

### Vue 3 Composition API
The `<script setup>` syntax provides concise, type-safe component logic:

```vue
<script setup lang="ts">
const count = ref(0)
const doubled = computed(() => count.value * 2)
</script>
```

### Pinia State Management
Each API endpoint gets its own store for managing data, loading, and errors:

```typescript
const store = useMediaLibraryStore()
await store.fetchLibraries()  // Action
store.libraries                // State
```

### Vite Build Tool
Fast, ES module-based build tool with hot module replacement:
- Dev server: `bun run dev` (localhost:5173)
- Production build: `bun run build`
- Type checking: `bun run type-check`

### TailwindCSS Styling
Utility-first CSS framework for rapid UI development:
```vue
<div class="flex items-center justify-between p-4 bg-white rounded-lg shadow">
```

### API Integration
Centralized HTTP client in `api/client.ts` handles all backend communication:

```typescript
const data = await api.get('/api/medialibrary')
```

## Quick Start Checklists

### Development Setup Checklist
- [ ] Node.js 18+ or Bun installed
- [ ] `bun install` completed
- [ ] `bun run dev` starts without errors
- [ ] Open http://localhost:5173 in browser
- [ ] See UI render and hot-reload works

### New Feature Checklist
- [ ] Component created in `src/components/`
- [ ] Props defined with `<script setup lang="ts">`
- [ ] Component used in at least one view
- [ ] Test file created `__tests__/Component.spec.ts`
- [ ] `bun run type-check` passes
- [ ] `bun run test:unit` passes
- [ ] `bun run lint` passes (no errors)
- [ ] Code committed with descriptive message

### Build & Deploy Checklist
- [ ] Local dev works: `bun run dev` runs
- [ ] Type check passes: `bun run type-check`
- [ ] Tests pass: `bun run test:unit`
- [ ] Build succeeds: `bun run build`
- [ ] Solution builds: `dotnet build` (from repo root)
- [ ] Ready to commit and merge

## Troubleshooting Quick Links

| Issue | Solution |
|---|---|
| Setup fails | [DEVELOPMENT.md - Prerequisites](DEVELOPMENT.md#prerequisites) |
| Dev server won't start | [DEVELOPMENT.md - Development Server Issues](DEVELOPMENT.md#development-server-issues) |
| HMR not working | [DEVELOPMENT.md - Common Issues - HMR](DEVELOPMENT.md#hmr-not-working) |
| Build fails | [DEVELOPMENT.md - Common Issues - Build](DEVELOPMENT.md#build-fails-on-dotnet-build) |
| Type errors | [DEVELOPMENT.md - TypeScript Best Practices](DEVELOPMENT.md#typescript-best-practices) |
| Test failures | [DEVELOPMENT.md - Testing Strategy](DEVELOPMENT.md#testing-strategy) |
| State not updating | [DEVELOPMENT.md - Common Issues - Store State](DEVELOPMENT.md#store-state-not-updating) |
| Performance issues | [DEVELOPMENT.md - Performance Tips](DEVELOPMENT.md#performance-tips) |

## File Structure at a Glance

```
docs/plugins/UI/
├── README.md              ← Start here for overview
├── DEVELOPMENT.md         ← Developer guide & tutorials
├── INDEX.md               ← You are here

plugins/StreamSharp.UI/
├── src/
│   ├── main.ts           (Entry point)
│   ├── App.vue           (Root component)
│   ├── components/       (Reusable components)
│   ├── views/            (Page components)
│   ├── stores/           (Pinia state)
│   ├── router/           (Route definitions)
│   ├── api/              (HTTP client)
│   └── data/             (Types & constants)
├── public/               (Static assets)
├── package.json          (Dependencies)
├── vite.config.ts        (Build config)
├── tsconfig.json         (TypeScript config)
├── AGENTS.md             (Existing guidelines)
└── README.md             (This documentation)
```

## Related Documentation

- [agents.md - Build & Test](../../agents.md#build--test)
- [agents.md - Plugin System](../../agents.md#plugin-system)
- [PostgreSQL Plugin Documentation](../PostgreSQL/README.md) — Backend data persistence
- [StreamSharp Architecture](../../agents.md#architecture)

## Development Workflow Summary

```
Typical Day:

1. Start dev server in terminal:
   $ bun run dev

2. Make changes in your editor
   - Components in src/components/
   - Views in src/views/
   - State in src/stores/

3. Browser auto-refreshes via HMR

4. Before committing:
   $ bun run type-check    # Check types
   $ bun run lint          # Fix linting
   $ bun run test:unit     # Run tests
   $ git commit -m "..."

5. Solution builds via MSBuild:
   $ dotnet build
```

## Quick Commands Reference

### Development
```bash
bun run dev                 # Start dev server (localhost:5173)
bun run type-check --watch  # Watch for type errors
bun run test:unit -- --watch # Watch tests
```

### Before Committing
```bash
bun run type-check          # Type check entire project
bun run lint                # Lint and fix code
bun run test:unit           # Run all tests
bun run build               # Production build
```

### Debugging
```bash
bun run build               # Builds (catches build issues)
bun run preview             # Preview production build
# Use Vue DevTools in browser for runtime debugging
```

### Maintenance
```bash
bun install                 # Install/update dependencies
bun update                  # Update packages
rm -r node_modules && bun install  # Clean reinstall
```

## Support & Resources

### Questions or Issues

1. **Check relevant documentation**:
   - Technical question? → [README](README.md) or [DEVELOPMENT.md](DEVELOPMENT.md)
   - Need to debug? → [DEVELOPMENT.md - Debugging](DEVELOPMENT.md#debugging)
   - Build failing? → [DEVELOPMENT.md - Build Issues](DEVELOPMENT.md#build-issues)

2. **Search error messages**:
   - Look in [DEVELOPMENT.md - Troubleshooting](DEVELOPMENT.md#troubleshooting-guide)
   - Check [Common Issues & Solutions](DEVELOPMENT.md#common-issues--solutions)

3. **Ask for help**:
   - Share error message and steps to reproduce
   - Run diagnostics: `bun run type-check` and `bun run build`

### External Resources

- [Vue 3 Documentation](https://vuejs.org/)
- [Pinia Store Documentation](https://pinia.vuejs.org/)
- [Vite Guide](https://vitejs.dev/)
- [TailwindCSS Documentation](https://tailwindcss.com/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Vitest Documentation](https://vitest.dev/)

---

**Last Updated**: 2026-01-09  
**Documentation Version**: 1.0  
**Vue Version**: 3.5.17  
**TypeScript Version**: 5.7+  
**TailwindCSS Version**: 3.4.11
