# Quản lý bài thi theo bộ phận

## Cài đặt và sử dụng

1. Chọn database SQL Server của `ConnectionStrings:JtecDB_Default`, chạy `Sql/001_department_exam.sql` để tạo bảng mới.
2. Khởi động lại ứng dụng và đăng nhập tài khoản có quyền `DEPARTMENT_EXAMS.VIEW` (vai trò `ADMIN` có toàn quyền).
3. Mở `/admin/department-exams` hoặc menu **Bài thi theo bộ phận**.
4. Thêm bài thi, chọn bộ phận có trong danh mục, nhập mã, tên, mô tả, thời gian và nội dung JSON. Có thể tải file `Data/stamping-new.json` trên máy để nhập đề mẫu vào form.
5. Nhấn **Lưu bài thi**. Một bộ phận có thể có nhiều bài; lọc danh sách bằng ô Bộ phận.
6. Nhấn **Làm bài: ...** dưới danh sách để mở `/admin/exams?examId={id}` với đề lấy từ SQL Server.

## Nhập câu hỏi bằng form hoặc import

- Trong **Thêm bài thi**, nhập thông tin chung, nhấn **Thêm nhóm câu hỏi**, đặt tên nhóm rồi **Thêm câu hỏi**.
- Mỗi câu nhập nội dung, điểm, đường dẫn ảnh tùy chọn và loại câu. Thêm các lựa chọn, tích đáp án đúng với câu chọn một/nhiều; với câu ghép số, nhập số trên hình tương ứng từng lựa chọn.
- Tổng điểm hiển thị ngay khi sửa và phải bằng 100 trước khi lưu. Có thể xóa câu, nhóm hoặc lựa chọn. Đổi loại câu sẽ đặt lại đáp án đúng để chọn lại.
- **Import đề thi từ file JSON** chấp nhận file tối đa 1 MB, cùng cấu trúc với `stamping-new.json`. Import yêu cầu xác nhận nếu form đã có câu hỏi; không tự lưu vào database. Sau import có thể sửa từng câu bằng form rồi nhấn **Lưu bài thi**.
- Phần **Nội dung JSON nâng cao** cho phép dán JSON rồi **Áp dụng JSON vào form**, hoặc **Lấy JSON từ form** để xem dữ liệu hiện tại. Khi lưu, form câu hỏi là nguồn dữ liệu chính; nội dung dán chưa áp dụng không thay thế câu hỏi.
- Sửa bài đã lưu sẽ dựng lại toàn bộ câu hỏi trong form; giữ ID câu hỏi/lựa chọn và HTML trong đáp án. JSON dùng tên trường camelCase hoặc PascalCase đều được hỗ trợ.
- JavaScript dựng form tại `Views/exam-question-editor.js`; kiểm tra cấu trúc và đáp án vẫn được thực hiện lại trên server tại `Repositories/ExamDefinitionValidator.cs`.

Script chưa được tự chạy trên database. Script là `CREATE TABLE` thông thường, không chạy lại nếu bảng đã tồn tại. Không dùng `FOREIGN KEY`, hậu tố `_web` hoặc cột phiên bản dữ liệu. Repository kiểm tra bộ phận tồn tại và chưa bị xóa trước khi lưu. Mã bài thi là duy nhất, không tái sử dụng mã đã xóa mềm.

## Dữ liệu lưu

`dbo.department_exam` gồm `id`, `department_id`, `code`, `name`, `description`, `minutes`, `definition_json`, `is_active`, `created_at`, `updated_at`, `deleted_at`.

- Mỗi bài thuộc một bộ phận; bộ phận có nhiều bài.
- Nội dung câu hỏi và đáp án lưu trong `definition_json`, không chỉ lưu đường dẫn file.
- Mã tối đa 50 ký tự; tên 200; mô tả 1.000; thời gian 1–480 phút.
- JSON theo `OnlineExamDefinition`: có `groups`, `questions`, `options`, `correct`; các loại câu `single`, `multiple`, `matching`.
- Tổng điểm phải bằng 100; tối đa 500 câu; ID câu hỏi và ID lựa chọn trong mỗi câu không trùng; đáp án đúng phải hợp lệ. Ảnh dùng đường dẫn dưới `/img/`; file ảnh vẫn đặt trong `wwwroot/img`.
- Tên và thời gian nhập trong form được ưu tiên khi tạo bản đề phục vụ thi.
- Xóa bài là xóa mềm. Bài đóng, bài đã xóa hoặc thuộc bộ phận đã xóa không được mở lượt thi mới. Lượt đã bắt đầu giữ bản sao đề và tiếp tục theo thời hạn cũ.

