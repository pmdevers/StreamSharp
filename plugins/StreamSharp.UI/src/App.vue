<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { computed, onBeforeUnmount, onMounted } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'

import AppHeader from '@/components/AppHeader.vue'
import AppSidebar from '@/components/AppSidebar.vue'
import { useHomeStore } from '@/stores/home'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'
import { useUiStore } from '@/stores/ui'

const uiStore = useUiStore()
const homeStore = useHomeStore()
const mediaLibraryStore = useMediaLibraryStore()
const route = useRoute()
const router = useRouter()

router.beforeEach(() => {
  homeStore.selectItem(null)
})
const { menuOpen } = storeToRefs(uiStore)
const { selectedItem } = storeToRefs(homeStore)
const { libraries } = storeToRefs(mediaLibraryStore)

const menuItems = computed(() =>
  libraries.value.map((library) => ({
    id: library.id,
    label: library.title,
  })),
)

function toggleMenu() {
  if (menuOpen.value) {
    uiStore.closeMenu()
    return
  }

  uiStore.openMenu()
}

function closeMenu() {
  uiStore.closeMenu()
}

let desktopMediaQuery: MediaQueryList | null = null

function syncMenuForViewport(query: MediaQueryList | MediaQueryListEvent) {
  if (query.matches) {
    uiStore.openMenu()
    return
  }

  uiStore.closeMenu()
}

onMounted(() => {
  desktopMediaQuery = window.matchMedia('(min-width: 1024px)')
  syncMenuForViewport(desktopMediaQuery)
  desktopMediaQuery.addEventListener('change', syncMenuForViewport)
  void mediaLibraryStore.loadLibraries()
})

onBeforeUnmount(() => {
  desktopMediaQuery?.removeEventListener('change', syncMenuForViewport)
})
</script>

<template>
  <div class="min-h-screen bg-background text-textPrimary">
    <div class="flex min-h-screen">
      <div v-if="menuOpen" class="hidden lg:fixed lg:inset-y-0 lg:left-0 lg:flex">
        <AppSidebar :menu-items="menuItems" class="shrink-0 bg-panel/70 backdrop-blur" />
      </div>

      <div :class="['flex min-h-screen w-full flex-col', menuOpen ? 'lg:pl-72' : '']">
        <AppHeader
          :menu-open="menuOpen"
          :menu-items="menuItems"
          :selected-item="selectedItem"
          @toggle-menu="toggleMenu"
        />

        <main class="flex-1 px-4 py-6 sm:px-6 sm:py-8">
          <RouterView />
        </main>
      </div>
    </div>

    <div v-if="menuOpen" class="fixed inset-0 z-50 lg:hidden" role="dialog" aria-modal="true">
      <button
        type="button"
        class="absolute inset-0 bg-black/60"
        aria-label="Close menu"
        @click="closeMenu"
      ></button>

      <AppSidebar
        :menu-items="menuItems"
        heading-tag="h2"
        :full-height="true"
        class="relative shadow-2xl"
        @item-click="closeMenu"
      />
    </div>
  </div>
</template>
