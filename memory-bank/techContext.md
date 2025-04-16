# Tech Context: MediTrack

## Công nghệ đề xuất

Dựa trên yêu cầu và mục tiêu của dự án, các công nghệ sau được đề xuất:

### Frontend
- **Framework/Library:** Vue 3
    - *Lý do:* Hiệu suất cao, hệ sinh thái tốt, dễ tiếp cận và bảo trì.
- **Build Tool:** Vite
    - *Lý do:* Tốc độ build và development server cực nhanh.
- **State Management:** Pinia
    - *Lý do:* Giải pháp quản lý state chính thức cho Vue 3, nhẹ nhàng và dễ sử dụng.
- **UI Framework:**
    - *Lựa chọn:* **Vuetify** - Cung cấp bộ component UI đầy đủ, theo Material Design. Giúp đẩy nhanh quá trình phát triển MVP.

### Backend
- **Framework:** .NET Core Web API (phiên bản LTS mới nhất)
    - *Lý do:* Hiệu năng cao, đa nền tảng, hệ sinh thái mạnh mẽ, phù hợp với kiến trúc RESTful/GraphQL. Tích hợp tốt với Azure.
- **ORM:** Entity Framework Core
    - *Lý do:* ORM phổ biến và mạnh mẽ cho .NET, đơn giản hóa tương tác database.
- **Authentication:** JWT (JSON Web Tokens) / OAuth 2.0
    - *Lý do:* Tiêu chuẩn phổ biến cho xác thực API. JWT phù hợp cho xác thực nội bộ, OAuth 2.0 nếu cần tích hợp bên thứ ba.
- **Database:**
    - *Lựa chọn:* **PostgreSQL** - Mã nguồn mở mạnh mẽ, tiết kiệm chi phí license, được hỗ trợ tốt trên nhiều nền tảng cloud (bao gồm Azure Database for PostgreSQL).

### Dịch vụ khác
- **Push Notifications:** Firebase Cloud Messaging (FCM)
    - *Lý do:* Giải pháp miễn phí, đa nền tảng, đáng tin cậy cho push notification.
- **OCR (Nhận dạng ký tự quang học):**
    - *Lựa chọn:* Google Cloud Vision API, Tesseract OCR (tự host), hoặc các dịch vụ OCR chuyên biệt khác.
    - *Lưu ý:* Tính năng nâng cao, cần đánh giá chi phí và độ chính xác. Có thể tích hợp sau MVP.
- **Hosting:** Microsoft Azure
    - *Lý do:* Tích hợp tốt với .NET Core, cung cấp nhiều dịch vụ PaaS (Platform as a Service) giúp đơn giản hóa việc triển khai và quản lý. Các lựa chọn khác như AWS, Google Cloud cũng khả thi.

## Môi trường phát triển (Dự kiến)
- **IDE:** Visual Studio Code (cho cả Frontend và Backend), Visual Studio (cho Backend .NET).
- **Source Control:** Git (GitHub, GitLab, Azure Repos).
- **Containerization (Tùy chọn):** Docker có thể được sử dụng để đóng gói và triển khai ứng dụng nhất quán.
