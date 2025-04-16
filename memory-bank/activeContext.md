# Active Context: MediTrack - Task 1: Thiết lập cấu trúc dự án

## Trọng tâm hiện tại (Task này)
- **Tạo cấu trúc thư mục gốc:** Tạo thư mục `backend` và `frontend`.
- **Khởi tạo dự án cơ bản (Tùy chọn):** Sử dụng CLI để tạo sườn dự án .NET và Vue.
- **Cập nhật Memory Bank:** Ghi lại trạng thái và các bước tiếp theo cho các task sau.

## Thay đổi gần đây
- Hoàn tất thiết lập Memory Bank cốt lõi (`projectbrief.md`, `productContext.md`, `systemPatterns.md`, `techContext.md`, `activeContext.md`, `progress.md`).
- Tạo tệp `.clinerules`.
- Thống nhất kế hoạch tổng thể, công nghệ (Vuetify, PostgreSQL), và cấu trúc thư mục dự án.
- Xác định phạm vi cho task hiện tại: chỉ tạo cấu trúc thư mục và khởi tạo cơ bản.

## Các bước tiếp theo (Cho các Task sau)
1.  **Backend (Task 2+):**
    *   Thiết kế chi tiết và triển khai Database Schema (PostgreSQL).
    *   Thiết lập EF Core và migrations.
    *   Xây dựng API Xác thực (Đăng ký/Đăng nhập JWT).
    *   Xây dựng API CRUD cho User Profiles, Medicines, Schedules.
    *   Triển khai logic tạo Notifications.
    *   Tích hợp FCM.
2.  **Frontend (Task 3+):**
    *   Cài đặt và cấu hình Vuetify.
    *   Thiết lập Vue Router, Pinia stores.
    *   Xây dựng Layouts.
    *   Xây dựng Views và Components cho các luồng chính (Auth, Profile, Medicine, Schedule).
    *   Tích hợp API calls.
    *   Xử lý Push Notifications.
3.  **Khác (Task 4+):**
    *   Thiết lập CI/CD.
    *   Viết Tests.

## Quyết định & Cân nhắc đang hoạt động
- **Công nghệ đã chọn:**
    - Frontend UI: **Vuetify**
    - Database: **PostgreSQL**
- **OCR:** Xác định là tính năng nâng cao, sẽ đưa vào sau MVP.
- **Khởi tạo dự án cơ bản:** Sẽ hỏi người dùng có muốn thực hiện trong task này không sau khi tạo thư mục.
