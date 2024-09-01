import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias:{
      "~": path.resolve(__dirname, "src"),
      "@": path.resolve(__dirname, "src")
    }
  }, 
  server: {
    host: '0.0.0.0',
    port: 5234,
    https: false,
    proxy: {
      '/api': {
        target: 'http://localhost:5079/api',  //这里后台的地址模拟的;应该填写你们真实的后台接口
        changeOrigin: true, //允许跨域
        rewrite: (path) => path.replace(/^\/api/, '') //路径重写，请求的时候使用这个api就可以
      }
    }
  },
})