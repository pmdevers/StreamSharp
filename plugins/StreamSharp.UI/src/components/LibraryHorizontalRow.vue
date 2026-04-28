<script setup lang="ts">
import LibraryItemCard from '@/components/LibraryItemCard.vue'
import type { LibraryItem } from '@/stores/mediaLibrary'

interface Props {
  title: string
  description?: string
  items: LibraryItem[]
  selectedItemId?: string
}

defineProps<Props>()

const emit = defineEmits<{
  (e: 'select-item', item: LibraryItem): void
}>()

function selectItem(item: LibraryItem) {
  emit('select-item', item)
}
</script>

<template>
  <section class="space-y-4">
    <header class="space-y-1">
      <h2 class="text-xl font-semibold text-white">{{ title }}</h2>
      <p v-if="description" class="text-sm text-white/65">{{ description }}</p>
    </header>

    <div class="flex gap-4 overflow-x-auto pb-2">
      <LibraryItemCard
        v-for="item in items"
        :key="item.id"
        :item="item"
        :selected="selectedItemId === item.id"
        @select="selectItem"
      />
    </div>
  </section>
</template>
