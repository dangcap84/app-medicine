<template>
  <v-card class="mx-auto my-12" max-width="900">
    <v-card-title>Danh sách thuốc</v-card-title>
    <v-card-text>
      <v-btn color="primary" class="mb-4" @click="openAdd" :disabled="loading">
        Thêm thuốc mới
      </v-btn>
      <v-btn color="secondary" class="mb-4 ml-2" @click="fetchMedicines" :loading="loading">
        Tải lại
      </v-btn>
      <v-alert v-if="error" type="error" dense>{{ error }}</v-alert>
      <v-data-table
        :headers="headers"
        :items="medicines"
        :loading="loading"
        class="elevation-1"
        item-key="id"
        :items-per-page="8"
        no-data-text="Không có thuốc nào."
        dense
      >
        <template #item.medicineUnitId="{ item }">
          {{ getUnitName(item.medicineUnitId) }}
        </template>
        <template #item.actions="{ item }">
          <v-btn icon="mdi-pencil" variant="text" @click="openEdit(item)" />
          <v-btn icon="mdi-delete" color="error" variant="text" @click="deleteMedicine(item.id)" />
        </template>
      </v-data-table>
      <v-dialog v-model="showForm" max-width="500">
        <medicine-form
          v-if="showForm"
          :medicine="selectedMedicine"
          @saved="onSaved"
          @close="closeForm"
        />
      </v-dialog>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '../plugins/axios'
import MedicineForm from './MedicineForm.vue'

interface Medicine {
  id: number
  name: string
  dosage?: string
  medicineUnitId?: string
  description?: string
}

interface MedicineUnit {
  id: string
  name: string
}

const medicines = ref<Medicine[]>([])
const medicineUnits = ref<MedicineUnit[]>([])
const loading = ref(false)
const error = ref('')
const showForm = ref(false)
const selectedMedicine = ref<Medicine | undefined>(undefined)

const headers = [
  { text: 'Tên thuốc', value: 'name', align: "start" as const },
  { text: 'Liều lượng', value: 'dosage' },
  { text: 'Đơn vị', value: 'medicineUnitId' },
  { text: 'Mô tả', value: 'description' },
  { text: 'Thao tác', value: 'actions', sortable: false }
]

const fetchMedicines = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get('/medicines')
    medicines.value = res.data
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể tải danh sách thuốc'
  } finally {
    loading.value = false
  }
}

const fetchMedicineUnits = async () => {
  try {
    const res = await api.get('/medicine-units')
    medicineUnits.value = res.data
  } catch {
    // ignore
  }
}

const getUnitName = (id: string | undefined) => {
  if (!id) return ''
  const unit = medicineUnits.value.find(u => u.id === id)
  return unit ? unit.name : ''
}

const openAdd = () => {
  selectedMedicine.value = undefined
  showForm.value = true
}
const openEdit = (medicine: Medicine) => {
  selectedMedicine.value = { ...medicine }
  showForm.value = true
}
const closeForm = () => {
  showForm.value = false
}
const onSaved = () => {
  showForm.value = false
  fetchMedicines()
}
const deleteMedicine = async (id: number) => {
  if (!confirm('Bạn có chắc muốn xóa thuốc này?')) return
  loading.value = true
  error.value = ''
  try {
    await api.delete(`/medicines/${id}`)
    medicines.value = medicines.value.filter(m => m.id !== id)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Không thể xóa thuốc'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchMedicines()
  // fetchMedicineUnits() bị loại bỏ, chỉ gọi khi thực sự cần
})
</script>

<style scoped>
.v-data-table {
  border-radius: 16px;
  margin-top: 16px;
  background: #fafbfc;
}
.v-data-table th {
  background: #e3f2fd !important;
  color: #1976d2 !important;
  font-weight: 600;
  font-size: 1.05rem;
}
.v-data-table td {
  font-size: 1rem;
}
.v-btn[icon] {
  margin: 0 2px;
}
</style>
