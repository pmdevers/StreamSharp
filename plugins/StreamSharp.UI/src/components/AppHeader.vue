<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

import type { LibraryItem } from '@/stores/mediaLibrary'

interface Props {
  title?: string
  profileLabel?: string
  menuOpen?: boolean
  menuItems?: Array<{
    id: string
    label: string
  }>
  selectedItem?: LibraryItem | null
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Home',
  profileLabel: 'Profile',
  menuOpen: false,
  menuItems: () => [],
  selectedItem: null,
})

const emit = defineEmits<{
  (e: 'toggle-menu'): void
}>()

function handleToggleMenu() {
  emit('toggle-menu')
}

const route = useRoute()
const currentLibraryId = computed(() => String(route.params.libraryId ?? 'movies'))
const isAdminRoute = computed(() => route.meta.admin === true)

const adminPageInfo = computed<{ title: string; subtitle: string; meta: string } | null>(() => {
  if (!isAdminRoute.value) return null

  switch (route.name) {
    case 'admin-libraries':
      return {
        title: 'Libraries',
        subtitle: 'Browse, create, scan, and load items across your media libraries.',
        meta: 'Media library management',
      }
    case 'admin-plugins':
      return {
        title: 'Plugin management',
        subtitle: 'Install and uninstall backend plugin packages.',
        meta: 'Plugin management',
      }
    default:
      return {
        title: 'Admin Console',
        subtitle: 'Monitor health checks and operate server controls.',
        meta: 'Operational controls',
      }
  }
})

const headerBackground = computed(() => {
  if (!isAdminRoute.value && props.selectedItem?.art) {
    return props.selectedItem.art
  }

  return 'linear-gradient(135deg, rgba(22,29,42,0.95), rgba(56,86,128,0.7))'
})

const displayTitle = computed(() => adminPageInfo.value?.title ?? props.selectedItem?.title ?? props.title)
const displaySubtitle = computed(() => adminPageInfo.value?.subtitle ?? props.selectedItem?.subtitle ?? 'Select a title from your library')
const displayMeta = computed(() => adminPageInfo.value?.meta ?? props.selectedItem?.meta ?? 'Ready to browse')
</script>

<template>
  <header
    class="sticky top-0 z-20 h-[40vh] min-h-[180px] max-h-[360px] overflow-hidden border-b border-white/10"
    :style="{ backgroundImage: headerBackground }"
  >
    <div class="absolute inset-0 bg-gradient-to-r from-black/60 via-black/25 to-black/10"></div>
    <div class="absolute inset-x-0 bottom-0 h-24 bg-gradient-to-b from-transparent to-background"></div>

    <div class="relative flex h-full flex-col justify-between px-4 py-4 sm:px-6 sm:py-5">
      <div class="relative flex items-center justify-between">
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="inline-flex h-10 w-10 items-center justify-center border border-white/15 bg-white/5 text-white transition hover:bg-white/10"
            @click="handleToggleMenu"
          >
            <span class="sr-only">{{ menuOpen ? 'Close menu' : 'Open menu' }}</span>
            <span class="text-xl leading-none">{{ menuOpen ? '×' : '≡' }}</span>
          </button>
        </div>

        <div class="border border-white/10 bg-white/5 px-3 py-1 text-xs text-white/75">
          {{ profileLabel }}
        </div>

        <nav
          v-if="!menuOpen"
          class="absolute left-1/2 top-1/2 hidden -translate-x-1/2 -translate-y-1/2 items-center gap-2 lg:flex"
        >
          <RouterLink
            :to="{ name: 'admin' }"
            :class="[
              'border px-3 py-1 text-xs uppercase tracking-[0.16em] transition',
              isAdminRoute
                ? 'border-white/35 bg-white/12 text-white'
                : 'border-white/10 bg-black/15 text-white/75 hover:border-white/25 hover:text-white',
            ]"
          >
            Admin
          </RouterLink>

          <RouterLink
            v-for="item in menuItems"
            :key="item.id"
            :to="{ name: 'library', params: { libraryId: item.id } }"
            :class="[
              'border px-3 py-1 text-xs uppercase tracking-[0.16em] transition',
              currentLibraryId === item.id
                ? 'border-white/35 bg-white/12 text-white'
                : 'border-white/10 bg-black/15 text-white/75 hover:border-white/25 hover:text-white',
            ]"
          >
            {{ item.label }}
          </RouterLink>
        </nav>
      </div>

      <div class="max-w-2xl space-y-1 pb-2">
        <p class="text-xs uppercase tracking-[0.18em] text-white/65">{{ isAdminRoute ? (route.meta.label ?? 'Admin') : 'Selected item' }}</p>
        <h1 class="text-2xl font-semibold text-white sm:text-3xl">{{ displayTitle }}</h1>
        <p class="text-sm text-white/85 sm:text-base">{{ displaySubtitle }}</p>
        <p class="text-xs text-white/70">{{ displayMeta }}</p>
      </div>
    </div>
  </header>
</template>
