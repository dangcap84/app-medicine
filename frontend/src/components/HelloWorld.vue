<script setup lang="ts">
import { ref } from 'vue'

defineProps<{ msg: string }>()

const count = ref(0)
</script>

<template>
  <div class="home-banner">
    <img src="/vite.svg" alt="MediTrack Logo" class="banner-img" />
    <div class="banner-content">
      <h1>MediTrack</h1>
      <p class="slogan">Ứng dụng quản lý thuốc & nhắc lịch uống hiện đại</p>
    </div>
  </div>

  <div class="home-section">
    <h2>Chào mừng bạn đến với MediTrack!</h2>
    <p>
      Quản lý thuốc, lịch uống, thông báo nhắc nhở và nhiều tiện ích sức khỏe khác.<br>
      Giao diện thân thiện, dễ sử dụng trên mọi thiết bị.
    </p>
  </div>

  <div class="card">
    <button type="button" @click="count++" class="primary-btn">Đếm: {{ count }}</button>
    <p>
      Bạn có thể chỉnh sửa file
      <code>components/HelloWorld.vue</code> để tuỳ biến trang chủ.
    </p>
  </div>
</template>

<style scoped>
.home-banner {
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(90deg, #e3f2fd 0%, #fff 100%);
  padding: 32px 0 16px 0;
  border-radius: 16px;
  box-shadow: 0 4px 16px #1976d21a;
  margin-bottom: 24px;
}
.banner-img {
  width: 96px;
  height: 96px;
  margin-right: 32px;
  border-radius: 50%;
  box-shadow: 0 2px 8px #1976d233;
  background: #fff;
}
.banner-content h1 {
  margin: 0;
  font-size: 2.5rem;
  color: #1976d2;
  font-weight: bold;
}
.slogan {
  font-size: 1.2rem;
  color: #1976d2cc;
  margin-top: 8px;
}
.home-section {
  text-align: center;
  margin-bottom: 32px;
}
.card {
  margin: 0 auto;
  max-width: 400px;
  padding: 24px;
  border-radius: 12px;
  background: #fafbfc;
  box-shadow: 0 2px 8px #1976d21a;
  text-align: center;
}
.primary-btn {
  background: #1976d2;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 8px 24px;
  font-size: 1rem;
  margin-bottom: 12px;
  cursor: pointer;
  transition: background 0.2s;
}
.primary-btn:hover {
  background: #1565c0;
}
</style>
