# Nhân viên và bộ phận

## Gán bộ phận cho nhân viên

- Ô chọn nhân viên dùng `ApiFilterDropdownModel` với API `/api/employees/department-search?q=...`, tìm theo mã/tên và trả tối đa 50 kết quả mỗi lần tìm.
- Trong bảng xem nhân viên của một bộ phận có nút **Xóa** (quyền `EMPLOYEES.UPDATE`): xác nhận gỡ và xóa mềm liên kết qua `DELETE /api/departments/{departmentId}/employees/{employeeId}`. Không xóa hồ sơ nhân viên hoặc liên kết với các bộ phận khác. Đồng bộ từ cột `employee.Department` có thể khôi phục liên kết này nếu dữ liệu nguồn vẫn khớp.

Đồng bộ ưu tiên tên trong bảng `department`: bỏ tiền tố `PD-`, chuẩn hóa dấu, hoa/thường, dấu phân cách và khoảng trắng. Ví dụ `PD-Cắt` → `Cắt`, `PD-Lắp ráp` → `Lắp ráp`, `PD-Kho Nhập` → `Kho Nhập`, `GA- Humanresorce` → `GA`. Tên trùng chính xác được ưu tiên trước mã; nếu không khớp, tìm tên bộ phận nguyên cụm từ trong chuỗi nguồn (ví dụ `Nhân viên kho xuất` → `Kho xuất`). Chỉ gán khi có duy nhất một bộ phận phù hợp; không đoán từ viết tắt ngắn hoặc lỗi chính tả tùy ý. Logic nằm trong `Repositories/DepartmentNameMatcher.cs`.

Kiểm tra ánh xạ: `dotnet run --project tests/JtechnApi.DepartmentMatching.Checks/JtechnApi.DepartmentMatching.Checks.csproj` từ thư mục gốc repository.

- Trên trang bộ phận `/admin/departments`, nút **Đồng bộ nhân viên** lấy `employee.Department` để thêm/khôi phục liên kết. So khớp với tên hoặc mã bộ phận hiện có, bỏ khác biệt hoa/thường, dấu và khoảng trắng; chấp nhận cả “Kĩ thuật” và “Kỹ thuật”. Không tự tạo bộ phận, không tách chuỗi nhiều bộ phận và không xóa các liên kết cũ. Giá trị không khớp hoặc khớp nhiều bộ phận được báo theo tên và số lượng nhân viên.
- Đồng bộ gồm mọi nhân viên chưa xóa mềm, kể cả ngừng hoạt động; bỏ qua bộ phận đã xóa. Gán thủ công vẫn chỉ cho nhân viên đang hoạt động. Chạy đồng bộ lặp lại không tạo trùng cặp liên kết.
- Nút **Xem nhân viên** ở từng dòng bộ phận mở `/admin/departments/{id}/employees`, hiển thị nhân viên đang có liên kết chưa xóa, mã, tên, email, điện thoại và trạng thái; có tìm kiếm và phân trang. Cần quyền `EMPLOYEES.VIEW`.
- API bổ sung: `POST /api/employees/sync-departments` (quyền xem/cập nhật nhân viên và antiforgery token); `GET /api/departments/{id}/employees` (quyền xem nhân viên). Đồng bộ cập nhật bảng liên kết, không ghi ngược vào cột `employee.Department`.

- Trang: `/admin/employee-departments`, trong menu **Nhân sự → Bộ phận nhân viên**.
- Một nhân viên có thể thuộc nhiều bộ phận. Chọn nhân viên, tích các bộ phận và nhấn **Lưu bộ phận**. Bỏ chọn tất cả để gỡ toàn bộ liên kết.
- Quyền xem: `EMPLOYEES.VIEW`; quyền lưu: đồng thời có `EMPLOYEES.VIEW` và `EMPLOYEES.UPDATE`. Nhân viên ngừng hoạt động chỉ được xem liên kết.
- Có thể mở trực tiếp nhân viên bằng `/admin/employee-departments?employeeId=123`.
- Giao diện dùng `panel-card`, `JtechnUI.toast` và `JtechnUI.confirm` theo module Employees; JavaScript nằm trong `Views/employee-departments.js`.

## SQL Server

Chọn database của `ConnectionStrings:JtecDB_Default`, chạy `Sql/001_employee_department.sql` sau khi đã có `dbo.employee` và `dbo.department`. Script tạo mới bảng thông thường; chưa tự thực thi trên database.

`dbo.employee_department` gồm `id`, `employee_id`, `department_id`, `created_at`, `updated_at`, `deleted_at`; có unique constraint cho cặp nhân viên/bộ phận, không dùng khóa ngoại hoặc cột phiên bản dữ liệu. Repository kiểm tra ID nhân viên và bộ phận trước khi lưu. Thời gian lưu UTC.

Gỡ bộ phận là xóa mềm liên kết; gán lại khôi phục cùng bản ghi. Bộ phận/nhân viên bị xóa mềm sẽ không xuất hiện trong các lựa chọn và kết quả đang hoạt động. Lưu danh sách dùng transaction Serializable để kiểm tra tính hợp lệ và cập nhật toàn bộ liên kết nhất quán.

Đây là danh sách liên kết có ID. Trường `Employee.Department` dạng chuỗi hiện tại vẫn giữ nguyên cho các luồng cũ; không tự chuyển đổi dữ liệu chuỗi hoặc đồng bộ hai chiều. Danh sách nhân viên hiện có vẫn hiển thị trường chuỗi đó.

## Cấu trúc

- `Models/EmployeeDepartment.cs`: mapping bảng.
- `Dtos/EmployeeDepartmentInput.cs`: danh sách ID bộ phận được chọn.
- `Repositories/IEmployeeDepartmentRepository.cs`, `EmployeeDepartmentRepository.cs`: truy vấn, validation nghiệp vụ, lưu và khôi phục liên kết.
- `Controllers/EmployeeDepartmentController.cs`: route, xác thực/quyền và HTTP response.
- DI: `AddScoped<IEmployeeDepartmentRepository, EmployeeDepartmentRepository>()`.

| Method | Route | Nội dung |
| --- | --- | --- |
| GET | `/api/employees/department-options` | Nhân viên và bộ phận để chọn |
| GET | `/api/employees/{employeeId}/departments` | Bộ phận hiện tại của nhân viên |
| PUT | `/api/employees/{employeeId}/departments` | Thay thế toàn bộ bộ phận được gán |

Body PUT: `{"departmentIds":[1,2]}`; `[]` để gỡ tất cả. PUT yêu cầu antiforgery token. ID không tồn tại/đã xóa trả `400`, nhân viên không tồn tại trả `404`, nhân viên ngừng hoạt động trả `409`. Lưu sau cùng thay thế danh sách của lần lưu trước.

Sau khi chạy SQL và khởi động lại ứng dụng, kiểm tra gán nhiều bộ phận, bỏ chọn, gán lại, lưu lặp không trùng liên kết, bộ phận bị xóa và quyền tài khoản chỉ xem trên database kiểm thử.
