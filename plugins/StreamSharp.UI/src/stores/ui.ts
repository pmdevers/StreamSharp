import { defineStore } from 'pinia'

export const useUiStore = defineStore('ui', {
  state: () => ({
    menuOpen: false,
  }),
  actions: {
    openMenu() {
      this.menuOpen = true
    },
    closeMenu() {
      this.menuOpen = false
    },
  },
})