import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    allowedHosts: ['impressively-confident-puffin.cloudpub.ru'],
    watch: {
      usePolling: true,
    },
    host: true,
    strictPort: true,
    port: 4001
  }
})
