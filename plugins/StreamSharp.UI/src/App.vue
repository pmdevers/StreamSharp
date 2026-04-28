<script setup lang="ts">
import { RouterLink, RouterView, useRoute } from 'vue-router'

import AppFooter from '@/components/AppFooter.vue'

const route = useRoute()
</script>

<template>
  <div class="min-h-screen bg-background text-textPrimary">
    <header class="sticky top-0 z-40 border-b border-[#ead8c9] bg-backgroundDark/95 text-white backdrop-blur-xl">
      <div class="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
        <RouterLink to="/" class="flex items-center gap-3">
          <div
            class="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary text-lg font-black text-backgroundDark shadow-warm"
          >
            J
          </div>
          <div>
            <p class="text-xs font-semibold uppercase tracking-[0.4em] text-white/65">Jailey's Trukipan</p>
            <p class="text-lg font-semibold text-white">Curaçao street food delivery</p>
          </div>
        </RouterLink>

        <nav class="hidden items-center gap-2 rounded-full border border-white/10 bg-white/5 p-1 text-sm text-white/80 md:flex">
          <RouterLink
            to="/"
            class="rounded-full px-4 py-2 transition hover:bg-white/10"
            :class="route.path === '/' ? 'bg-white text-backgroundDark' : ''"
          >
            Home
          </RouterLink>
          <RouterLink
            to="/track-order"
            class="rounded-full px-4 py-2 transition hover:bg-white/10"
            :class="route.path === '/track-order' ? 'bg-white text-backgroundDark' : ''"
          >
            Track order
          </RouterLink>
          <RouterLink
            to="/our-story"
            class="rounded-full px-4 py-2 transition hover:bg-white/10"
            :class="route.path === '/our-story' ? 'bg-white text-backgroundDark' : ''"
          >
            Our story
          </RouterLink>
        </nav>

        <a
          v-if="route.path === '/'"
          href="#featured-menu"
          class="btn-primary"
        >
          Browse menu
        </a>
        <RouterLink
          v-else
          to="/"
          class="btn-primary"
        >
          Back home
        </RouterLink>
      </div>
    </header>

    <main>
      <RouterView />
    </main>

    <AppFooter />
  </div>
</template>
