<script setup lang="ts">
import type { LibraryItem } from '@/stores/mediaLibrary'

interface Props {
  item: LibraryItem
  selected?: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  (e: 'select', item: LibraryItem): void
}>()

function selectItem(item: LibraryItem) {
  emit('select', item)
}
</script>

<template>
  <button
    type="button"
    :class="[
      'group relative min-h-60 min-w-[400px] overflow-hidden border-[5px] bg-panel p-4 text-left shadow-card transition',
      selected ? 'border-white/55 ring-1 ring-white/40' : 'border-white/10 hover:border-white/30',
    ]"
    :style="{ backgroundImage: item.art }"
    @click="selectItem(item)"
  >
    <div class="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent"></div>

    <div class="relative flex h-full flex-col justify-end gap-1">
      <p
        v-if="item.tag"
        class="w-fit border border-white/20 bg-black/30 px-2 py-0.5 text-[10px] uppercase tracking-[0.12em] text-white/80"
      >
        {{ item.tag }}
      </p>
      <h3 class="text-base font-semibold text-white">{{ item.title }}</h3>
      <p class="text-xs text-white/80">{{ item.subtitle }}</p>
      <p class="text-xs text-white/70">{{ item.meta }}</p>
    </div>
  </button>
</template>
