# Sidebar và đồng bộ quyền

## Gán vai trò theo bộ phận

Trang `/admin/employee-roles` có mục **Gán vai trò theo bộ phận**. Chọn bộ phận và các vai trò, rồi bấm **Lưu vai trò cho bộ phận**. Hệ thống bổ sung các vai trò cho nhân viên đang hoạt động có liên kết trong `employee_department`; giữ nguyên vai trò hiện có và bỏ qua vai trò đã gán. Nhân viên mới được đưa vào bộ phận sau đó không tự nhận vai trò; cần thực hiện lại thao tác gán.

API `PUT /api/admin/departments/{departmentId}/roles` nhận `{"roleCodes":["STAFF"]}`, yêu cầu quyền `PERMISSIONS.ASSIGN` và antiforgery token. Toàn bộ lượt gán được lưu trong một transaction vào `role_employee_web`; không thay đổi schema database.

Danh mục menu dùng chung tại `Configuration/SidebarMenu.cs`. Mỗi mục khai báo `Resource`, tiêu đề, URL, biểu tượng và nhóm. Sidebar kiểm tra quyền `<Resource>.VIEW` từ database; tài khoản/nhân viên và vai trò phải đang hoạt động. Vai trò `ADMIN` được xem tất cả menu theo cơ chế hiện có. Nhóm không có mục được phép xem sẽ không hiển thị.

Để thêm menu, khai báo một `SidebarMenuItem` mới, ví dụ:

```csharp
new SidebarMenuItem("TRAINING", "Đào tạo", "/admin/training", "✓", "Nhân sự")
```

Sau khi build, mở **Vai trò và quyền → Chọn vai trò để cấu hình quyền → Đồng bộ quyền**. Hệ thống đọc cùng danh mục menu và bổ sung `TRAINING.VIEW`, `TRAINING.CREATE`, `TRAINING.UPDATE`, `TRAINING.DELETE` vào bảng `permission_web`. Không cần viết SQL thêm từng quyền. Nút đồng bộ yêu cầu `PERMISSIONS.ASSIGN`; không tự gán quyền mới cho vai trò. Tích quyền rồi nhấn **Lưu quyền** để áp dụng.

Đồng bộ không xóa quyền cũ, không đổi tên/trạng thái quyền đang có, không sửa liên kết vai trò và không tạo trùng mã khi chạy lại. Các lựa chọn đang tích nhưng chưa lưu được giữ lại khi tải lại danh sách quyền sau đồng bộ. API đồng bộ là `POST /api/admin/permissions/sync`, có antiforgery token; logic nằm trong `RoleRepository` qua `IRoleRepository`.

Menu chỉ quyết định việc hiển thị; các endpoint vẫn áp dụng authorization hiện có, ví dụ gán vai trò cần `PERMISSIONS.ASSIGN`. Quản lý bài thi theo bộ phận dùng `DEPARTMENT_EXAMS.VIEW` cho trang và API đọc, cộng thêm CREATE/UPDATE/DELETE tương ứng khi ghi; không bắt buộc vai trò ADMIN. Khi xây dựng chức năng mới, gắn policy tương ứng tại controller cho cả route view và API, không dựa vào việc ẩn menu để bảo vệ dữ liệu. Các mục sản xuất/báo cáo có URL `#` vẫn là placeholder; đồng bộ quyền không tự tạo trang hoặc chức năng CRUD.

Sidebar đọc các quyền trong một lượt truy vấn danh sách, không gọi authorization riêng cho từng mục. Các thay đổi quyền đã lưu có hiệu lực khi tải lại trang, không cần đăng nhập lại đối với kiểm tra quyền theo database.

Kiểm tra danh mục: `dotnet run --project tests/JtechnApi.SidebarPermissions.Checks/JtechnApi.SidebarPermissions.Checks.csproj` từ thư mục gốc repository. Cần kiểm thử đồng bộ/gán quyền với SQL Server thực tế trước khi sử dụng.
