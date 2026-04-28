import { defineStore } from 'pinia'

import type { LibraryItem } from '@/stores/mediaLibrary'

export type { LibraryItem } from '@/stores/mediaLibrary'

export const useHomeStore = defineStore('home', {
  state: () => ({
    selectedItem: null as LibraryItem | null,
  }),
  actions: {
    selectItem(item: LibraryItem | null) {
      this.selectedItem = item
    },
  },
})