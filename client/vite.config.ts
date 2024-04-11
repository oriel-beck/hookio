import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { nodePolyfills } from 'vite-plugin-node-polyfills'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [
      react(), 
      nodePolyfills({
        include: ['crypto'],
        protocolImports: true,
      })
    ],
    server: {
      proxy: {
        '/api': {
          target: env.VITE_API_ADDRESS,
          changeOrigin: true,
          secure: false
        }
      },
    }
  }
})

