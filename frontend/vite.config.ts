import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'Request Manager',
        short_name: 'Requests',
        description: 'Submit, track, and manage team requests',
        theme_color: '#4f46e5',
        background_color: '#ffffff',
        display: 'standalone',
        icons: []   // ← No icons for now (prevents the error)
      },
      devOptions: {
        enabled: true
      }
    })
  ],
  resolve: {
    alias: {
      '@': '/src'
    }
  }
})