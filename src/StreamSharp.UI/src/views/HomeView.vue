<script setup lang="ts">
import { computed, reactive } from 'vue'

type MenuItem = {
  id: number
  name: string
  description: string
  price: number
  badge: string
  eta: string
  note: string
}

const categories = ['Truki Pan classics', 'Pastechi & sides', 'Family trays', 'Fresh batidos']

const menuItems: MenuItem[] = [
  {
    id: 1,
    name: 'Stobá beef panini',
    description: 'Slow-braised beef, pikliz slaw, gouda melt, and sweet plantain aioli on toasted pan bati.',
    price: 18.5,
    badge: 'Best seller',
    eta: '18-24 min',
    note: 'Medium heat',
  },
  {
    id: 2,
    name: 'Keshi yena fries box',
    description: 'Golden fries loaded with pulled chicken, olives, raisins, edam cheese sauce, and fresh herbs.',
    price: 16,
    badge: 'Street favorite',
    eta: '16-20 min',
    note: 'Mild heat',
  },
  {
    id: 3,
    name: 'Kabritu taco trio',
    description: 'Tender curried goat, pickled onions, tamarind glaze, and crunchy lettuce in grilled corn tortillas.',
    price: 21,
    badge: 'Chef special',
    eta: '22-28 min',
    note: 'Bold heat',
  },
  {
    id: 4,
    name: 'Batido di mango',
    description: 'Icy mango smoothie blended with lime, passionfruit, and condensed milk for a tropical finish.',
    price: 8.5,
    badge: 'Cool down',
    eta: '10-15 min',
    note: 'No heat',
  },
]

const neighborhoods = ['Punda', 'Pietermaai', 'Otrobanda', 'Jan Thiel', 'Saliña', 'Mambo Beach']

const highlights = [
  {
    title: "Authentic truk'i pan flavor",
    text: 'Smoky grills, melty cheese, crisp pastechi, and sweet drinks inspired by Curaçao roadside favorites.',
  },
  {
    title: 'Fast island delivery',
    text: 'Orders are grouped by neighborhood so your food stays hot and arrives with a fresh, street-side feel.',
  },
  {
    title: 'Built for sharing',
    text: 'Perfect for solo cravings, family trays, and late-night hangs after the beach or a night out.',
  },
]

const cart = reactive<Record<number, number>>({
  1: 1,
  2: 1,
})

const deliveryFee = 4.5

const subtotal = computed(() =>
  menuItems.reduce((sum, item) => sum + item.price * (cart[item.id] ?? 0), 0),
)

const totalItems = computed(() =>
  Object.values(cart).reduce((sum, quantity) => sum + quantity, 0),
)

const total = computed(() => subtotal.value + (subtotal.value > 0 ? deliveryFee : 0))

const cartItems = computed(() => menuItems.filter((item) => (cart[item.id] ?? 0) > 0))

function addItem(id: number) {
  cart[id] = (cart[id] ?? 0) + 1
}

function removeItem(id: number) {
  if (!cart[id]) {
    return
  }

  cart[id] -= 1

  if (cart[id] <= 0) {
    delete cart[id]
  }
}
</script>

