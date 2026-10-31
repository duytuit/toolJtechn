# Thi trực tuyến

Quản lý nhiều bài thi theo bộ phận và lưu đề vào SQL Server: [cài đặt, SQL và API](DEPARTMENT_EXAMS.md).

Logic đề thi, lưu bài, thời gian và chấm điểm nằm trong `Repositories/OnlineExamRepository.cs`. Controller chỉ xử lý HTTP và gọi repository. Repository giữ singleton để đồng bộ ghi file giữa các request.

JavaScript nằm tại `Views/online-exams.js`, được controller phục vụ qua `/js/online-exams.js` cho người đã đăng nhập và được sao chép khi build/publish.

Đăng nhập tài khoản nhân viên rồi truy cập `/admin/exams` hoặc mục **Thi trực tuyến** trên thanh bên.

Trang hiển thị bài thi SQL Server đang mở thuộc các bộ phận đã gán cho nhân viên. Chọn đề, chọn **Ngày thi** và tích **Nhân viên mới** nếu phù hợp trước khi bắt đầu. Ngày thi được lưu riêng với thời điểm tạo lượt thi và không ảnh hưởng bộ đếm thời gian.

## Đề thi và chấm điểm

- Nội dung `options[].text` hỗ trợ HTML định dạng như `br`, `div`, `p`, chữ đậm/nghiêng, danh sách và bảng. Giao diện dựng lại các thẻ cho phép, bỏ script, sự kiện và thuộc tính không cần thiết; chỉ giữ thụt lề `margin-left` bằng pixel (tối đa 100px). Chấm điểm vẫn theo ID option, không theo HTML hiển thị.

- Đề công nhân Dập mới, mã 6, được nhập từ dữ liệu người dùng cung cấp: 53 câu, 9 nhóm, 100 điểm, 30 phút.
- Dữ liệu và đáp án nằm tại `Data/stamping-new.json`, phía ngoài `wwwroot`. Không công khai file này qua endpoint hoặc static files.
- Hỗ trợ chọn một, chọn nhiều và điền số ghép với hình (câu 14 và 21). Câu chọn nhiều và ghép số chỉ được điểm khi đúng toàn bộ; không chia điểm thành phần.
- Đạt từ 80; trên 60 đến dưới 80 kiểm tra lại; từ 50 đến 60 đào tạo lại; dưới 50 không đạt. Quy tắc này giải quyết phần mô tả gốc bị trùng mốc 80.
- Hiện dùng toàn bộ 53 câu theo thứ tự dữ liệu gốc; không xáo trộn hoặc chọn ngẫu nhiên như một số câu trong trang HTML mẫu.

## Bổ sung hình ảnh trước khi mở thi

Trong phần thêm/sửa bài thi tại `/admin/department-exams`, chọn file ảnh ngay dưới câu hỏi để tải và xem trước. API `POST /upload/single` nhận multipart `file`, `folder=exams` và token chống giả mạo; yêu cầu đăng nhập cùng quyền tạo hoặc sửa bài thi. Hỗ trợ JPG, JPEG, PNG, GIF, WEBP tối đa 10 MB, kiểm tra chữ ký định dạng. Ảnh lưu tại `wwwroot/img/exams/` với tên ngẫu nhiên; đường dẫn `/img/exams/...` được gán vào `questions[].image` và lưu trong `definitionJson` khi nhấn **Lưu bài thi**. Cần cấp quyền ghi thư mục này khi triển khai. Bỏ ảnh chỉ xóa đường dẫn khỏi câu hỏi; file đã tải vẫn được giữ.

Tệp đính kèm chỉ chứa đường dẫn ảnh, không chứa file ảnh. Sao chép ảnh gốc vào các vị trí tương ứng bên dưới `wwwroot/img/`, giữ nguyên tên và chữ hoa/thường:

```text
wwwroot/img/congdoandap/...
wwwroot/img/congdoandap2/...
```

Danh sách đường dẫn chính xác nằm ở trường `image` trong JSON đề thi. Trang danh sách hiển thị số ảnh còn thiếu. Server chặn tạo lượt mới khi chưa đủ ảnh để tránh mở một đề không thể làm đầy đủ. Không thay hình chuyên môn bằng hình minh họa tự tạo.

