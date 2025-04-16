# Database Schema Design (MediTrack - PostgreSQL)

## Sơ đồ ERD (Mermaid)

```mermaid
erDiagram
    USERS ||--o{ USER_PROFILES : "has one"
    USERS ||--o{ MEDICINES : "manages"
    USERS ||--o{ SCHEDULES : "manages"
    USERS ||--o{ NOTIFICATIONS : "receives"

    USER_PROFILES {
        Guid UserId PK FK
        string FirstName
        string LastName
        date DateOfBirth
        string Gender
        string AvatarUrl nullable
        datetime CreatedAt
        datetime UpdatedAt
    }

    USERS {
        Guid Id PK
        string Email UK
        string PasswordHash
        datetime CreatedAt
        datetime UpdatedAt
    }

    MEDICINES {
        Guid Id PK
        Guid UserId FK
        string Name
        string Dosage
        Guid MedicineUnitId FK
        string Notes nullable
        datetime CreatedAt
        datetime UpdatedAt
    }

    MEDICINE_UNITS {
        Guid Id PK
        string Name UK  // e.g., "viên", "ml", "ống"
        string Description nullable
    }

    SCHEDULES {
        Guid Id PK
        Guid UserId FK
        Guid MedicineId FK
        date StartDate
        date EndDate nullable
        string FrequencyType  // e.g., "Daily", "Weekly", "SpecificDays"
        string DaysOfWeek nullable // e.g., "Mon,Wed,Fri" for SpecificDays
        string Notes nullable
        datetime CreatedAt
        datetime UpdatedAt
    }

    SCHEDULE_TIMES {
        Guid Id PK
        Guid ScheduleId FK
        time TimeOfDay // e.g., "08:00", "14:30", "21:00"
        int Quantity // Số lượng uống mỗi lần
    }

    NOTIFICATIONS {
        Guid Id PK
        Guid UserId FK
        Guid ScheduleTimeId FK
        datetime ScheduledTime // Thời gian dự kiến gửi thông báo
        datetime SentTime nullable // Thời gian thực tế gửi
        boolean IsRead
        string Message
        datetime CreatedAt
    }

    MEDICINES ||--|{ MEDICINE_UNITS : "uses"
    SCHEDULES ||--|{ MEDICINES : "schedules"
    SCHEDULES ||--o{ SCHEDULE_TIMES : "has multiple"
    NOTIFICATIONS ||--|{ SCHEDULE_TIMES : "relates to"

```

## Mô tả chi tiết các bảng

*   **USERS:**
    *   `Id (PK)`: Khóa chính, định danh duy nhất cho người dùng.
    *   `Email (UK)`: Địa chỉ email, dùng để đăng nhập, là duy nhất.
    *   `PasswordHash`: Chuỗi băm của mật khẩu người dùng.
    *   `CreatedAt`, `UpdatedAt`: Dấu thời gian tạo và cập nhật bản ghi.
*   **USER\_PROFILES:**
    *   `UserId (PK, FK)`: Khóa chính và khóa ngoại tham chiếu đến `USERS.Id`.
    *   `FirstName`, `LastName`: Họ và tên.
    *   `DateOfBirth`: Ngày sinh.
    *   `Gender`: Giới tính.
    *   `AvatarUrl`: Đường dẫn đến ảnh đại diện (có thể null).
    *   `CreatedAt`, `UpdatedAt`: Dấu thời gian.
*   **MEDICINES:**
    *   `Id (PK)`: Khóa chính.
    *   `UserId (FK)`: Khóa ngoại tham chiếu đến `USERS.Id`, xác định thuốc này của ai.
    *   `Name`: Tên thuốc.
    *   `Dosage`: Liều lượng (ví dụ: "500mg", "10ml").
    *   `MedicineUnitId (FK)`: Khóa ngoại tham chiếu đến `MEDICINE_UNITS.Id`, xác định đơn vị của thuốc.
    *   `Notes`: Ghi chú thêm về thuốc (có thể null).
    *   `CreatedAt`, `UpdatedAt`: Dấu thời gian.
*   **MEDICINE\_UNITS:**
    *   `Id (PK)`: Khóa chính.
    *   `Name (UK)`: Tên đơn vị (ví dụ: "viên", "ml", "ống"), là duy nhất.
    *   `Description`: Mô tả thêm (có thể null).
*   **SCHEDULES:**
    *   `Id (PK)`: Khóa chính.
    *   `UserId (FK)`: Khóa ngoại tham chiếu đến `USERS.Id`.
    *   `MedicineId (FK)`: Khóa ngoại tham chiếu đến `MEDICINES.Id`, xác định lịch này cho thuốc nào.
    *   `StartDate`, `EndDate`: Ngày bắt đầu và kết thúc lịch trình (EndDate có thể null nếu uống vô thời hạn).
    *   `FrequencyType`: Loại tần suất ("Daily", "Weekly", "SpecificDays").
    *   `DaysOfWeek`: Các ngày trong tuần cần uống (chỉ dùng khi `FrequencyType` là "SpecificDays", ví dụ: "Mon,Wed,Fri").
    *   `Notes`: Ghi chú thêm (có thể null).
    *   `CreatedAt`, `UpdatedAt`: Dấu thời gian.
*   **SCHEDULE\_TIMES:**
    *   `Id (PK)`: Khóa chính.
    *   `ScheduleId (FK)`: Khóa ngoại tham chiếu đến `SCHEDULES.Id`.
    *   `TimeOfDay`: Thời gian cụ thể trong ngày cần uống (ví dụ: 08:00:00).
    *   `Quantity`: Số lượng thuốc cần uống vào thời điểm này.
*   **NOTIFICATIONS:**
    *   `Id (PK)`: Khóa chính.
    *   `UserId (FK)`: Khóa ngoại tham chiếu đến `USERS.Id`.
    *   `ScheduleTimeId (FK)`: Khóa ngoại tham chiếu đến `SCHEDULE_TIMES.Id`, xác định thông báo này cho lần uống nào.
    *   `ScheduledTime`: Thời gian dự kiến gửi thông báo (thường là `SCHEDULES.Date` kết hợp `SCHEDULE_TIMES.TimeOfDay`).
    *   `SentTime`: Thời gian thực tế thông báo được gửi đi (có thể null nếu chưa gửi).
    *   `IsRead`: Trạng thái đã đọc hay chưa.
    *   `Message`: Nội dung thông báo.
    *   `CreatedAt`: Dấu thời gian tạo thông báo.

*(Thiết kế này có thể được điều chỉnh và bổ sung khi cần thiết trong quá trình phát triển.)*
