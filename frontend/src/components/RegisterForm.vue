<template>
  <v-card class="mx-auto my-12" max-width="400">
    <v-card-title>Đăng ký</v-card-title>
    <v-card-text>
      <v-form @submit.prevent="onSubmit" ref="formRef" v-slot="{ isValid }">
        <v-text-field
          v-model="email"
          label="Email"
          type="email"
          prepend-inner-icon="mdi-email"
          :rules="[v => !!v || 'Bắt buộc nhập', v => /.+@.+\..+/.test(v) || 'Email không hợp lệ']"
          required
        />
        <v-text-field
          v-model="password"
          label="Mật khẩu"
          type="password"
          prepend-inner-icon="mdi-lock"
          :rules="[v => !!v || 'Bắt buộc nhập']"
          required
        />
        <v-text-field
          v-model="confirmPassword"
          label="Nhập lại mật khẩu"
          type="password"
          prepend-inner-icon="mdi-lock-check"
          :rules="[v => v === password || 'Mật khẩu không khớp']"
          required
        />
        <v-btn
          :disabled="loading || !isValid"
          type="submit"
          color="primary"
          class="mt-4"
          block
        >
          Đăng ký
        </v-btn>
        <v-alert v-if="error" type="error" class="mt-2" dense>{{ error }}</v-alert>
        <v-alert v-if="success" type="success" class="mt-2" dense>{{ success }}</v-alert>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import api from '../plugins/axios' // Sử dụng đường dẫn tương đối

const email = ref('') // Thay username bằng email
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const error = ref('')
const success = ref('')
const formRef = ref()

const onSubmit = async () => {
  error.value = ''
  success.value = ''
  if (password.value !== confirmPassword.value) {
    error.value = 'Mật khẩu không khớp'
    return
  }
  loading.value = true
  try {
    // Sử dụng instance 'api' đã cấu hình
    const response = await api.post('/auth/register', { // Bỏ '/api' vì đã có trong baseURL của instance 'api'
      email: email.value, // Gửi email thay vì username
      password: password.value
    })
    // Lưu JWT vào localStorage
    localStorage.setItem('token', response.data.token)
    success.value = 'Đăng ký thành công!'
    // TODO: chuyển hướng sang trang chính
  } catch (e: any) {
    if (e.response?.status === 409) {
      error.value = e.response.data.message
    } else {
      error.value = e.response?.data?.message || 'Đăng ký thất bại'
    }
  } finally {
    loading.value = false
  }
}
</script>
