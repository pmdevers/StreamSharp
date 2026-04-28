<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

import { useAdminStore } from '@/stores/admin'

interface Props {
  menuItems: Array<{
    id: string
    label: string
  }>
  headingTag?: 'h1' | 'h2'
  fullHeight?: boolean
}

withDefaults(defineProps<Props>(), {
  headingTag: 'h1',
  fullHeight: false,
})

const emit = defineEmits<{
  (e: 'item-click'): void
}>()

function handleItemClick() {
  emit('item-click')
}

const route = useRoute()
const adminStore = useAdminStore()
const currentLibraryId = computed(() => String(route.params.libraryId ?? 'movies'))
const adminRoutes = computed(() => adminStore.adminRoutes)
const activeRouteName = computed(() => String(route.name ?? ''))
</script>

<template>
  <aside
    :class="[
      'w-72 border-r border-white/10 bg-panel px-4 py-6',
      fullHeight ? 'h-full' : '',
    ]"
  >
    <div class="mb-8 px-2">
      <p class="text-xs uppercase tracking-[0.24em] text-white/55">StreamSharp</p>
      <component :is="headingTag" class="mt-2 text-2xl font-bold">Library</component>
    </div>

    <nav :class="['flex flex-col gap-2', fullHeight ? '' : 'flex-1']">
      <RouterLink
        v-for="item in menuItems"
        :key="item.id"
        :to="{ name: 'library', params: { libraryId: item.id } }"
        :class="[
          'rounded-xl px-3 py-2 text-sm font-medium transition',
          currentLibraryId === item.id
            ? 'bg-white/12 text-white'
            : 'text-white/70 hover:bg-white/10 hover:text-white',
        ]"
        @click="handleItemClick"
      >
        {{ item.label }}
      </RouterLink>

      <template v-if="adminRoutes.length > 0">
        <div class="my-3 border-t border-white/10"></div>

        <p class="px-3 pt-1 text-xs uppercase tracking-[0.2em] text-white/40">Admin</p>

        <RouterLink
          v-for="adminRoute in adminRoutes"
          :key="adminRoute.name"
          :to="{ name: adminRoute.name }"
          :class="[
            'rounded-xl px-3 py-2 text-sm font-medium transition',
            activeRouteName === adminRoute.name
              ? 'bg-white/12 text-white'
              : 'text-white/70 hover:bg-white/10 hover:text-white',
          ]"
          @click="handleItemClick"
        >
          {{ adminRoute.label }}
        </RouterLink>
      </template>
    </nav>
  </aside>
</template>
