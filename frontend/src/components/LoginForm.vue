<template>
  <v-card class="mx-auto my-12" max-width="400">
    <v-card-title>Đăng nhập</v-card-title>
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
        <v-btn
          :disabled="loading || !isValid"
          type="submit"
          color="primary"
          class="mt-4"
          block
        >
          Đăng nhập
        </v-btn>
        <v-alert v-if="error" type="error" class="mt-2" dense>{{ error }}</v-alert>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'

const username = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const formRef = ref()

const onSubmit = async () => {
  error.value = ''
  loading.value = true
  try {
    const response = await axios.post('/api/auth/login', {
      username: username.value,
      password: password.value
    })
    // Lưu JWT vào localStorage
    localStorage.setItem('token', response.data.token)
    // TODO: chuyển hướng sang trang chính
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Đăng nhập thất bại'
  } finally {
    loading.value = false
  }
}
</script>
