import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

export type AdminRoute = {
  name: string
  path: string
  label: string
}

export type AdminNotification = {
  id: string
  level: 'info' | 'warning' | 'error'
  message: string
  timestamp: string
}

export function useAdminRoutes(): AdminRoute[] {
  const router = useRouter()

  return router
    .getRoutes()
    .filter((route) => route.meta.admin === true && typeof route.name === 'string')
    .map((route) => ({
      name: route.name as string,
      path: route.path,
      label: typeof route.meta.label === 'string' ? route.meta.label : (route.name as string),
    }))
}

export const useAdminStore = defineStore('admin', () => {
  const router = useRouter()

  const adminRoutes = computed<AdminRoute[]>(() =>
    router
      .getRoutes()
      .filter((route) => route.meta.admin === true && typeof route.name === 'string')
      .map((route) => ({
        name: route.name as string,
        path: route.path,
        label: typeof route.meta.label === 'string' ? route.meta.label : (route.name as string),
      })),
  )

  const notifications = ref<AdminNotification[]>([])

  function addNotification(level: AdminNotification['level'], message: string) {
    const notification: AdminNotification = {
      id: `${Date.now()}-${Math.random().toString(36).slice(2, 7)}`,
      level,
      message,
      timestamp: new Date().toISOString(),
    }

    notifications.value = [notification, ...notifications.value].slice(0, 50)
    return notification.id
  }

  function dismissNotification(id: string) {
    notifications.value = notifications.value.filter((n) => n.id !== id)
  }

  function clearNotifications() {
    notifications.value = []
  }

  return {
    adminRoutes,
    notifications,
    addNotification,
    dismissNotification,
    clearNotifications,
  }
})
