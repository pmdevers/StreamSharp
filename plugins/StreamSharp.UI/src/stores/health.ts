import { defineStore } from 'pinia'

function toErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message) {
    return error.message
  }

  if (typeof error === 'string' && error.length > 0) {
    return error
  }

  return fallback
}

async function checkEndpoint(path: string): Promise<boolean> {
  const response = await fetch(path)
  return response.ok
}

export const useHealthStore = defineStore('health', {
  state: () => ({
    healthz: null as boolean | null,
    readyz: null as boolean | null,
    livez: null as boolean | null,
    isLoading: false,
    error: null as string | null,
    lastCheckedAt: null as string | null,
  }),
  getters: {
    isHealthy(state) {
      return state.healthz === true && state.readyz === true && state.livez === true
    },
  },
  actions: {
    clearError() {
      this.error = null
    },
    async refresh() {
      this.isLoading = true
      this.error = null

      try {
        const [healthz, readyz, livez] = await Promise.all([
          checkEndpoint('/healthz'),
          checkEndpoint('/readyz'),
          checkEndpoint('/livez'),
        ])

        this.healthz = healthz
        this.readyz = readyz
        this.livez = livez
        this.lastCheckedAt = new Date().toISOString()
      } catch (error) {
        this.error = toErrorMessage(error, 'Failed to check service health.')
      } finally {
        this.isLoading = false
      }
    },
  },
})