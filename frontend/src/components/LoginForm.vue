<template>
  <v-card class="mx-auto my-12" max-width="400">
    <v-card-title>Đăng nhập</v-card-title>
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
import { useRouter } from 'vue-router'
import api from '../plugins/axios'

const router = useRouter()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const formRef = ref()

const onSubmit = async () => {
  error.value = ''
  loading.value = true
  try {
    const response = await api.post('/auth/login', {
      email: email.value,
      password: password.value
    })
    // Lưu JWT vào localStorage
    localStorage.setItem('token', response.data.token)
    // Chuyển hướng về trang chủ sau khi đăng nhập thành công
    router.push('/')
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Đăng nhập thất bại'
  } finally {
    loading.value = false
  }
}
</script>
