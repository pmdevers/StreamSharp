import { defineStore } from 'pinia'

import { apiClient } from '@/api/client'

type ApiClient = {
  GET: (path: string) => Promise<{ data?: unknown; error?: unknown }>
  POST: (
    path: string,
    options?: {
      params?: { query?: Record<string, string | number | boolean | undefined> }
    },
  ) => Promise<{ data?: unknown; error?: unknown }>
}

function toErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message) {
    return error.message
  }

  if (typeof error === 'string' && error.length > 0) {
    return error
  }

  return fallback
}

async function parseJsonResponse(response: Response): Promise<unknown> {
  if (response.status === 204) {
    return undefined
  }

  const text = await response.text()
  if (!text) {
    return undefined
  }

  try {
    return JSON.parse(text)
  } catch {
    return text
  }
}

export const usePluginsStore = defineStore('plugins', {
  state: () => ({
    plugins: [] as string[],
    isLoading: false,
    isInstalling: false,
    isUninstalling: false,
    error: null as string | null,
  }),
  actions: {
    clearError() {
      this.error = null
    },
    async loadPlugins() {
      this.isLoading = true
      this.error = null

      const client = apiClient as ApiClient
      const { data, error } = await client.GET('/plugins')

      this.isLoading = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to load plugins.')
        return
      }

      if (!Array.isArray(data)) {
        this.error = 'Unexpected plugins response.'
        return
      }

      this.plugins = data.filter((item): item is string => typeof item === 'string')
    },
    async installPlugin(pluginFile: File) {
      this.isInstalling = true
      this.error = null

      const formData = new FormData()
      formData.append('pluginFile', pluginFile)

      try {
        const response = await fetch('/api/plugins/install', {
          method: 'POST',
          body: formData,
        })

        if (!response.ok) {
          const errorBody = await parseJsonResponse(response)
          this.error = toErrorMessage(errorBody, 'Failed to install plugin.')
          return false
        }

        await this.loadPlugins()
        return true
      } catch (error) {
        this.error = toErrorMessage(error, 'Failed to install plugin.')
        return false
      } finally {
        this.isInstalling = false
      }
    },
    async uninstallPlugin(name: string) {
      this.isUninstalling = true
      this.error = null

      const client = apiClient as ApiClient
      const { error } = await client.POST('/plugins/uninstall', {
        params: { query: { name } },
      })

      this.isUninstalling = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to uninstall plugin.')
        return false
      }

      this.plugins = this.plugins.filter((plugin) => plugin !== name)
      return true
    },
  },
})