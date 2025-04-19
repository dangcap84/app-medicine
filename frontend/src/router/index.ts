import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import LoginForm from '../components/LoginForm.vue'
import RegisterForm from '../components/RegisterForm.vue'
import MedicineList from '../components/MedicineList.vue'
import ScheduleList from '../components/ScheduleList.vue'
import NotificationList from '../components/NotificationList.vue'
import HomeHero from '../components/HomeHero.vue'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'Login', component: LoginForm, meta: { public: true } },
  { path: '/register', name: 'Register', component: RegisterForm, meta: { public: true } },
  { path: '/', name: 'Home', component: HomeHero, meta: { public: true } },
  { path: '/medicines', name: 'Medicines', component: MedicineList, meta: { requiresAuth: true } },
  { path: '/schedules', name: 'Schedules', component: ScheduleList, meta: { requiresAuth: true } },
  { path: '/notifications', name: 'Notifications', component: NotificationList, meta: { requiresAuth: true } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Route guard kiểm tra JWT
router.beforeEach((to, from, next) => {
  const isPublic = to.meta.public
  const token = localStorage.getItem('token')
  if (!isPublic && !token) {
    next({ name: 'Login' })
  } else {
    next()
  }
})

export default router
