import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: { // Thêm cấu hình server
    proxy: {
      // Chuỗi '/api' là tiền tố của request bạn muốn proxy
      '/api': {
        target: 'https://localhost:44382', // URL của backend API (đã cập nhật)
        changeOrigin: true, // Cần thiết cho virtual hosted sites
        secure: false, // Thêm dòng này để bỏ qua lỗi self-signed certificate
        // rewrite: (path) => path.replace(/^\/api/, '') // Không cần rewrite vì backend đã có /api
      }
    }
  }
})
