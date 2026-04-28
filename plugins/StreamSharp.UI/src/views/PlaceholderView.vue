<script setup lang="ts">
interface Props {
  title: string
  eyebrow: string
  description: string
}

defineProps<Props>()
</script>

<template>
  <section class="mx-auto max-w-5xl px-4 py-20 sm:px-6 lg:px-8 lg:py-28">
    <div class="soft-card">
      <p class="text-sm font-semibold uppercase tracking-[0.35em] text-secondary">{{ eyebrow }}</p>
      <h1 class="mt-5 text-4xl font-black text-textPrimary sm:text-5xl">{{ title }}</h1>
      <p class="mt-5 max-w-2xl text-lg leading-8 text-textSecondary">
        {{ description }}
      </p>
      <div class="mt-10 rounded-[12px] border border-dashed border-[#ead8c9] bg-background p-6 text-sm text-textSecondary sm:p-8">
        This page is ready for the next prompt. The shared navigation and brand styling are already in place.
      </div>
    </div>
  </section>
</template>