## Tích hợp với thi online

- Luồng thi hỗ trợ chọn bài bằng `examId`. Hạn thi và lịch sử được tách theo bài; cùng nhân viên không bị mở nhầm lượt đang làm của đề khác.
- `/admin/exams` hiển thị các đề đang mở trong các bộ phận được gán cho nhân viên qua `employee_department`. Chỉ tài khoản nhân viên đang làm việc, bộ phận và liên kết chưa xóa mới được chọn bài. Bài mới phải được nhập vào SQL Server; không tự mở đề file cũ cho người thi mới.
- Danh mục và nội dung đề mới lưu SQL Server; bài làm và kết quả vẫn lưu trong `App_Data/OnlineExams`. Chức năng này chưa chuyển lịch sử bài làm sang database.
- Quản lý nội dung và đáp án chỉ dành cho `ADMIN`; API người thi chỉ trả dữ liệu đề không có đáp án đúng. Server kiểm tra bộ phận khi xem chi tiết đề, bắt đầu, tiếp tục và lưu lượt chưa nộp; không bỏ qua kiểm tra này cho quản trị viên. Đổi ID trên URL không mở được đề ngoài bộ phận của nhân viên. Lượt đã nộp vẫn có thể xem theo quyền sở hữu dù phân công sau này thay đổi.
- Trước khi bắt đầu, chọn **Ngày thi** và tích hoặc bỏ tích **Nhân viên mới**. Ngày thi lưu riêng trong `ExamDate`, không thay đổi `StartedAt` (UTC, thời điểm tạo lượt thật). Không dùng ngày thi do người dùng chọn để tính hạn nộp.
- Hai thông tin trên lưu cùng lượt thi trong `App_Data/OnlineExams` và được hiển thị trong lịch sử/thông tin bài làm. Tiếp tục lượt đang làm không đổi ngày thi hoặc cờ nhân viên mới. Lượt cũ thiếu thông tin hiển thị “Chưa ghi nhận”. Không cần thay đổi bảng SQL vì lịch sử bài làm hiện vẫn lưu file.

## Mã nguồn và API

Logic quản lý tại `Repositories/DepartmentExamRepository.cs`, kiểm tra đề tại `Repositories/ExamDefinitionValidator.cs`. Controller inject `IDepartmentExamRepository`, DI scoped. `OnlineExamRepository` giữ singleton và tạo scope riêng khi lấy đề SQL Server, không giữ DbContext scoped trong singleton.

| Method | Route | Nội dung |
| --- | --- | --- |
| GET | `/api/department-exams?departmentId=...` | Danh sách; không gửi JSON đáp án trong danh sách |
| GET | `/api/department-exams/{id}` | Nội dung đầy đủ cho quản trị viên |
| POST | `/api/department-exams` | Tạo bài |
| PUT | `/api/department-exams/{id}` | Cập nhật |
| DELETE | `/api/department-exams/{id}` | Xóa mềm |

Các API quản lý yêu cầu `DEPARTMENT_EXAMS.VIEW`. Tạo/sửa/xóa yêu cầu thêm `DEPARTMENT_EXAMS.CREATE`, `DEPARTMENT_EXAMS.UPDATE`, `DEPARTMENT_EXAMS.DELETE` tương ứng; thao tác ghi dùng antiforgery token. Giao diện chỉ hiện nút thao tác được cấp quyền, dùng `_DataTable`, `_CompactInput`, `panel-card` và `JtechnUI` như các module hiện có; JavaScript đặt trong `Views/department-exams.js`.

Sau khi cài SQL, kiểm tra tạo hai bài cùng bộ phận, lọc danh sách, mã trùng, JSON sai, đóng/mở thi, sửa/xóa và làm bài qua ID trên database kiểm thử. Bộ kiểm tra tự động dùng repository giả cho nguồn SQL để kiểm tra tách lượt và snapshot; không thay thế kiểm thử tích hợp với SQL Server.
