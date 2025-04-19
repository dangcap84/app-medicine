<template>
  <v-card class="mx-auto my-8" max-width="500">
    <v-card-title>{{ isEdit ? 'Chỉnh sửa thuốc' : 'Thêm thuốc mới' }}</v-card-title>
    <v-card-text>
      <v-form @submit.prevent="onSubmit" ref="formRef" v-slot="{ isValid }">
        <div class="form-grid">
          <v-text-field
            v-model="form.name"
            label="Tên thuốc"
            :rules="[v => !!v || 'Bắt buộc nhập']"
            required
          />
          <v-text-field
            v-model="form.dosage"
            label="Liều lượng"
            :rules="[v => !!v || 'Bắt buộc nhập']"
            required
          />
          <v-select
            v-model="form.medicineUnitId"
            label="Đơn vị"
            :items="medicineUnits"
            item-title="name"
            item-value="id"
            :rules="[v => !!v || 'Bắt buộc chọn']"
            required
            class="custom-combobox"
            density="comfortable"
            rounded="lg"
          />
          <v-text-field
            v-model="form.description"
            label="Mô tả"
          />
        </div>
        <div class="form-actions">
          <v-btn
            :disabled="loading || !isValid"
            type="submit"
            color="primary"
            class="mt-4"
          >
            {{ isEdit ? 'Cập nhật' : 'Thêm mới' }}
          </v-btn>
        </div>
        <v-alert v-if="error" type="error" class="mt-2" dense>{{ error }}</v-alert>
        <v-alert v-if="success" type="success" class="mt-2" dense>{{ success }}</v-alert>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, defineProps, defineEmits, onMounted } from 'vue'
import api from '../plugins/axios'

interface Medicine {
  id: number
  name: string
  description?: string | null
  dosage?: string | null
  medicineUnitId?: string | null | undefined
}

const props = defineProps<{
  medicine?: Medicine
}>()
const emit = defineEmits(['saved'])

const isEdit = ref(!!props.medicine)
const form = ref({
  name: props.medicine?.name || '',
  description: props.medicine?.description || '',
  dosage: props.medicine?.dosage || '',
  medicineUnitId: props.medicine?.medicineUnitId === undefined ? '' : props.medicine.medicineUnitId
})
const loading = ref(false)
const error = ref('')
const success = ref('')
const formRef = ref()
const medicineUnits = ref<Array<{ id: string; name: string }>>([])

watch(() => props.medicine, (val) => {
  if (val) {
    form.value.name = val.name
    form.value.description = val.description || ''
    form.value.dosage = val.dosage || ''
    form.value.medicineUnitId = val.medicineUnitId === undefined ? '' : val.medicineUnitId
    isEdit.value = true
  } else {
    form.value.name = ''
    form.value.description = ''
    form.value.dosage = ''
    form.value.medicineUnitId = ''
    isEdit.value = false
  }
})

const loadMedicineUnits = async () => {
  try {
    const response = await api.get('/medicine-units')
    medicineUnits.value = response.data
  } catch (e: any) {
    error.value = 'Không thể tải danh sách đơn vị thuốc'
  }
}

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

onMounted(() => {
  loadMedicineUnits()
})
</script>

<style scoped>
.custom-combobox .v-input__control {
  min-height: 52px !important;
  border-radius: 12px !important;
  font-size: 1.08rem;
  padding-left: 8px;
}
.custom-combobox .v-field__input {
  padding-top: 14px !important;
  padding-bottom: 14px !important;
}
.custom-combobox .v-field {
  border-radius: 12px !important;
  box-shadow: 0 2px 8px #1976d21a;
}
</style>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 24px;
}
.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 8px;
}
@media (max-width: 600px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
.custom-combobox .v-input__control {
  min-height: 52px !important;
  border-radius: 12px !important;
  font-size: 1.08rem;
  padding-left: 8px;
}
.custom-combobox .v-field__input {
  padding-top: 14px !important;
  padding-bottom: 14px !important;
}
.custom-combobox .v-field {
  border-radius: 12px !important;
  box-shadow: 0 2px 8px #1976d21a;
}
</style>
