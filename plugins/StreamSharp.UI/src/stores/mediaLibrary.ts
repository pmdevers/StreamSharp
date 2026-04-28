import { defineStore } from 'pinia'

import { apiClient } from '@/api/client'
import librariesData from '@/data/libraries.json'

type ApiQuery = Record<string, string | number | boolean | undefined>

type ApiClient = {
  GET: (path: string, options?: { params?: { query?: ApiQuery } }) => Promise<{ data?: unknown; error?: unknown }>
  POST: (
    path: string,
    options?: {
      body?: unknown
      params?: { query?: ApiQuery }
    },
  ) => Promise<{ data?: unknown; error?: unknown }>
}

export type LibraryItem = {
  id: string
  libraryId: string
  genre: string
  title: string
  subtitle: string
  meta: string
  overview?: string
  tag?: string
  progress?: number
  art: string
}

export type Library = {
  id: string
  title: string
  description: string
  createdAt?: string
}

export type LibraryRow = {
  id: string
  title: string
  description: string
  items: LibraryItem[]
}

type LoadLibrariesOptions = {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
}

type LoadLibraryItemsOptions = LoadLibrariesOptions

type MediaLibraryResponseItem = {
  id?: unknown
  title?: unknown
  name?: unknown
  description?: unknown
  createdAt?: unknown
}

type MediaLibraryItemResponseItem = {
  id?: unknown
  libraryId?: unknown
  genre?: unknown
  type?: unknown
  title?: unknown
  name?: unknown
  subtitle?: unknown
  meta?: unknown
  overview?: unknown
  tag?: unknown
  progress?: unknown
  art?: unknown
  createdAt?: unknown
}

const fallbackLibraries = librariesData as Library[]

function toErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message) {
    return error.message
  }

  if (typeof error === 'string' && error.length > 0) {
    return error
  }

  return fallback
}

function toDisplayDate(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return 'Recently added'
  }

  return new Intl.DateTimeFormat('en', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(date)
}

function toArtSeed(value: string): number {
  let hash = 0

  for (let index = 0; index < value.length; index += 1) {
    hash = (hash * 31 + value.charCodeAt(index)) % 360
  }

  return hash
}

function toFallbackArt(seedSource: string): string {
  const hue = toArtSeed(seedSource)
  const accentHue = (hue + 42) % 360

  return `linear-gradient(135deg, hsla(${hue}, 58%, 18%, 0.96), hsla(${accentHue}, 72%, 46%, 0.72))`
}

function toLibraryItem(raw: MediaLibraryItemResponseItem, libraryId: string): LibraryItem | null {
  const id = typeof raw.id === 'string' ? raw.id : null
  const title = typeof raw.title === 'string'
    ? raw.title
    : typeof raw.name === 'string'
      ? raw.name
      : null

  if (!id || id.length === 0 || !title) {
    return null
  }

  const itemType = typeof raw.genre === 'string'
    ? raw.genre
    : typeof raw.type === 'string'
      ? raw.type
      : 'Library item'

  const createdAt = typeof raw.createdAt === 'string' ? raw.createdAt : null
  const defaultMeta = createdAt ? `Added ${toDisplayDate(createdAt)}` : itemType

  return {
    id,
    libraryId: typeof raw.libraryId === 'string' ? raw.libraryId : libraryId,
    genre: itemType,
    title,
    subtitle: typeof raw.subtitle === 'string' ? raw.subtitle : itemType,
    meta: typeof raw.meta === 'string' ? raw.meta : defaultMeta,
    overview: typeof raw.overview === 'string' ? raw.overview : undefined,
    tag: typeof raw.tag === 'string' ? raw.tag : undefined,
    progress: typeof raw.progress === 'number' ? raw.progress : undefined,
    art: typeof raw.art === 'string' ? raw.art : toFallbackArt(`${libraryId}:${itemType}:${title}`),
  }
}

function toLibrary(item: MediaLibraryResponseItem): Library | null {
  if (typeof item.id !== 'string' || item.id.length === 0) {
    return null
  }

  const title = typeof item.title === 'string'
    ? item.title
    : typeof item.name === 'string'
      ? item.name
      : 'Untitled library'

  const description = typeof item.description === 'string'
    ? item.description
    : `Browse ${title}`

  return {
    id: item.id,
    title,
    description,
    createdAt: typeof item.createdAt === 'string' ? item.createdAt : undefined,
  }
}

function toQuery(options?: LoadLibrariesOptions): ApiQuery | undefined {
  if (!options) {
    return undefined
  }

  return {
    page: options.page,
    pageSize: options.pageSize,
    search: options.search,
    sortBy: options.sortBy,
  }
}

