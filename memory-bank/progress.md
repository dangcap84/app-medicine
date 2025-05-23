# Project Progress

## Backend Setup

1.  [X] **Project Setup:** Created solution and projects (`Domain`, `Application`, `Infrastructure`, `Api`).
2.  [X] **EF Core & PostgreSQL Setup:**
    *   [X] Added `Npgsql.EntityFrameworkCore.PostgreSQL` package.
    *   [X] Created all necessary Domain Entities (`User`, `UserProfile`, `Medicine`, `MedicineUnit`, `Schedule`, `ScheduleTime`, `Notification`).
    *   [X] Created `ApplicationDbContext` with `DbSet`s and configurations.
    *   [X] Added connection string to `appsettings.Development.json`.
    *   [X] Registered `ApplicationDbContext` in `Program.cs`.
3.  [X] **Initial Migration:**
    *   [X] Installed `dotnet-ef` tool.
    *   [X] Created `InitialCreate` migration.
    *   [X] Applied migration to the database.
4.  [X] **JWT Authentication API:**
    *   [X] Added `Microsoft.AspNetCore.Authentication.JwtBearer` package to Api project.
    *   [X] Added JWT settings to `appsettings.json`.
    *   [X] Created Auth DTOs (`RegisterRequestDto`, `LoginRequestDto`, `AuthResponseDto`) in Application project.
    *   [X] Created `IAuthService` interface in Application project.
    *   [X] Added `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`, `Microsoft.IdentityModel.Tokens` packages to Infrastructure project.
    *   [X] Implemented `AuthService` in Infrastructure project.
    *   [X] Registered `IAuthService` and configured Authentication/Authorization in `Program.cs`.
    *   [X] Created `AuthController` in Api project.
5.  [X] **Medicine CRUD API:**
    *   [X] Created Medicine DTOs (`MedicineDto`, `CreateMedicineDto`, `UpdateMedicineDto`) in Application project.
    *   [X] Created `IMedicineService` interface in Application project.
    *   [X] Implemented `MedicineService` in Infrastructure project.
    *   [X] Registered `IMedicineService` in `Program.cs`.
    *   [X] Created `MedicinesController` with CRUD actions and `[Authorize]` attribute in Api project.
6.  [X] **Schedule CRUD API:**
    *   [X] Created DTOs for Schedule (`ScheduleTimeDto`, `ScheduleDto`, `CreateScheduleTimeDto`, `CreateScheduleDto`, `UpdateScheduleDto`).
    *   [X] Defined `FrequencyType` enum in Domain project.
    *   [X] Updated `Schedule` entity to use `FrequencyType` enum.
    *   [X] Configured EF Core to store `FrequencyType` as string.
    *   [X] Created and applied migration for `FrequencyType` change.
    *   [X] Created Interface `IScheduleService`.
    *   [X] Implemented `ScheduleService`.
    *   [X] Registered `IScheduleService`.
    *   [X] Created `SchedulesController`.
    *   [X] Resolved build errors related to `FrequencyType`.
7.  [X] **UserProfile CRUD API:** (Get, Update)
    *   [X] Create DTOs for UserProfile (UserProfileDto, UpdateUserProfileDto).
    *   [X] Create Interface `IUserProfileService`.
    *   [X] Implement `UserProfileService`.
    *   [X] Register Service.
    *   [X] Create `UserProfilesController`.
8.  [X] **Notification Logic:** (Background service or scheduled job to create notifications based on schedules)
    *   [X] Create Interface `INotificationService`.
    *   [X] Implement `NotificationService`.
    *   [X] Create background service `NotificationGenerationService`.
    *   [X] Configure notification settings in `appsettings.json`.
    *   [X] Register services in `Program.cs`.
9.  [X] **Notification API:** (Get notifications for user, mark as read)
    *   [X] Add DTOs for Notification (NotificationDto, UpdateNotificationDto).
    *   [X] Update INotificationService with API methods.
    *   [X] Update NotificationService implementation.
    *   [X] Add UpdatedAt to Notification entity and create migration.
    *   [X] Create NotificationsController with endpoints.
10. [X] **Unit/Integration Testing:**
    *   [X] Created basic Unit Test files for Services (`Auth`, `Medicine`, `Schedule`, `UserProfile`, `Notification`) using Moq.
    *   [X] Implemented comprehensive Unit Tests for Services including input validation and error cases.
    *   [X] Implemented Integration Tests for Controllers (`Auth`, `Medicines`, `Schedules`, `UserProfiles`, `Notifications`).
11. [X] **Swagger/OpenAPI:** Configure Swagger for API documentation and testing. (Includes XML comments)
12. [X] **CORS Configuration:** Allow frontend application (localhost:3000, localhost:5173) to call the API.
13. [X] **Error Handling & Logging:**
    *   [X] Created custom exception types for domain-specific errors.
    *   [X] Implemented global error handling middleware.
    *   [X] Configured structured logging with Serilog.
    *   [X] Added request/response logging middleware.

## Frontend Setup (Vue.js with TypeScript)

1.  [X] **Project Setup:** Đã khởi tạo dự án Vue 3 + Vite + TypeScript, cấu trúc src, components, assets, style.css, main.ts chuẩn Vuetify.
2.  [X] **Install Dependencies:** Đã cài đặt đầy đủ: vue, vuetify, @mdi/font, vite, typescript, vue-tsc, @vitejs/plugin-vue, pinia, vue-router.
3.  [X] **Authentication:**
    *   [X] Tạo component LoginForm.vue và RegisterForm.vue.
    *   [X] Gọi API đăng nhập/đăng ký, lưu JWT vào localStorage, xử lý lỗi cơ bản.
    *   [X] Setup Axios interceptors để tự động đính kèm token vào request.
    *   [X] Implement route guards cho các route bảo vệ.
4.  [X] **Medicine Management:**
    *   [X] Tạo component MedicineList.vue, hiển thị danh sách thuốc, gọi API lấy danh sách thuốc từ backend.
    *   [X] Tạo component MedicineForm.vue, thêm/sửa thuốc, xóa thuốc, cập nhật danh sách.
5.  [X] **Schedule Management:**
    *   [X] Tạo component ScheduleList.vue, hiển thị danh sách lịch, gọi API.
    *   [X] Tạo component ScheduleForm.vue, thêm/sửa/xóa lịch, cập nhật danh sách.
6.  [X] **Notifications:**
    *   [X] Tạo component NotificationList.vue, hiển thị danh sách thông báo, đánh dấu đã đọc.
7.  [X] **UI/UX:** Tạo layout tổng thể AppLayout.vue, navigation, responsive, tích hợp router và các component vào layout.
