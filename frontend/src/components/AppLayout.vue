<template>
  <v-app>
    <v-navigation-drawer
      v-model="drawer"
      app
      temporary
      :width="220"
      class="d-md-none"
    >
      <v-list>
        <v-list-item link :to="'/'">Trang chủ</v-list-item>
        <v-list-item link :to="'/medicines'">Thuốc</v-list-item>
        <v-list-item link :to="'/schedules'">Lịch uống</v-list-item>
        <v-list-item link :to="'/notifications'">Thông báo</v-list-item>
        <v-list-item link :to="'/register'" v-if="!isLoggedIn">Đăng ký</v-list-item>
        <v-list-item link :to="'/login'" v-if="!isLoggedIn">Đăng nhập</v-list-item>
        <v-list-item v-else @click="logout">Đăng xuất</v-list-item>
      </v-list>
    </v-navigation-drawer>
    <v-app-bar app color="primary" dark>
      <v-app-bar-nav-icon class="d-md-none" @click="drawer = !drawer" />
      <v-toolbar-title>MediTrack</v-toolbar-title>
      <v-spacer />
      <div class="d-none d-md-flex">
        <v-btn text :to="'/'" router>Trang chủ</v-btn>
        <v-btn text :to="'/medicines'" router>Thuốc</v-btn>
        <v-btn text :to="'/schedules'" router>Lịch uống</v-btn>
        <v-btn text :to="'/notifications'" router>Thông báo</v-btn>
        <v-btn text :to="'/register'" router v-if="!isLoggedIn">Đăng ký</v-btn>
        <v-btn text :to="'/login'" router v-if="!isLoggedIn">Đăng nhập</v-btn>
        <v-btn text @click="logout" v-else>Đăng xuất</v-btn>
      </div>
    </v-app-bar>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const drawer = ref(false)
const isLoggedIn = ref(!!localStorage.getItem('token'))

function updateLoginStatus() {
  isLoggedIn.value = !!localStorage.getItem('token')
}

onMounted(() => {
  window.addEventListener('storage', updateLoginStatus)
})
onUnmounted(() => {
  window.removeEventListener('storage', updateLoginStatus)
})

const logout = () => {
  localStorage.removeItem('token')
  updateLoginStatus()
  window.location.href = '/login'
}
</script>
