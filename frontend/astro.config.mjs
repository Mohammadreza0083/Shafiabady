import { defineConfig } from 'astro/config';
import node from '@astrojs/node';
import vercel from '@astrojs/vercel';
import tailwind from '@astrojs/tailwind';
import sitemap from '@astrojs/sitemap';

const isVercel = process.env.VERCEL === '1' || Boolean(process.env.VERCEL);

// https://astro.build/config
export default defineConfig({
  site: 'https://shafieiabadi.ir',
  // In Astro 5, output: 'server' enables on-demand SSR by default for dynamic catalog & admin routes
  output: 'server',
  adapter: isVercel
    ? vercel({
        webAnalytics: { enabled: false },
        edgeMiddleware: false,
      })
    : node({
        mode: 'standalone',
      }),
  integrations: [
    tailwind({
      applyBaseStyles: false,
    }),
    sitemap({
      filter: (page) => !page.includes('/admin'),
    }),
  ],
  server: {
    port: 4500,
    host: true,
  },
});
