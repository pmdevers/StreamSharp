import { createRouter, createWebHistory } from 'vue-router'

import AdminView from '@/views/AdminView.vue'
import HomeView from '@/views/HomeView.vue'
import MediaLibraryView from '@/views/MediaLibraryView.vue'
import PluginsView from '@/views/PluginsView.vue'

declare module 'vue-router' {
  interface RouteMeta {
    admin?: boolean
    label?: string
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: { name: 'library', params: { libraryId: 'c1a6e3d2-7f4b-4a8e-b5c9-12f3a8e6d041' } },
    },
    {
      path: '/library/:libraryId',
      name: 'library',
      component: HomeView,
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { admin: true, label: 'Status' },
    },
    {
      path: '/admin/libraries',
      name: 'admin-libraries',
      component: MediaLibraryView,
      meta: { admin: true, label: 'Libraries' },
    },
    {
      path: '/admin/plugins',
      name: 'admin-plugins',
      component: PluginsView,
      meta: { admin: true, label: 'Plugins' },
    },
  ],
  scrollBehavior(to) {
    if (to.hash) {
      return {
        el: to.hash,
        behavior: 'smooth',
      }
    }

    return { top: 0, behavior: 'smooth' }
  },
})

export default router
