<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { computed, onMounted, ref } from 'vue'

import { useHealthStore } from '@/stores/health'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'
import { useServerStore } from '@/stores/server'

const mediaLibraryStore = useMediaLibraryStore()
const serverStore = useServerStore()
const healthStore = useHealthStore()

const {
  isLoadingLibraries,
} = storeToRefs(mediaLibraryStore)
const { isRestarting, isShuttingDown } = storeToRefs(serverStore)
const {
  healthz,
  readyz,
  livez,
  isHealthy,
  isLoading: isLoadingHealth,
  lastCheckedAt,
} = storeToRefs(healthStore)

const activityMessage = ref<string | null>(null)

const adminErrors = computed(() => [
  mediaLibraryStore.error,
  serverStore.error,
  healthStore.error,
].filter((value): value is string => Boolean(value)))

const healthCards = computed(() => [
  { id: 'healthz', label: 'Health', value: healthz.value },
  { id: 'readyz', label: 'Ready', value: readyz.value },
  { id: 'livez', label: 'Live', value: livez.value },
])

async function refreshAdminData() {
  await Promise.all([
    mediaLibraryStore.loadLibraries(),
    healthStore.refresh(),
  ])
}

async function handleRestartServer() {
  const restarted = await serverStore.restart()
  if (restarted) {
    activityMessage.value = 'Restart request sent to the server.'
  }
}

async function handleShutdownServer() {
  const shutDown = await serverStore.shutdown()
  if (shutDown) {
    activityMessage.value = 'Shutdown request sent to the server.'
  }
}

onMounted(() => {
  void refreshAdminData()
})
</script>

<template>
  <div class="space-y-8">
    <section class="grid gap-4 lg:grid-cols-[1.25fr_0.75fr]">
      <div class="panel-card p-6">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p class="text-xs uppercase tracking-[0.24em] text-white/55">Service</p>
            <h2 class="mt-2 text-2xl font-semibold text-white">Control room</h2>
            <p class="mt-2 max-w-2xl text-sm text-white/70">
              Monitor health checks, manage libraries, and operate server controls from one screen.
            </p>
          </div>

          <button
            type="button"
            class="btn-ghost"
            :disabled="isLoadingLibraries || isLoadingHealth"
            @click="refreshAdminData"
          >
            {{ isLoadingLibraries || isLoadingHealth ? 'Refreshing...' : 'Refresh all' }}
          </button>
        </div>

        <div class="mt-6 grid gap-3 sm:grid-cols-3">
          <div
            v-for="card in healthCards"
            :key="card.id"
            class="glass-panel p-4"
          >
            <p class="text-xs uppercase tracking-[0.2em] text-white/55">{{ card.label }}</p>
            <p
              :class="[
                'mt-3 text-2xl font-semibold',
                card.value === true ? 'text-accent' : card.value === false ? 'text-error' : 'text-white',
              ]"
            >
              {{ card.value === null ? 'Unknown' : card.value ? 'OK' : 'Offline' }}
            </p>
          </div>
        </div>

        <p class="mt-4 text-xs text-white/55">
          Overall status: {{ isHealthy ? 'Healthy' : 'Needs attention' }}
          <span v-if="lastCheckedAt"> · Last check {{ new Date(lastCheckedAt).toLocaleString() }}</span>
        </p>
      </div>

      <div class="panel-card p-6">
        <p class="text-xs uppercase tracking-[0.24em] text-white/55">Activity</p>
        <p class="mt-2 text-lg font-semibold text-white">Latest action</p>
        <p class="mt-3 text-sm text-white/70">
          {{ activityMessage ?? 'No admin action has been triggered in this session.' }}
        </p>

        <div v-if="adminErrors.length" class="mt-5 space-y-2">
          <p class="text-xs uppercase tracking-[0.2em] text-error">Errors</p>
          <p
            v-for="error in adminErrors"
            :key="error"
            class="rounded-xl border border-error/30 bg-error/10 px-3 py-2 text-sm text-white/85"
          >
            {{ error }}
          </p>
        </div>
      </div>
    </section>

    <section class="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
      <RouterLink
        :to="{ name: 'admin-libraries' }"
        class="panel-card block p-6 transition hover:ring-1 hover:ring-white/20"
      >
        <p class="text-xs uppercase tracking-[0.24em] text-white/55">Media Library</p>
        <h2 class="mt-2 text-xl font-semibold text-white">Libraries</h2>
        <p class="mt-2 text-sm text-white/70">Browse, create, scan, and load items across all libraries.</p>
        <p class="mt-4 text-xs text-accent">Manage libraries →</p>
      </RouterLink>

      <div class="panel-card p-6">
        <p class="text-xs uppercase tracking-[0.24em] text-white/55">Server</p>
        <h2 class="mt-2 text-xl font-semibold text-white">Operations</h2>
        <p class="mt-2 text-sm text-white/70">Send lifecycle commands to the backend process.</p>

        <div class="mt-6 space-y-3">
          <button
            type="button"
            class="btn-primary w-full justify-center"
            :disabled="isRestarting"
            @click="handleRestartServer"
          >
            {{ isRestarting ? 'Restarting...' : 'Restart server' }}
          </button>
          <button
            type="button"
            class="btn-ghost w-full justify-center"
            :disabled="isShuttingDown"
            @click="handleShutdownServer"
          >
            {{ isShuttingDown ? 'Shutting down...' : 'Shutdown server' }}
          </button>
        </div>
      </div>

      <RouterLink
        :to="{ name: 'admin-plugins' }"
        class="panel-card block p-6 transition hover:ring-1 hover:ring-white/20"
      >
        <p class="text-xs uppercase tracking-[0.24em] text-white/55">Plugins</p>
        <h2 class="mt-2 text-xl font-semibold text-white">Plugin management</h2>
        <p class="mt-2 text-sm text-white/70">Install and uninstall backend plugin packages.</p>
        <p class="mt-4 text-xs text-accent">Manage plugins →</p>
      </RouterLink>
    </section>
  </div>
</template>