## Lưu bài và thời gian

- Mỗi tài khoản chỉ có một lượt chưa nộp; bấm bắt đầu sẽ tiếp tục lượt đó. Sau khi nộp có thể thi lượt mới.
- Đáp án tự lưu khi đổi lựa chọn và mỗi 5 giây khi có thay đổi. Trạng thái lưu hiển thị trên giao diện. Đóng trang khi chưa lưu sẽ có cảnh báo.
- Server quyết định hạn nộp bằng UTC. Tải lại hoặc đóng trang không đặt lại thời gian. Sau hạn, chỉ chấm đáp án đã tới server trước hạn; không nhận thay đổi muộn. Lượt hết hạn được chấm khi có yêu cầu lưu/nộp hoặc mở lại lượt đó.
- Nộp lại một lượt đã nộp trả về kết quả đã lưu, không chấm lại. Mỗi lượt giữ bản sao đề để chỉnh đề sau này không thay đổi lượt cũ.
- Lượt thi, tiến độ, đáp án và kết quả được lưu trong SQL Server `dbo.online_exam_attempt`; snapshot JSON giữ nguyên đề và câu trả lời để tiếp tục/nộp bài. `results` là số câu đúng, `scores` là điểm, `status` là BIT (1 đạt, 0 không đạt, NULL chưa nộp), `counting_time` tính bằng giây, `limit_time` tính bằng phút và `fail_aws` là JSON câu sai cùng câu trả lời. `cycle_name` được lưu theo tháng/năm của ngày thi (`create_date`), định dạng `MMyyyy` (ví dụ `092026`).
- Chạy `Sql/002_online_exam_attempt.sql` khi tạo bảng mới. Nếu đã chạy script bảng cũ `002_online_exam_attempt.sql`, chạy thêm `Sql/003_online_exam_attempt_storage.sql` để bổ sung khóa và snapshot cần cho lưu/đọc từ database. Các lượt cũ chỉ có trong `App_Data/OnlineExams` không tự động nhập vào SQL Server.
- Chưa đồng bộ vào bảng MySQL `exams` cũ và chưa có màn hình quản lý kết quả của toàn bộ nhân viên. API `GET /Exam` hiện có được giữ nguyên.

## API

Tất cả endpoint mới yêu cầu đăng nhập; chỉ xem/sửa lượt thuộc tài khoản hiện tại. Các POST dùng antiforgery token từ trang Razor. Không tự cấp quyền quản lý hoặc thay đổi policy hiện có.

## Kết quả và thống kê

Menu **Test Kỹ Năng** gồm Thi trực tuyến, Bài thi theo bộ phận, Kết quả bài test và Thống kê. Kết quả hiển thị lượt đã nộp theo trang; thống kê gồm danh sách câu trả lời sai và top 10 câu hỏi bị trả lời sai nhiều nhất. Hai trang yêu cầu quyền `TEST_RESULTS.VIEW` và `TEST_STATISTICS.VIEW` tương ứng. Thống kê tổng hợp các lượt đang lưu trong SQL Server; dữ liệu bài thi cũ chỉ lưu trong file JSON không xuất hiện.

| Method | Đường dẫn | Nội dung |
| --- | --- | --- |
| GET | `/api/exam-results/filters` | Danh sách bộ phận và bài thi cho bộ lọc kết quả |
| GET | `/api/exam-results?page=1&pageSize=20&deptId=&examId=&search=` | Danh sách kết quả đã nộp, lọc bộ phận/bài thi và phân trang |
| GET | `/api/exam-results/export?deptId=&examId=&search=` | Xuất Excel toàn bộ kết quả phù hợp với bộ lọc hiện tại |
| GET | `/api/exam-results/quick-statistics?deptId=&examId=` | Thống kê nhanh hai đợt trong tháng hiện tại (ngày 1–15 và ngày 16–cuối tháng) |
| GET | `/api/exam-results/quick-statistics/export?period=1&category=passed&deptId=&examId=` | Xuất Excel theo đợt và nhóm thống kê (`total`, `passed`, `failed`, `retake`, `retrain`, `notTaken`) |
| DELETE | `/api/exam-results/{id}` | Xóa mềm một kết quả đã nộp; yêu cầu `TEST_RESULTS.DELETE` |
| GET | `/api/exam-results/statistics?page=1&pageSize=20&search=` | Thống kê, trang câu trả lời sai và top 10 câu sai |

