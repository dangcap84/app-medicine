import axios from 'axios'

// Tạo instance riêng cho ứng dụng
const api = axios.create({
  baseURL: '/api'
})

// Interceptor: tự động đính kèm JWT vào header Authorization nếu có
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers = config.headers || {}
      config.headers['Authorization'] = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

export default api
