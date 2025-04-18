<template>
  <v-app>
    <v-app-bar app color="primary" dark>
      <v-toolbar-title>MediTrack</v-toolbar-title>
      <v-spacer />
      <v-btn text :to="'/'" router>Trang chủ</v-btn>
      <v-btn text :to="'/medicines'" router>Thuốc</v-btn>
      <v-btn text :to="'/schedules'" router>Lịch uống</v-btn>
      <v-btn text :to="'/notifications'" router>Thông báo</v-btn>
      <v-btn text :to="'/register'" router v-if="!isLoggedIn">Đăng ký</v-btn>
      <v-btn text :to="'/login'" router v-if="!isLoggedIn">Đăng nhập</v-btn>
      <v-btn text @click="logout" v-else>Đăng xuất</v-btn>
    </v-app-bar>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const isLoggedIn = computed(() => !!localStorage.getItem('token'))

const logout = () => {
  localStorage.removeItem('token')
  window.location.href = '/login'
}
</script>