Thống kê nhanh đếm nhân viên đang hoạt động có bộ phận được gán; khi lọc bài thi, bộ phận của bài thi được dùng nếu chưa chọn bộ phận. Mỗi nhân viên được xếp theo lượt nộp gần nhất trong từng đợt; nhân viên chưa có lượt nộp trong khoảng ngày đó nằm trong nhóm **Chưa thi**. Các số liệu có thể bấm để xuất danh sách Excel tương ứng.

Danh sách nhân viên/câu hỏi trả lời sai chỉ trả về một trang mỗi request; có thể tìm theo tên/mã nhân viên, bài thi, câu hỏi và nội dung trả lời. `_DataTable` yêu cầu trang tiếp theo từ API thay vì phân trang toàn bộ kết quả tại trình duyệt.

| Method | Đường dẫn | Nội dung |
| --- | --- | --- |
| GET | `/api/online-exams` | Thông tin đề, ảnh thiếu và lịch sử cá nhân |
| POST | `/api/online-exams/start` | Bắt đầu hoặc tiếp tục |
| GET | `/api/online-exams/attempts/{id}` | Đề và đáp án đã lưu của người thi |
| POST | `/api/online-exams/attempts/{id}/save` | Lưu tiến độ |
| POST | `/api/online-exams/attempts/{id}/submit` | Chấm và lưu kết quả |

Body lưu/nộp: `{"answers":{"1":["0","1","2"],"2":["2"]}}`. Với câu chọn một/nhiều, giá trị là ID option; với ghép số, mảng chứa số nhập tương ứng thứ tự option. Không nhận điểm hoặc đáp án chuẩn từ client.

## Kiểm tra

Từ thư mục gốc repository:

```powershell
dotnet build JtechnApi/JtechnApi.csproj --no-restore
dotnet run --project tests/JtechnApi.OnlineExams.Checks/JtechnApi.OnlineExams.Checks.csproj
node --check JtechnApi/Modules/Exams/Views/online-exams.js
```

Bộ kiểm tra dùng dữ liệu tạm, không kết nối database hoặc sửa bài thi thật. Bao gồm tính đầy đủ của đề, chấm điểm, mốc phân loại, quyền sở hữu, lưu qua lần khởi tạo service, bắt đầu đồng thời, nộp lặp và đáp án đến trễ. Cần kiểm thử thêm trên trình duyệt với đăng nhập và ảnh thật trước khi đưa vào sử dụng.

## Mốc điểm theo đề

JSON hỗ trợ `"scores": [96, 90]`: từ 96 đạt, từ 90 đến dưới 96 kiểm tra lại, dưới 90 không đạt (đào tạo lại). Hai mốc dùng điểm thực tế, kể cả phần thập phân; phải thỏa `0 ≤ mốc kiểm tra lại < mốc đạt ≤ 100`.

Trong form quản lý bài thi theo bộ phận, mục **Mốc điểm phân loại** cho phép sửa hai giá trị này. Import JSON, lấy JSON từ form và lưu đề đều giữ cấu hình. Bỏ chọn cấu hình riêng để dùng quy tắc cũ (80 / trên 60 / 50). Đề cũ thiếu `scores` tiếp tục dùng quy tắc cũ. Kết quả và lịch sử lấy cấu hình từ bản đề được lưu trong từng lượt thi.

Hai file mẫu: `stamping-new.json` dùng `[80, 60]`, `cong_nhan_moi_vao_cam.json` dùng `[96, 90]`. File mẫu không tự cập nhật đề đã import vào SQL Server; cần import và lưu lại từ giao diện. Đề Cắm gốc có tổng 99.825 điểm; cần điều chỉnh điểm câu hỏi thành tổng 100 trước khi lưu.
