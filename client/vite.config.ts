import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react-swc'
import { compression } from 'vite-plugin-compression2'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [
      react(),
      compression({ algorithm: "brotliCompress" })
    ],
    server: {
      proxy: {
        '/api': {
          target: env.VITE_API_ADDRESS,
          changeOrigin: true,
          secure: false
        }
      },
    },
  }
})

