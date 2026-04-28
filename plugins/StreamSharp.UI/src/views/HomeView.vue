<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'

import LibraryHorizontalRow from '@/components/LibraryHorizontalRow.vue'
import { useHomeStore } from '@/stores/home'
import type { LibraryItem } from '@/stores/mediaLibrary'
import { useMediaLibraryStore } from '@/stores/mediaLibrary'

const homeStore = useHomeStore()
const mediaLibraryStore = useMediaLibraryStore()
const route = useRoute()
const router = useRouter()
const { selectedItem } = storeToRefs(homeStore)
const { libraries, resolvedRows } = storeToRefs(mediaLibraryStore)

const fallbackLibraryId = computed(() => libraries.value[0]?.id ?? '')
const currentLibraryId = computed(() => String(route.params.libraryId ?? fallbackLibraryId.value))
const hasValidLibrary = computed(() => libraries.value.some((library) => library.id === currentLibraryId.value))

const visibleRows = computed(() =>
	resolvedRows.value.filter((row) => row.id.startsWith(`${currentLibraryId.value}::`)),
)

watch(
	currentLibraryId,
	(libraryId) => {
		if (libraryId) {
			void mediaLibraryStore.loadItemsForLibrary(libraryId)
		}
	},
	{ immediate: true },
)

watch(
	[() => route.params.libraryId, fallbackLibraryId],
	() => {
		if (!fallbackLibraryId.value) {
			return
		}

		if (!hasValidLibrary.value) {
			router.replace({ name: 'library', params: { libraryId: fallbackLibraryId.value } })
		}
	},
	{ immediate: true },
)

watch(
	visibleRows,
	(rows) => {
		const currentItems = rows.flatMap((row) => row.items)
		const hasSelectedInLibrary = currentItems.some((item) => item.id === selectedItem.value?.id)

		if (!hasSelectedInLibrary && currentItems.length > 0) {
			homeStore.selectItem(currentItems[0])
		}
	},
	{ immediate: true },
)

function handleSelectItem(item: LibraryItem) {
	homeStore.selectItem(item)
}
</script>

<template>
	<div class="space-y-8">
		<LibraryHorizontalRow
			v-for="row in visibleRows"
			:key="row.id"
			:title="row.title"
			:description="row.description"
			:items="row.items"
			:selected-item-id="selectedItem?.id"
			@select-item="handleSelectItem"
		/>
	</div>
</template>