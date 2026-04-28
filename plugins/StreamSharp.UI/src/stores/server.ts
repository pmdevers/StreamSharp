import { defineStore } from 'pinia'

import { apiClient } from '@/api/client'

type ApiClient = {
  GET: (path: string) => Promise<{ data?: unknown; error?: unknown }>
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

export const useServerStore = defineStore('server', {
  state: () => ({
    isRestarting: false,
    isShuttingDown: false,
    error: null as string | null,
  }),
  actions: {
    clearError() {
      this.error = null
    },
    async restart() {
      this.isRestarting = true
      this.error = null

      const client = apiClient as ApiClient
      const { error } = await client.GET('/server/restart')

      this.isRestarting = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to restart server.')
        return false
      }

      return true
    },
    async shutdown() {
      this.isShuttingDown = true
      this.error = null

      const client = apiClient as ApiClient
      const { error } = await client.GET('/server/shutdown')

      this.isShuttingDown = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to shut down server.')
        return false
      }

      return true
    },
  },
})