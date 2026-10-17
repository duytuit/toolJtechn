# Quản lý bộ phận trên SQL Server

JavaScript của giao diện nằm cùng module tại `Views/departments.js`. Controller phục vụ riêng file này qua `/js/departments.js` cho người đã đăng nhập; file được sao chép khi build/publish.

## Cài đặt

1. Mở SSMS, chọn đúng database được cấu hình tại `ConnectionStrings:JtecDB_Default`.
2. Chạy `Sql/001_department.sql` để tạo mới bảng `dbo.department` bằng lệnh `CREATE TABLE` thông thường.
3. Khởi động lại ứng dụng sau khi build. Đăng nhập và mở `/admin/departments`, hoặc vào **Nhân sự → Danh sách bộ phận**.

Script dành cho database chưa có bảng `dbo.department`; không tự đổi tên hoặc nâng cấp bảng cũ. Không cần chạy EF migration. SQL chưa được tự động thực thi trên database. Model thống nhất là `Models/Department.cs`, dùng SQL Server qua `DBJtecContext`.

## Dữ liệu

Danh sách hiển thị cột **Tổng nhân viên**, chỉ tính nhân viên đang làm việc (`IsActive = true`), chưa xóa mềm và có liên kết `employee_department` chưa xóa, thống nhất với nút **Xem nhân viên**. Bộ phận chưa có nhân viên phù hợp hiển thị 0. Số lượng được tính khi truy vấn, không thêm cột vật lý vào bảng; danh sách tự tải lại sau khi đồng bộ.

| Trường | Kiểu SQL Server | Quy tắc |
| --- | --- | --- |
| `id` | `int identity` | Khóa chính |
| `code` | `varchar(50)` | Bắt buộc, không trùng, lưu in hoa; chữ không dấu, số, `_`, `-`; bắt đầu bằng chữ hoặc số |
| `name` | `nvarchar(150)` | Bắt buộc, hỗ trợ tiếng Việt |
| `description` | `nvarchar(1000)` | Không bắt buộc |
| `created_at` | `datetime2` | UTC |
| `updated_at` | `datetime2 null` | UTC |
| `deleted_at` | `datetime2 null` | Thời điểm xóa mềm UTC; `NULL` khi chưa xóa |

Giao diện theo quy ước `Employees/Views`: `panel-card`, bảng `_DataTable` phân trang 10 dòng, input `_CompactInput`, xác nhận và toast `JtechnUI`. Danh sách lấy dữ liệu từ `/api/departments/all`, tìm theo mã/tên ở trình duyệt. Trang thêm là `/admin/departments/create`, trang sửa là `/admin/departments/{id}/edit`; lưu xong quay lại danh sách với toast thành công. JavaScript dùng chung vẫn nằm tại `Views/departments.js` và được tải trong section `Scripts` sau các component chung.

Chỉnh sửa và xóa dùng ID bộ phận, không gửi phiên bản dữ liệu. Khi hai người cùng chỉnh sửa, lần lưu sau ghi đè thông tin lần lưu trước. Mã trùng được kiểm tra cả bằng API và unique constraint của SQL Server.

Xóa mềm bằng cách ghi `deleted_at`, giữ lại bản ghi và ẩn khỏi các truy vấn thông thường. Mã của bộ phận đã xóa vẫn được giữ và không tái sử dụng. Nếu bảng cũ thiếu cột này, bổ sung riêng bằng `ALTER TABLE dbo.department ADD deleted_at DATETIME2 NULL;`. Không tự đổi trường `Department` dạng chuỗi hiện có của nhân viên và chưa tạo liên kết khóa ngoại tới nhân viên. Repository bộ phận và API `/KTNQ` đọc bảng SQL Server `dbo.department`; không còn đọc danh mục MySQL. Các trường cũ như `status`, `permissions`, `parent_id` không còn trong model và kết quả danh mục bộ phận.

## Truy cập và API

Chức năng chỉ yêu cầu đăng nhập, không dùng quyền riêng cho bộ phận. Mọi tài khoản đã đăng nhập đều có thể xem, thêm, sửa và xóa bộ phận. Script thêm quyền riêng đã được bỏ; các quyền của module khác không thay đổi.

| Method | Đường dẫn |
| --- | --- |
| GET | `/api/departments?search=&page=1&pageSize=20` |
| GET | `/api/departments/all` |
| GET | `/api/departments/{id}` |
| POST | `/api/departments` |
| PUT | `/api/departments/{id}` |
| DELETE | `/api/departments/{id}` |

POST/PUT/DELETE yêu cầu antiforgery token và tài khoản đăng nhập.

Ví dụ body tạo mới:

```json
{
  "code": "DAP",
  "name": "Bộ phận Dập",
  "description": "Phụ trách công đoạn dập tanshi"
}
```

Tạo thành công trả `201`, sửa trả `200`, xóa trả `204`; dữ liệu không hợp lệ trả `400`, không tìm thấy trả `404`, trùng mã trả `409`.

## Xác minh sau khi cài đặt

Kiểm tra thêm bộ phận có tiếng Việt, tìm theo mã/tên, sửa mô tả và xóa một bản ghi thử. Kiểm tra mã trùng (kể cả khác chữ hoa/thường), bỏ trống tên/mã, độ dài vượt giới hạn và hai tab cùng sửa một bản ghi. Kiểm tra API từ chối truy cập khi chưa đăng nhập. Chỉ dùng database kiểm thử cho các thao tác này.
