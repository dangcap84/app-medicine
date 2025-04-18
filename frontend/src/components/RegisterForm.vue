<template>
  <v-card class="mx-auto my-12" max-width="400">
    <v-card-title>Đăng ký</v-card-title>
    <v-card-text>
      <v-form @submit.prevent="onSubmit" ref="formRef" v-slot="{ isValid }">
        <v-text-field
          v-model="username"
          label="Tên đăng nhập"
          prepend-inner-icon="mdi-account"
          :rules="[v => !!v || 'Bắt buộc nhập']"
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
import axios from 'axios'

const username = ref('')
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
    const response = await axios.post('/api/auth/register', {
      username: username.value,
      password: password.value
    })
    // Lưu JWT vào localStorage
    localStorage.setItem('token', response.data.token)
    success.value = 'Đăng ký thành công!'
    // TODO: chuyển hướng sang trang chính
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Đăng ký thất bại'
  } finally {
    loading.value = false
  }
}
</script>
