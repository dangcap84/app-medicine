<template>
  <div
    class="home-hero"
    :style="{
      'background-image': 'url(https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=1500&q=80)',
      'background-size': 'cover',
      'background-position': 'center',
      'min-height': '60vh',
      position: 'relative'
    }"
  >
    <div class="overlay">
      <h1>MediTrack</h1>
      <p class="subtitle">Quản lý thuốc thông minh, chăm sóc sức khỏe chủ động</p>
      <v-btn color="primary" large @click="handleStart">Bắt đầu ngay</v-btn>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
const router = useRouter()
function handleStart() {
  if (localStorage.getItem('token')) {
    router.push('/medicines')
  } else {
    router.push('/register')
  }
}
</script>

<style scoped>
.home-hero {
  width: 100vw;
  height: calc(100vh - 64px);
  min-height: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  left: 50%;
  right: 50%;
  margin-left: -50vw;
  margin-right: -50vw;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  box-sizing: border-box;
  overflow: hidden;
}
.overlay {
  background: rgba(0, 0, 0, 0.45);
  padding: 48px 32px;
  border-radius: 18px;
  text-align: center;
  color: #fff;
  box-shadow: 0 8px 32px rgba(0,0,0,0.18);
}
h1 {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 16px;
  letter-spacing: 2px;
}
.subtitle {
  font-size: 1.4rem;
  margin-bottom: 32px;
  font-weight: 400;
}
@media (max-width: 900px) {
  .home-hero {
    height: calc(70vh - 56px);
    min-height: 0;
    padding: 0;
  }
  .overlay {
    padding: 24px 8px;
  }
  h1 {
    font-size: 2rem;
  }
  .subtitle {
    font-size: 1rem;
  }
}
@media (max-width: 600px) {
  .home-hero {
    height: calc(60vh - 56px);
    min-height: 0;
    padding: 0;
  }
  .overlay {
    padding: 16px 4px;
  }
  h1 {
    font-size: 1.3rem;
  }
  .subtitle {
    font-size: 0.9rem;
  }
}
</style>
