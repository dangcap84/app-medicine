<template>
  <v-card class="mx-auto my-12" max-width="600">
    <v-card-title>Danh sách thuốc</v-card-title>
    <v-card-text>
      <v-btn color="primary" class="mb-4" @click="openAdd" :disabled="loading">
        Thêm thuốc mới
      </v-btn>
      <v-btn color="secondary" class="mb-4 ml-2" @click="fetchMedicines" :loading="loading">
        Tải lại
      </v-btn>
      <v-alert v-if="error" type="error" dense>{{ error }}</v-alert>
      <v-list v-if="medicines.length">
        <v-list-item v-for="medicine in medicines" :key="medicine.id">
          <v-list-item-content>
            <v-list-item-title>{{ medicine.name }}</v-list-item-title>
            <v-list-item-subtitle>{{ medicine.description }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-btn icon="mdi-pencil" @click="openEdit(medicine)" />
            <v-btn icon="mdi-delete" color="error" @click="deleteMedicine(medicine.id)" />
          </v-list-item-action>
        </v-list-item>
      </v-list>
      <div v-else-if="!loading">Không có thuốc nào.</div>
      <medicine-form
        v-if="showForm"
        :medicine="selectedMedicine"
        @saved="onSaved"
        @close="closeForm"
      />
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
  description?: string
}

const medicines = ref<Medicine[]>([])
const loading = ref(false)
const error = ref('')
const showForm = ref(false)
const selectedMedicine = ref<Medicine | undefined>(undefined)

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

onMounted(fetchMedicines)
</script>