<template>
  <div class="bg-background text-textPrimary">
    <section class="relative overflow-hidden bg-backgroundDark text-white">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(231,168,75,0.35),transparent_34%),radial-gradient(circle_at_80%_10%,rgba(79,191,138,0.18),transparent_24%),linear-gradient(180deg,rgba(58,47,42,1)_0%,rgba(45,37,33,1)_100%)]"></div>
      <div class="relative mx-auto grid max-w-7xl gap-10 px-4 py-16 sm:px-6 lg:grid-cols-[1.1fr,0.9fr] lg:px-8 lg:py-24">
        <div class="space-y-8">
          <div class="inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-4 py-2 text-sm text-white/85 backdrop-blur">
            <span class="h-2 w-2 rounded-full bg-accent"></span>
            Fresh Curaçao street food, delivered fast
          </div>

          <div class="space-y-6">
            <h1 class="max-w-3xl text-5xl font-black tracking-tight sm:text-6xl lg:text-[4.25rem]">
              Truk'i pan soul, delivered to your door.
            </h1>
            <p class="max-w-2xl text-lg text-white/75 sm:text-xl">
              Order smoky stobá sandwiches, loaded fries, pastechi, and tropical drinks inspired by Curaçao’s iconic roadside food trucks.
            </p>
          </div>

          <div class="flex flex-col gap-4 sm:flex-row">
            <a href="#featured-menu" class="btn-primary">Start your order</a>
            <a href="#how-it-works" class="btn-secondary">How delivery works</a>
          </div>

          <div class="grid gap-4 sm:grid-cols-3">
            <div class="rounded-card border border-white/10 bg-white/10 p-5 backdrop-blur">
              <p class="text-3xl font-black text-white">25 min</p>
              <p class="mt-1 text-sm text-white/70">Average delivery in central Willemstad</p>
            </div>
            <div class="rounded-card border border-white/10 bg-white/10 p-5 backdrop-blur">
              <p class="text-3xl font-black text-white">4.9/5</p>
              <p class="mt-1 text-sm text-white/70">Loved for late-night bites and generous portions</p>
            </div>
            <div class="rounded-card border border-white/10 bg-white/10 p-5 backdrop-blur">
              <p class="text-3xl font-black text-white">6 zones</p>
              <p class="mt-1 text-sm text-white/70">From Punda to Jan Thiel and nearby beaches</p>
            </div>
          </div>
        </div>

        <div class="relative">
          <div class="absolute -left-6 top-8 hidden h-28 w-28 rounded-full bg-primary/30 blur-3xl lg:block"></div>
          <div class="absolute -right-10 bottom-20 hidden h-32 w-32 rounded-full bg-accent/20 blur-3xl lg:block"></div>

          <div class="rounded-[1.75rem] bg-white p-4 shadow-card sm:p-6">
            <div class="rounded-[1.5rem] bg-background p-5 sm:p-6">
              <div class="flex items-center justify-between gap-4">
                <div>
                  <p class="text-sm font-semibold uppercase tracking-[0.3em] text-secondary/80">Tonight’s hot route</p>
                  <h2 class="mt-2 text-2xl font-bold text-textPrimary">Pietermaai to Jan Thiel</h2>
                </div>
                <div class="rounded-full bg-success/15 px-3 py-1 text-sm font-medium text-success">
                  Dispatching now
                </div>
              </div>

              <div class="mt-6 grid gap-4 sm:grid-cols-2">
                <div class="soft-card">
                  <p class="text-sm font-semibold uppercase tracking-[0.25em] text-secondary">Popular combo</p>
                  <p class="mt-3 text-2xl font-black text-textPrimary">Stobá + mango batido</p>
                  <p class="mt-2 text-sm text-textSecondary">
                    Savory beef, melted cheese, and a cold tropical drink — the classic curbside order.
                  </p>
                  <div class="mt-5 inline-flex rounded-full bg-background px-4 py-2 text-sm font-semibold text-textPrimary">
                    Guilder-inspired pricing · $24
                  </div>
                </div>

                <div class="soft-card space-y-4">
                  <div class="flex items-center justify-between text-sm text-textSecondary">
                    <span>Delivery zones</span>
                    <span>{{ neighborhoods.length }} neighborhoods</span>
                  </div>
                  <div class="flex flex-wrap gap-2">
                    <span
                      v-for="neighborhood in neighborhoods"
                      :key="neighborhood"
                      class="rounded-full border border-[#ead8c9] bg-background px-3 py-2 text-sm text-textPrimary"
                    >
                      {{ neighborhood }}
                    </span>
                  </div>
                </div>
              </div>

              <div class="mt-4 rounded-card border border-[#ead8c9] bg-white p-5">
                <div class="flex items-center justify-between text-sm text-textSecondary">
                  <span>Peak order time</span>
                  <span>7:30 PM - 10:00 PM</span>
                </div>
                <div class="mt-4 h-3 overflow-hidden rounded-full bg-background">
                  <div class="h-full w-3/4 rounded-full bg-gradient-to-r from-primary via-secondary to-accent"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="border-y border-[#ead8c9] bg-white">
      <div class="mx-auto flex max-w-7xl flex-wrap gap-3 px-4 py-5 sm:px-6 lg:px-8">
        <span
          v-for="category in categories"
          :key="category"
          class="rounded-full border border-[#ead8c9] bg-background px-4 py-2 text-sm text-textPrimary"
        >
          {{ category }}
        </span>
      </div>
    </section>

    <section id="featured-menu" class="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 lg:py-20">
      <div class="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p class="text-sm font-semibold uppercase tracking-[0.35em] text-secondary">Featured menu</p>
          <h2 class="mt-3 text-3xl font-black text-textPrimary sm:text-4xl">
            Build a delivery order that feels straight off the grill.
          </h2>
        </div>
        <p class="max-w-2xl text-base text-textSecondary">
          Mix Curaçao comfort food with street-side energy: grilled meats, melty cheese, spicy sauces, and icy fruit drinks.
        </p>
      </div>

      <div class="mt-10 grid gap-6 xl:grid-cols-[1.5fr,0.8fr]">
        <div class="grid gap-6 md:grid-cols-2">
          <article
            v-for="item in menuItems"
            :key="item.id"
            class="soft-card group transition hover:-translate-y-1 hover:shadow-[0_10px_24px_rgba(0,0,0,0.12)]"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <p class="inline-flex rounded-full bg-background px-3 py-1 text-xs font-semibold uppercase tracking-[0.25em] text-textSecondary">
                  {{ item.badge }}
                </p>
                <h3 class="mt-4 text-2xl font-bold text-textPrimary">{{ item.name }}</h3>
              </div>
              <p class="text-xl font-black text-secondary">${{ item.price.toFixed(2) }}</p>
            </div>

            <p class="mt-4 text-sm leading-7 text-textSecondary">
              {{ item.description }}
            </p>

            <div class="mt-6 flex flex-wrap gap-2 text-xs font-medium text-textSecondary">
              <span class="rounded-full border border-[#ead8c9] bg-background px-3 py-2">{{ item.eta }}</span>
              <span class="rounded-full border border-[#ead8c9] bg-background px-3 py-2">{{ item.note }}</span>
            </div>

            <div class="mt-8">
              <div v-if="cart[item.id]" class="flex items-center justify-between rounded-card border border-[#ead8c9] bg-background p-2">
                <button
                  type="button"
                  class="flex h-11 w-11 items-center justify-center rounded-[8px] bg-white text-xl font-semibold text-textPrimary transition hover:bg-background"
                  @click="removeItem(item.id)"
                >
                  −
                </button>
                <span class="text-sm font-semibold uppercase tracking-[0.3em] text-textPrimary">
                  {{ cart[item.id] }} in cart
                </span>
                <button
                  type="button"
                  class="flex h-11 w-11 items-center justify-center rounded-[8px] bg-accent text-xl font-semibold text-white transition hover:opacity-90"
                  @click="addItem(item.id)"
                >
                  +
                </button>
              </div>
              <button
                v-else
                type="button"
                class="btn-primary w-full"
                @click="addItem(item.id)"
              >
                Add to order
              </button>
            </div>
          </article>
        </div>

        <aside class="h-fit rounded-[12px] border border-[#ead8c9] bg-white p-6 shadow-card xl:sticky xl:top-28">
          <div class="flex items-start justify-between gap-4">
            <div>
              <p class="text-sm font-semibold uppercase tracking-[0.3em] text-secondary">Your order</p>
              <h3 class="mt-3 text-2xl font-bold text-textPrimary">Cart summary</h3>
            </div>
            <div class="rounded-full bg-background px-4 py-2 text-sm font-semibold text-textPrimary">
              {{ totalItems }} items
            </div>
          </div>

          <div class="mt-8 space-y-4">
            <div
              v-for="item in cartItems"
              :key="item.id"
              class="rounded-card border border-[#ead8c9] bg-background p-4"
            >
              <div class="flex items-start justify-between gap-4">
                <div>
                  <p class="font-semibold text-textPrimary">{{ item.name }}</p>
                  <p class="mt-1 text-sm text-textSecondary">${{ item.price.toFixed(2) }} each</p>
                </div>
                <p class="text-sm font-semibold text-secondary">x{{ cart[item.id] }}</p>
              </div>
              <div class="mt-4 flex items-center gap-3">
                <button
                  type="button"
                  class="flex h-10 w-10 items-center justify-center rounded-[8px] border border-[#ead8c9] bg-white text-lg text-textPrimary transition hover:bg-background"
                  @click="removeItem(item.id)"
                >
                  −
                </button>
                <button
                  type="button"
                  class="flex h-10 w-10 items-center justify-center rounded-[8px] bg-accent text-lg font-semibold text-white transition hover:opacity-90"
                  @click="addItem(item.id)"
                >
                  +
                </button>
              </div>
            </div>

            <div v-if="cartItems.length === 0" class="rounded-card border border-dashed border-[#ead8c9] bg-background p-6 text-sm text-textSecondary">
              Add a few Curaçao favorites to see your delivery total and estimated arrival time.
            </div>
          </div>

          <div class="mt-8 space-y-3 border-t border-[#ead8c9] pt-6 text-sm text-textSecondary">
            <div class="flex items-center justify-between">
              <span>Subtotal</span>
              <span>${{ subtotal.toFixed(2) }}</span>
            </div>
            <div class="flex items-center justify-between">
              <span>Delivery fee</span>
              <span>${{ subtotal > 0 ? deliveryFee.toFixed(2) : '0.00' }}</span>
            </div>
            <div class="flex items-center justify-between text-lg font-semibold text-textPrimary">
              <span>Total</span>
              <span>${{ total.toFixed(2) }}</span>
            </div>
          </div>

          <button type="button" class="btn-primary mt-8 w-full">
            Checkout for delivery
          </button>
          <p class="mt-3 text-center text-xs uppercase tracking-[0.3em] text-textSecondary">
            Live ETA after address confirmation
          </p>
        </aside>
      </div>
    </section>

    <section id="how-it-works" class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8 lg:py-10">
      <div class="grid gap-6 md:grid-cols-3">
        <article
          v-for="(highlight, index) in highlights"
          :key="highlight.title"
          class="rounded-card border border-[#ead8c9] bg-white p-6 shadow-card"
        >
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-background text-lg font-black text-secondary">
            0{{ index + 1 }}
          </div>
          <h3 class="mt-5 text-xl font-bold text-textPrimary">{{ highlight.title }}</h3>
          <p class="mt-3 text-sm leading-7 text-textSecondary">{{ highlight.text }}</p>
        </article>
      </div>
    </section>
  </div>
</template>
