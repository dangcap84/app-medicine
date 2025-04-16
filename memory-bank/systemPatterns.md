# System Patterns: MediTrack

## Kiến trúc tổng quan
Hệ thống MediTrack dự kiến sẽ theo kiến trúc Client-Server điển hình:
- **Client (Frontend):** Ứng dụng di động (sử dụng Vue 3) giao tiếp với Backend qua API.
- **Server (Backend):** .NET Core Web API cung cấp các endpoint để xử lý logic nghiệp vụ và tương tác với cơ sở dữ liệu.
- **Database:** Cơ sở dữ liệu quan hệ (SQL Server hoặc PostgreSQL) để lưu trữ dữ liệu người dùng, thuốc, lịch trình, v.v.
- **Dịch vụ bên thứ ba:** Tích hợp với các dịch vụ như Firebase Cloud Messaging (Push Notification) và API OCR.

```mermaid
graph LR
    A[Mobile App (Vue 3)] --> B{Backend API (.NET Core)};
    B --> C[(Database)];
    B --> D[Firebase Cloud Messaging];
    B --> E[OCR Service];
```

## Các mẫu thiết kế chính (Dự kiến)
- **RESTful API:** Backend sẽ cung cấp API theo chuẩn REST để Frontend tương tác. Cân nhắc GraphQL nếu cần độ linh hoạt cao hơn trong truy vấn dữ liệu trong tương lai.
- **Repository Pattern:** Sử dụng Repository Pattern kết hợp với Unit of Work trong tầng Backend (.NET Core) để trừu tượng hóa việc truy cập dữ liệu, giúp code dễ quản lý và test hơn.
- **Dependency Injection:** Tận dụng cơ chế Dependency Injection sẵn có của .NET Core để quản lý các dependency trong ứng dụng Backend.
- **State Management (Frontend):** Sử dụng Pinia để quản lý trạng thái ứng dụng Vue 3 một cách hiệu quả, đặc biệt khi ứng dụng phức tạp hơn.
- **Asynchronous Processing:** Sử dụng các tác vụ bất đồng bộ (async/await) trong .NET Core để xử lý các yêu cầu I/O (như gọi API bên ngoài, truy cập database) mà không block luồng chính, cải thiện hiệu năng.

## Quyết định kỹ thuật quan trọng (Ban đầu)
- **Xác thực:** Sử dụng JWT (JSON Web Tokens) cho việc xác thực người dùng giữa Frontend và Backend. OAuth 2.0 có thể được xem xét nếu cần tích hợp đăng nhập qua các nhà cung cấp khác (Google, Facebook).
- **ORM:** Sử dụng Entity Framework Core làm ORM để tương tác với cơ sở dữ liệu, giúp đơn giản hóa việc ánh xạ đối tượng và truy vấn.
- **Push Notification:** Tích hợp Firebase Cloud Messaging (FCM) để gửi thông báo nhắc nhở uống thuốc đến thiết bị người dùng.
- **OCR:** Tìm kiếm và tích hợp một dịch vụ OCR (có thể là Google Cloud Vision hoặc một giải pháp mã nguồn mở như Tesseract được host) để hỗ trợ nhập đơn thuốc từ ảnh. Đây là tính năng nâng cao, có thể đưa vào sau MVP.

## Cân nhắc về khả năng mở rộng và bảo trì
- **Modular Design:** Thiết kế cả Frontend và Backend theo hướng module hóa để dễ dàng thêm/bớt tính năng.
- **Logging & Monitoring:** Triển khai cơ chế logging (ví dụ: Serilog) và monitoring để theo dõi sức khỏe hệ thống và gỡ lỗi.
- **Testing:** Xây dựng unit test và integration test cho Backend, và component test/e2e test cho Frontend để đảm bảo chất lượng.
