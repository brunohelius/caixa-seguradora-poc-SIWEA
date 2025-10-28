import path from "path"
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    // Disable caching for CSS during development
    headers: {
      'Cache-Control': 'no-store',
    },
    port: 5174,
  },
  css: {
    devSourcemap: true,
  },
  // T143: Build optimizations
  build: {
    // Minification
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,  // Remove console.log in production
        drop_debugger: true,
      },
    },
    // Code splitting and chunking
    rollupOptions: {
      output: {
        manualChunks: {
          // Vendor chunk for React and core libraries
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          // Recharts chunk (large charting library)
          'recharts': ['recharts'],
          // TanStack Query chunk
          'query': ['@tanstack/react-query'],
          // Axios chunk
          'axios': ['axios'],
        },
      },
    },
    // Chunk size warnings
    chunkSizeWarningLimit: 1000,
    // Source maps (disable in production for smaller builds)
    sourcemap: false,
  },
  // Performance optimizations
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router-dom', 'axios'],
  },
})
