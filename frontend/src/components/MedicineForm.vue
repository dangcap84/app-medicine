<template>
  <v-card class="mx-auto my-8" max-width="500">
    <v-card-title>{{ isEdit ? 'Chỉnh sửa thuốc' : 'Thêm thuốc mới' }}</v-card-title>
    <v-card-text>
      <v-form @submit.prevent="onSubmit" ref="formRef" v-slot="{ isValid }">
        <v-text-field
          v-model="form.name"
          label="Tên thuốc"
          :rules="[v => !!v || 'Bắt buộc nhập']"
          required
        />
        <v-text-field
          v-model="form.description"
          label="Mô tả"
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
  medicine?: { id: number; name: string; description?: string }
}>()
const emit = defineEmits(['saved'])

const isEdit = ref(!!props.medicine)
const form = ref({
  name: props.medicine?.name || '',
  description: props.medicine?.description || ''
})
const loading = ref(false)
const error = ref('')
const success = ref('')
const formRef = ref()

watch(() => props.medicine, (val) => {
  if (val) {
    form.value.name = val.name
    form.value.description = val.description || ''
    isEdit.value = true
  } else {
    form.value.name = ''
    form.value.description = ''
    isEdit.value = false
  }
})

const onSubmit = async () => {
  error.value = ''
  success.value = ''
  loading.value = true
  try {
    if (isEdit.value && props.medicine) {
      await api.put(`/medicines/${props.medicine.id}`, form.value)
      success.value = 'Cập nhật thành công'
    } else {
      await api.post('/medicines', form.value)
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
