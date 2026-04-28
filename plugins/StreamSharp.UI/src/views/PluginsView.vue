<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { onMounted, ref } from 'vue'

import { usePluginsStore } from '@/stores/plugins'

const pluginsStore = usePluginsStore()
const { plugins, isLoading: isLoadingPlugins, isInstalling, isUninstalling } = storeToRefs(pluginsStore)

const pluginFile = ref<File | null>(null)
const pluginInputKey = ref(0)
const activePlugin = ref<string | null>(null)
const activityMessage = ref<string | null>(null)

function handlePluginFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  pluginFile.value = target.files?.[0] ?? null
}

async function handleInstallPlugin() {
  if (!pluginFile.value) return

  const fileName = pluginFile.value.name
  const installed = await pluginsStore.installPlugin(pluginFile.value)
  if (installed) {
    activityMessage.value = `Installed plugin ${fileName}.`
    pluginFile.value = null
    pluginInputKey.value += 1
  }
}

async function handleUninstallPlugin(name: string) {
  activePlugin.value = name
  const removed = await pluginsStore.uninstallPlugin(name)
  activePlugin.value = null

  if (removed) {
    activityMessage.value = `Uninstalled plugin ${name}.`
  }
}

onMounted(() => {
  void pluginsStore.loadPlugins()
})
</script>

<template>
  <div class="space-y-6">
    <section class="panel-card p-6">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p class="text-xs uppercase tracking-[0.24em] text-white/55">Plugins</p>
          <h2 class="mt-2 text-xl font-semibold text-white">Plugin management</h2>
          <p class="mt-2 text-sm text-white/70">Install and uninstall backend plugin packages.</p>
        </div>

        <button
          type="button"
          class="btn-ghost"
          :disabled="isLoadingPlugins"
          @click="pluginsStore.loadPlugins"
        >
          {{ isLoadingPlugins ? 'Refreshing...' : 'Refresh plugins' }}
        </button>
      </div>

      <div class="mt-5 grid gap-5 xl:grid-cols-[0.8fr_1.2fr]">
        <div class="glass-panel p-4">
          <label :key="pluginInputKey" class="text-xs uppercase tracking-[0.2em] text-white/55" for="plugin-file">Install plugin</label>
          <input
            id="plugin-file"
            type="file"
            class="mt-3 block w-full text-sm text-white/80 file:mr-4 file:rounded-full file:border-0 file:bg-white/10 file:px-4 file:py-2 file:text-sm file:font-medium file:text-white hover:file:bg-white/15"
            @change="handlePluginFileChange"
          />
          <button
            type="button"
            class="btn-primary mt-4"
            :disabled="!pluginFile || isInstalling"
            @click="handleInstallPlugin"
          >
            {{ isInstalling ? 'Installing...' : 'Install selected file' }}
          </button>
        </div>

        <div class="glass-panel p-4">
          <div class="flex items-center justify-between">
            <p class="text-xs uppercase tracking-[0.2em] text-white/55">Installed</p>
            <p class="text-sm text-white/55">{{ plugins.length }} total</p>
          </div>

          <div class="mt-4 space-y-3">
            <div v-if="plugins.length === 0" class="text-sm text-white/55">
              No installed plugins reported by the server.
            </div>
            <div
              v-for="plugin in plugins"
              :key="plugin"
              class="flex flex-col gap-3 rounded-2xl border border-white/10 bg-white/5 px-4 py-4 sm:flex-row sm:items-center sm:justify-between"
            >
              <div>
                <p class="font-medium text-white">{{ plugin }}</p>
                <p class="mt-1 text-sm text-white/60">Plugin package registered with the backend.</p>
              </div>

              <button
                type="button"
                class="btn-ghost"
                :disabled="isUninstalling && activePlugin === plugin"
                @click="handleUninstallPlugin(plugin)"
              >
                {{ isUninstalling && activePlugin === plugin ? 'Removing...' : 'Uninstall' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <div v-if="activityMessage" class="glass-panel px-5 py-4 text-sm text-white/80">
      {{ activityMessage }}
    </div>

    <div v-if="pluginsStore.error" class="rounded-xl border border-error/30 bg-error/10 px-5 py-4 text-sm text-white/85">
      {{ pluginsStore.error }}
    </div>
  </div>
</template>
