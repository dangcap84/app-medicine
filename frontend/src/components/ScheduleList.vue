<template>
  <v-card class="mx-auto my-12" max-width="700">
    <v-card-title>Quản lý lịch uống thuốc</v-card-title>
    <v-card-text>
      <v-btn color="primary" class="mb-4" @click="openAdd" :disabled="loading">
        Thêm lịch mới
      </v-btn>
      <v-btn color="secondary" class="mb-4 ml-2" @click="fetchSchedules" :loading="loading">
        Tải lại
      </v-btn>
      <v-alert v-if="error" type="error" dense>{{ error }}</v-alert>
      <v-list v-if="schedules.length">
        <v-list-item v-for="schedule in schedules" :key="schedule.id">
          <v-list-item-content>
            <v-list-item-title>
              {{ schedule.medicineName }} - {{ schedule.time }}
            </v-list-item-title>
            <v-list-item-subtitle>
              {{ schedule.frequencyType }} | {{ schedule.notes }}
            </v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-btn icon="mdi-pencil" @click="openEdit(schedule)" />
            <v-btn icon="mdi-delete" color="error" @click="deleteSchedule(schedule.id)" />
          </v-list-item-action>
        </v-list-item>
      </v-list>
      <div v-else-if="!loading">Không có lịch nào.</div>
      <schedule-form
        v-if="showForm"
        :schedule="selectedSchedule"
        @saved="onSaved"
        @close="closeForm"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '../plugins/axios'
import ScheduleForm from './ScheduleForm.vue'

interface Schedule {
  id: number
  medicineName: string
  time: string
  frequencyType: string
  notes?: string
}

const schedules = ref<Schedule[]>([])
const loading = ref(false)
const error = ref('')
const showForm = ref(false)
const selectedSchedule = ref<Schedule | undefined>(undefined)

const fetchSchedules = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get('/schedules')
    schedules.value = res.data
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể tải danh sách lịch'
  } finally {
    loading.value = false
  }
}

const openAdd = () => {
  selectedSchedule.value = undefined
  showForm.value = true
}
const openEdit = (schedule: Schedule) => {
  selectedSchedule.value = { ...schedule }
  showForm.value = true
}
const closeForm = () => {
  showForm.value = false
}
const onSaved = () => {
  showForm.value = false
  fetchSchedules()
}
const deleteSchedule = async (id: number) => {
  if (!confirm('Bạn có chắc muốn xóa lịch này?')) return
  loading.value = true
  error.value = ''
  try {
    await api.delete(`/schedules/${id}`)
    schedules.value = schedules.value.filter(s => s.id !== id)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể xóa lịch'
  } finally {
    loading.value = false
  }
}

onMounted(fetchSchedules)
</script>
