<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { computed, onMounted, ref, watch } from 'vue'

import { useMediaLibraryStore } from '@/stores/mediaLibrary'

const mediaLibraryStore = useMediaLibraryStore()

const {
  libraries,
  libraryItems,
  loadingItemsByLibrary,
  isLoadingLibraries,
  isCreatingLibrary,
  isScanningLibrary,
} = storeToRefs(mediaLibraryStore)

const selectedLibraryId = ref('')
const newLibraryName = ref('')
const activityMessage = ref<string | null>(null)

const selectedLibrary = computed(() =>
  libraries.value.find((library) => library.id === selectedLibraryId.value) ?? null,
)

const selectedLibraryItems = computed(() =>
  libraryItems.value.filter((item) => item.libraryId === selectedLibraryId.value),
)

const selectedLibraryTypes = computed(() => {
  const counts = new Map<string, number>()

  for (const item of selectedLibraryItems.value) {
    counts.set(item.genre, (counts.get(item.genre) ?? 0) + 1)
  }

  return [...counts.entries()].map(([label, count]) => ({ label, count }))
})

watch(
  libraries,
  (items) => {
    if (!items.length) {
      selectedLibraryId.value = ''
      return
    }

    if (!items.some((item) => item.id === selectedLibraryId.value)) {
      selectedLibraryId.value = items[0].id
    }
  },
  { immediate: true },
)

async function handleCreateLibrary() {
  const trimmedName = newLibraryName.value.trim()
  if (!trimmedName) return

  const createdLibrary = await mediaLibraryStore.createLibrary(trimmedName)
  if (createdLibrary) {
    selectedLibraryId.value = createdLibrary.id
    newLibraryName.value = ''
    activityMessage.value = `Created library ${createdLibrary.title}.`
  }
}

async function handleRefreshLibrary() {
  if (!selectedLibraryId.value) return

  await mediaLibraryStore.loadItemsForLibrary(selectedLibraryId.value)
  const libraryTitle = selectedLibrary.value?.title ?? 'Library'
  activityMessage.value = `Loaded ${selectedLibraryItems.value.length} items for ${libraryTitle}.`
}

async function handleScanLibrary() {
  if (!selectedLibraryId.value) return

  const success = await mediaLibraryStore.scanLibrary(selectedLibraryId.value)
  if (success) {
    activityMessage.value = `Scan started for ${selectedLibrary.value?.title ?? 'library'}.`
  }
}

onMounted(() => {
  void mediaLibraryStore.loadLibraries()
})
</script>

<template>
  <div class="space-y-6">
    <section class="panel-card p-6">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p class="text-xs uppercase tracking-[0.24em] text-white/55">Media Library</p>
          <h2 class="mt-2 text-xl font-semibold text-white">Libraries</h2>
          <p class="mt-2 text-sm text-white/70">Browse, create, and scan your media libraries.</p>
        </div>

        <div class="flex flex-wrap gap-2">
          <button
            type="button"
            class="btn-ghost"
            :disabled="isLoadingLibraries"
            @click="mediaLibraryStore.loadLibraries"
          >
            {{ isLoadingLibraries ? 'Loading...' : 'Refresh libraries' }}
          </button>
          <button
            type="button"
            class="btn-ghost"
            :disabled="!selectedLibraryId || loadingItemsByLibrary[selectedLibraryId]"
            @click="handleRefreshLibrary"
          >
            {{ selectedLibraryId && loadingItemsByLibrary[selectedLibraryId] ? 'Loading items...' : 'Load items' }}
          </button>
          <button
            type="button"
            class="btn-primary"
            :disabled="!selectedLibraryId || isScanningLibrary"
            @click="handleScanLibrary"
          >
            {{ isScanningLibrary ? 'Scanning...' : 'Scan selected' }}
          </button>
        </div>
      </div>

      <div class="mt-5 grid gap-4 lg:grid-cols-[0.95fr_1.05fr]">
        <div class="space-y-3">
          <label class="text-xs uppercase tracking-[0.2em] text-white/55" for="library-select">Active library</label>
          <select
            id="library-select"
            v-model="selectedLibraryId"
            class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm text-white outline-none"
          >
            <option v-for="library in libraries" :key="library.id" :value="library.id" class="bg-background text-white">
              {{ library.title }}
            </option>
          </select>

          <div class="glass-panel p-4">
            <p class="text-sm font-medium text-white">{{ selectedLibrary?.title ?? 'No library selected' }}</p>
            <p class="mt-2 text-sm text-white/65">{{ selectedLibrary?.description ?? 'Choose a library to inspect its current item state.' }}</p>
            <p v-if="selectedLibrary?.createdAt" class="mt-3 text-xs text-white/50">
              Created {{ new Date(selectedLibrary.createdAt).toLocaleString() }}
            </p>
          </div>

          <div class="glass-panel p-4">
            <label class="text-xs uppercase tracking-[0.2em] text-white/55" for="new-library-name">Create library</label>
            <div class="mt-3 flex flex-col gap-3 sm:flex-row">
              <input
                id="new-library-name"
                v-model="newLibraryName"
                type="text"
                placeholder="New library name"
                class="flex-1 rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm text-white outline-none placeholder:text-white/35"
              />
              <button
                type="button"
                class="btn-primary"
                :disabled="!newLibraryName.trim() || isCreatingLibrary"
                @click="handleCreateLibrary"
              >
                {{ isCreatingLibrary ? 'Creating...' : 'Create' }}
              </button>
            </div>
          </div>
        </div>

        <div class="space-y-4">
          <div class="grid gap-3 sm:grid-cols-3">
            <div class="glass-panel p-4">
              <p class="text-xs uppercase tracking-[0.2em] text-white/55">Libraries</p>
              <p class="mt-2 text-2xl font-semibold text-white">{{ libraries.length }}</p>
            </div>
            <div class="glass-panel p-4">
              <p class="text-xs uppercase tracking-[0.2em] text-white/55">Loaded items</p>
              <p class="mt-2 text-2xl font-semibold text-white">{{ selectedLibraryItems.length }}</p>
            </div>
            <div class="glass-panel p-4">
              <p class="text-xs uppercase tracking-[0.2em] text-white/55">Types</p>
              <p class="mt-2 text-2xl font-semibold text-white">{{ selectedLibraryTypes.length }}</p>
            </div>
          </div>

          <div class="glass-panel p-4">
            <p class="text-xs uppercase tracking-[0.2em] text-white/55">Type distribution</p>
            <div class="mt-4 space-y-3">
              <div v-if="selectedLibraryTypes.length === 0" class="text-sm text-white/55">
                Load items to inspect grouped media types.
              </div>
              <div
                v-for="entry in selectedLibraryTypes"
                :key="entry.label"
                class="flex items-center justify-between border-b border-white/10 pb-2 text-sm text-white/80 last:border-b-0 last:pb-0"
              >
                <span>{{ entry.label }}</span>
                <span class="font-medium text-white">{{ entry.count }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <div v-if="activityMessage" class="glass-panel px-5 py-4 text-sm text-white/80">
      {{ activityMessage }}
    </div>

    <div v-if="mediaLibraryStore.error" class="rounded-xl border border-error/30 bg-error/10 px-5 py-4 text-sm text-white/85">
      {{ mediaLibraryStore.error }}
    </div>
  </div>
</template>
