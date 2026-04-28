import { createRouter, createWebHistory } from 'vue-router'

import HomeView from '@/views/HomeView.vue'
import PlaceholderView from '@/views/PlaceholderView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/track-order',
      name: 'track-order',
      component: PlaceholderView,
      props: {
        title: 'Track your order',
        eyebrow: 'Coming next',
        description:
          'Use this route for live rider updates, delivery status, and arrival estimates once you are ready to build the next step of the app.',
      },
    },
    {
      path: '/our-story',
      name: 'our-story',
      component: PlaceholderView,
      props: {
        title: 'The story behind Truki Pan Dushi',
        eyebrow: 'Brand page',
        description:
          'This route can become your story, sourcing, or vendor page while keeping the same warm Curaçao-inspired experience across the app.',
      },
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