export const useMediaLibraryStore = defineStore('media-library', {
  state: () => ({
    libraries: fallbackLibraries,
    libraryItems: [] as LibraryItem[],
    libraryDetails: {} as Record<string, Library | null | undefined>,
    isLoadingLibraries: false,
    isLoadingLibraryById: false,
    isCreatingLibrary: false,
    isScanningLibrary: false,
    loadingItemsByLibrary: {} as Record<string, boolean>,
    error: null as string | null,
  }),
  getters: {
    resolvedRows(state): LibraryRow[] {
      const libraryMap = Object.fromEntries(state.libraries.map((library) => [library.id, library]))
      const rowMap = new Map<string, LibraryItem[]>()

      for (const item of state.libraryItems) {
        const key = `${item.libraryId}::${item.genre}`
        if (!rowMap.has(key)) {
          rowMap.set(key, [])
        }
        rowMap.get(key)!.push(item)
      }

      const rows: LibraryRow[] = []
      for (const [key, items] of rowMap) {
        const [libraryId, genre] = key.split('::')
        const library = libraryMap[libraryId]
        if (!library) {
          continue
        }

        rows.push({
          id: key,
          title: genre,
          description: `${genre} titles from ${library.title}`,
          items,
        })
      }

      return rows
    },
    rowsForLibrary: (state) => {
      const rowMap = new Map<string, LibraryRow[]>()
      const libraryMap = Object.fromEntries(state.libraries.map((library) => [library.id, library]))

      for (const item of state.libraryItems) {
        const library = libraryMap[item.libraryId]
        if (!library) {
          continue
        }

        const rows = rowMap.get(item.libraryId) ?? []
        const rowId = `${item.libraryId}::${item.genre}`
        const existingRow = rows.find((row) => row.id === rowId)

        if (existingRow) {
          existingRow.items.push(item)
        } else {
          rows.push({
            id: rowId,
            title: item.genre,
            description: `${item.genre} titles from ${library.title}`,
            items: [item],
          })
          rowMap.set(item.libraryId, rows)
        }
      }

      return (libraryId: string) => rowMap.get(libraryId) ?? []
    },
  },
  actions: {
    clearError() {
      this.error = null
    },
    async loadLibraries(options?: LoadLibrariesOptions) {
      this.isLoadingLibraries = true
      this.error = null

      const client = apiClient as ApiClient
      const { data, error } = await client.GET('/medialibrary', {
        params: { query: toQuery(options) },
      })

      this.isLoadingLibraries = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to load libraries.')
        return
      }

      if (!Array.isArray(data)) {
        this.error = 'Unexpected media library response.'
        return
      }

      const mappedLibraries = data
        .map((item) => toLibrary(item as MediaLibraryResponseItem))
        .filter((library): library is Library => library !== null)

      if (mappedLibraries.length > 0) {
        this.libraries = mappedLibraries
      }
    },
    async loadLibraryById(libraryId: string) {
      this.isLoadingLibraryById = true
      this.error = null

      const client = apiClient as ApiClient
      const { data, error } = await client.GET(`/medialibrary/${libraryId}`)

      this.isLoadingLibraryById = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to load library.')
        return null
      }

      const mappedLibrary = toLibrary((data ?? null) as MediaLibraryResponseItem)
      this.libraryDetails[libraryId] = mappedLibrary

      if (mappedLibrary) {
        const existingIndex = this.libraries.findIndex((library) => library.id === mappedLibrary.id)
        if (existingIndex >= 0) {
          this.libraries.splice(existingIndex, 1, mappedLibrary)
        } else {
          this.libraries.push(mappedLibrary)
        }
      }

      return mappedLibrary
    },
    async createLibrary(name: string) {
      this.isCreatingLibrary = true
      this.error = null

      const client = apiClient as ApiClient
      const { data, error } = await client.POST('/medialibrary', {
        body: { name },
      })

      this.isCreatingLibrary = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to create library.')
        return null
      }

      const mappedLibrary = toLibrary((data ?? null) as MediaLibraryResponseItem)
      if (mappedLibrary && !this.libraries.some((library) => library.id === mappedLibrary.id)) {
        this.libraries.push(mappedLibrary)
      }

      return mappedLibrary
    },
    async loadItemsForLibrary(libraryId: string, options?: LoadLibraryItemsOptions) {
      this.loadingItemsByLibrary = {
        ...this.loadingItemsByLibrary,
        [libraryId]: true,
      }
      this.error = null

      const client = apiClient as ApiClient
      const { data, error } = await client.GET(`/medialibrary/${libraryId}/items`, {
        params: { query: toQuery(options) },
      })

      this.loadingItemsByLibrary = {
        ...this.loadingItemsByLibrary,
        [libraryId]: false,
      }

      if (error) {
        this.error = toErrorMessage(error, 'Failed to load library items.')
        return
      }

      if (!Array.isArray(data)) {
        this.error = 'Unexpected media library items response.'
        return
      }

      const newItems = data
        .map((item) => toLibraryItem(item as MediaLibraryItemResponseItem, libraryId))
        .filter((item): item is LibraryItem => item !== null)

      this.libraryItems = [
        ...this.libraryItems.filter((item) => item.libraryId !== libraryId),
        ...newItems,
      ]
    },
    async scanLibrary(libraryId: string) {
      this.isScanningLibrary = true
      this.error = null

      const client = apiClient as ApiClient
      const { error } = await client.POST(`/medialibrary/${libraryId}/scan`)

      this.isScanningLibrary = false

      if (error) {
        this.error = toErrorMessage(error, 'Failed to scan library.')
        return false
      }

      return true
    },
  },
})