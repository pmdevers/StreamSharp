<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <footer class="border-t border-white/10 bg-backgroundDark text-white">
    <div class="mx-auto grid max-w-7xl gap-10 px-4 py-12 sm:px-6 lg:grid-cols-[1.4fr,1fr,1fr] lg:px-8">
      <div class="space-y-4">
        <div class="flex items-center gap-3">
          <div class="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary text-lg font-black text-backgroundDark shadow-warm">
            J
          </div>
          <div>
            <p class="text-sm font-semibold text-white">Jailey's Trukipan</p>
            <p class="text-sm text-white/65">Warm Curaçao comfort, straight to your door.</p>
          </div>
        </div>
        <p class="max-w-md text-sm text-white/70">
          A delivery experience inspired by truk'i pan nights — grilled favorites, pastechi, batidos, and island flavor.
        </p>
      </div>

      <div>
        <p class="text-sm font-semibold uppercase tracking-[0.25em] text-white/55">Explore</p>
        <div class="mt-4 flex flex-col gap-3 text-sm text-white/75">
          <RouterLink to="/" class="transition hover:text-white">Home</RouterLink>
          <RouterLink to="/track-order" class="transition hover:text-white">Track order</RouterLink>
          <RouterLink to="/our-story" class="transition hover:text-white">Our story</RouterLink>
        </div>
      </div>

      <div>
        <p class="text-sm font-semibold uppercase tracking-[0.25em] text-white/55">Delivery hours</p>
        <div class="mt-4 space-y-3 text-sm text-white/75">
          <p>Mon - Thu · 4:00 PM - 11:00 PM</p>
          <p>Fri - Sat · 4:00 PM - 1:30 AM</p>
          <p>Sun · 2:00 PM - 10:00 PM</p>
          <p class="text-white/55">Serving Pietermaai, Otrobanda, Jan Thiel, and nearby neighborhoods.</p>
        </div>
      </div>
    </div>
  </footer>
</template>
