import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'

import App from '@/App.vue'
import HomeView from '@/views/HomeView.vue'

describe('App shell', () => {
  it('renders the home hero content', async () => {
    const router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          component: HomeView,
        },
        {
          path: '/track-order',
          component: HomeView,
        },
        {
          path: '/our-story',
          component: HomeView,
        },
      ],
    })

    router.push('/')
    await router.isReady()

    const wrapper = mount(App, {
      global: {
        plugins: [router],
      },
    })

    expect(wrapper.text()).toContain("Jailey's Trukipan")
    expect(wrapper.text()).toContain("Truk'i pan soul, delivered to your door.")
  })
})
