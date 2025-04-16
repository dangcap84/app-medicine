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
9.  [ ] **Notification API:** (Get notifications for user, mark as read)
10. [ ] **Unit/Integration Testing:** (Implement tests for services and controllers)
11. [X] **Swagger/OpenAPI:** Configure Swagger for API documentation and testing. (Includes XML comments)
12. [ ] **CORS Configuration:** Allow frontend application to call the API.
13. [ ] **Error Handling & Logging:** Implement global error handling and enhance logging.

## Frontend Setup (Vue.js with TypeScript)

1.  [ ] **Project Setup:** Initialize Vue.js project using Vite.
2.  [ ] **Install Dependencies:** Add `axios` for API calls, `vue-router` for routing, state management library (e.g., Pinia).
3.  [ ] **Authentication:**
    *   [ ] Create Login/Register pages/components.
    *   [ ] Implement logic to call Auth API endpoints.
    *   [ ] Store JWT token securely (e.g., localStorage/sessionStorage).
    *   [ ] Setup Axios interceptors to attach token to requests.
    *   [ ] Implement route guards for protected routes.
4.  [ ] **Medicine Management:**
    *   [ ] Create components for listing, adding, editing, deleting medicines.
    *   [ ] Implement logic to call Medicine API endpoints.
5.  [ ] **Schedule Management:**
    *   [ ] Create components for managing schedules.
    *   [ ] Implement logic to call Schedule API endpoints.
6.  [ ] **Notifications:**
    *   [ ] Display notifications to the user.
    *   [ ] Implement logic to call Notification API endpoints.
7.  [ ] **UI/UX:** Implement basic styling and layout.
