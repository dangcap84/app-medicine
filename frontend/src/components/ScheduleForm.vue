<template>
  <v-card class="mx-auto my-8" max-width="500">
    <v-card-title>{{ isEdit ? 'Chỉnh sửa lịch' : 'Thêm lịch mới' }}</v-card-title>
    <v-card-text>
      <v-form @submit.prevent="onSubmit" ref="formRef" v-slot="{ isValid }">
        <v-text-field
          v-model="form.medicineName"
          label="Tên thuốc"
          :rules="[v => !!v || 'Bắt buộc nhập']"
          required
        />
        <v-text-field
          v-model="form.time"
          label="Thời gian uống (ví dụ: 08:00)"
          :rules="[v => !!v || 'Bắt buộc nhập']"
          required
        />
        <v-select
          v-model="form.frequencyType"
          :items="frequencyTypes"
          label="Tần suất"
          :rules="[v => !!v || 'Bắt buộc chọn']"
          required
        />
        <v-text-field
          v-model="form.notes"
          label="Ghi chú"
        />
        <v-btn
          :disabled="loading || !isValid"
          type="submit"
          color="primary"
          class="mt-4"
          block
        >
          {{ isEdit ? 'Cập nhật' : 'Thêm mới' }}
        </v-btn>
        <v-alert v-if="error" type="error" class="mt-2" dense>{{ error }}</v-alert>
        <v-alert v-if="success" type="success" class="mt-2" dense>{{ success }}</v-alert>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, defineProps, defineEmits } from 'vue'
import api from '../plugins/axios'

const props = defineProps<{
  schedule?: { id: number; medicineName: string; time: string; frequencyType: string; notes?: string }
}>()
const emit = defineEmits(['saved', 'close'])

const isEdit = ref(!!props.schedule)
const form = ref({
  medicineName: props.schedule?.medicineName || '',
  time: props.schedule?.time || '',
  frequencyType: props.schedule?.frequencyType || '',
  notes: props.schedule?.notes || ''
})
const loading = ref(false)
const error = ref('')
const success = ref('')
const formRef = ref()

const frequencyTypes = [
  'Hàng ngày',
  'Cách ngày',
  'Theo tuần',
  'Theo tháng',
  'Khác'
]

watch(() => props.schedule, (val) => {
  if (val) {
    form.value.medicineName = val.medicineName
    form.value.time = val.time
    form.value.frequencyType = val.frequencyType
    form.value.notes = val.notes || ''
    isEdit.value = true
  } else {
    form.value.medicineName = ''
    form.value.time = ''
    form.value.frequencyType = ''
    form.value.notes = ''
    isEdit.value = false
  }
})

const onSubmit = async () => {
  error.value = ''
  success.value = ''
  loading.value = true
  try {
    if (isEdit.value && props.schedule) {
      await api.put(`/schedules/${props.schedule.id}`, form.value)
      success.value = 'Cập nhật thành công'
    } else {
      await api.post('/schedules', form.value)
      success.value = 'Thêm mới thành công'
    }
    emit('saved')
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Lỗi thao tác'
  } finally {
    loading.value = false
  }
}
</script>
