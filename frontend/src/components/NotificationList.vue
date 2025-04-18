<template>
  <v-card class="mx-auto my-12" max-width="600">
    <v-card-title>Thông báo</v-card-title>
    <v-card-text>
      <v-btn color="secondary" class="mb-4" @click="fetchNotifications" :loading="loading">
        Tải lại
      </v-btn>
      <v-alert v-if="error" type="error" dense>{{ error }}</v-alert>
      <v-list v-if="notifications.length">
        <v-list-item v-for="noti in notifications" :key="noti.id">
          <v-list-item-content>
            <v-list-item-title>{{ noti.title }}</v-list-item-title>
            <v-list-item-subtitle>{{ noti.message }}</v-list-item-subtitle>
            <v-list-item-subtitle>
              <span v-if="noti.read">Đã đọc</span>
              <span v-else>Chưa đọc</span>
            </v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-btn v-if="!noti.read" color="primary" @click="markAsRead(noti.id)">Đánh dấu đã đọc</v-btn>
          </v-list-item-action>
        </v-list-item>
      </v-list>
      <div v-else-if="!loading">Không có thông báo nào.</div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '../plugins/axios'

interface Notification {
  id: number
  title: string
  message: string
  read: boolean
}

const notifications = ref<Notification[]>([])
const loading = ref(false)
const error = ref('')

const fetchNotifications = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get('/notifications')
    notifications.value = res.data
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể tải thông báo'
  } finally {
    loading.value = false
  }
}

const markAsRead = async (id: number) => {
  loading.value = true
  error.value = ''
  try {
    await api.put(`/notifications/${id}`, { read: true })
    const noti = notifications.value.find(n => n.id === id)
    if (noti) noti.read = true
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể cập nhật trạng thái'
  } finally {
    loading.value = false
  }
}

onMounted(fetchNotifications)
</script>
