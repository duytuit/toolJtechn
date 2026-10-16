# Hướng dẫn cho AI agent

## Phạm vi và cách làm việc

- Tài liệu áp dụng cho `JtechnApi/` và các thư mục con. Đọc thêm `AGENTS.md` trong thư mục con nếu có trước khi sửa mã tại đó.
- Đọc `README.md`, kiểm tra `git status --short` và các file liên quan trước khi thay đổi. Giữ nguyên công việc đang có của người dùng.
- Mặc định trao đổi và báo cáo bằng tiếng Việt. Thực hiện thay đổi đúng phạm vi yêu cầu, theo phong cách mã xung quanh.
- Không tự nâng target framework, SDK, package hoặc chuyển sang kiến trúc nhiều project khi nhiệm vụ không yêu cầu.

## Kiến trúc và nơi đặt mã

Đây là một project ASP.NET Core MVC + API, target `net6.0`, dùng `Program` và `Startup`.

| Vị trí | Trách nhiệm |
| --- | --- |
| `Program.cs` | Khởi tạo host và Serilog |
| `Startup.cs` | Điều phối đăng ký dịch vụ và thứ tự HTTP middleware |
| `Configuration/WebServiceExtensions.cs` | Cookie authentication, authorization, CORS, MVC, validation, HTTP client, compression |
| `Configuration/InfrastructureServiceExtensions.cs` | Cấu hình kết nối, EF contexts, Redis multiplexer, Oracle |
| `Configuration/ModuleServiceExtensions.cs` | Gọi đăng ký các module nghiệp vụ |
| `Modules/Accessorys`, `Departments`, `Employees`, `Exams`, `Users` | Controller, DTO, model, repository, view và đăng ký DI theo nghiệp vụ |
| `Infrastructure/Persistence` | EF contexts, repository dùng chung, ADO và connection factories |
| `Infrastructure/Caching` | Redis service |
| `Infrastructure/Middleware` | Exception middleware |
| `Infrastructure/HostedServices` | Mã tác vụ nền; file tồn tại không có nghĩa đã được đăng ký chạy |
| `Common` | Settings, phân trang, validation và tiện ích dùng chung |
| `Controllers`, `ViewModels`, `Views` | Controller, model giao diện, view và layout dùng chung |
| `wwwroot` | CSS, JavaScript, ảnh và tài nguyên tĩnh |

Namespace cũ được giữ sau khi sắp xếp thư mục. Ví dụ, code trong `Modules/Employees` vẫn dùng `JtechnApi.Employees`; code trong `Infrastructure/Persistence` có thể dùng `JtechnApi.Shares.*`; exception middleware dùng `JtechnApi.Infra`. Không tự đổi namespace hàng loạt theo đường dẫn mới. Khi thêm file, dùng namespace phù hợp với các file cùng thành phần.

## Thay đổi module và giao diện

- Tìm luồng hiện có từ route/controller tới DTO, repository hoặc DbContext, rồi tới view/JavaScript gọi API trước khi sửa.
- Đặt tính năng riêng trong module tương ứng. Không tạo repository/service trung gian chỉ để bọc lại một lời gọi nếu không có nhu cầu cụ thể.
- Đăng ký dependency của module tại `<Module>Module.cs`; thêm module mới vào `Configuration/ModuleServiceExtensions.cs`. Repository và DbContext thường dùng scoped lifetime; không inject trực tiếp chúng vào singleton.
- Giữ hợp đồng API hiện có: route, HTTP method, tên trường JSON, status code, quy tắc validation và phân quyền, trừ khi yêu cầu thay đổi chúng.
- `Employees` có cả controller MVC và API cùng tên `EmployeeController`, phân biệt bằng namespace. Kiểm tra đúng controller cần sửa.
- View module được tham chiếu bằng `~/Modules/<Module>/Views/<View>.cshtml`. Khi di chuyển view, cập nhật mọi tham chiếu và kiểm tra layout, partial, tag helper. Layout chung nằm trong `Views/Shared`.
- Không giả định module tự kế thừa `_ViewImports.cshtml` hoặc `_ViewStart.cshtml` trong `Views/`: kiểm tra vị trí và phạm vi kế thừa Razor thực tế.

## Dữ liệu, cấu hình và quyền truy cập

- `DBJtecContext` dùng SQL Server; `DBContext` dùng MySQL. Oracle sử dụng `OracleConnectionFactory`. Xác định đúng nguồn dữ liệu từ mã hiện có trước khi thay đổi truy vấn.
- MySQL dùng `ServerVersion.AutoDetect`, có thể kết nối server trong lúc khởi tạo. Redis và các thao tác nghiệp vụ cũng có thể cần dịch vụ thật. Build thành công không chứng minh ứng dụng chạy thành công.
- Dùng truy vấn có tham số cho SQL/ADO; không ghép dữ liệu đầu vào trực tiếp vào câu SQL. Giữ cách quản lý connection, transaction và disposal đúng với thành phần đang sửa.
- Không tự chạy migration, cập nhật schema hoặc ghi dữ liệu trên môi trường thật chỉ để xác minh mã. Chỉ thực hiện khi nằm trong yêu cầu đã được cho phép.
- Không đưa connection string, mật khẩu, token hoặc dữ liệu cá nhân vào tài liệu, log hay báo cáo. Dùng giá trị mẫu khi cần minh họa.
- Giữ cơ chế cookie authentication và policy dạng `Permission:<CODE>` trong `Modules/Users/Authorization`. Khi thêm endpoint, áp dụng quyền phù hợp với nghiệp vụ và cách hiện có.
- API dưới `/api` đang trả `401`/`403` khi xác thực hoặc quyền truy cập thất bại thay vì redirect về trang đăng nhập. Giữ hành vi này khi sửa authentication.
- `ValidateModelFilter` được đăng ký toàn cục, còn bộ lọc ModelState mặc định của API bị tắt. Kiểm tra cơ chế này trước khi sửa validation.
- Không thay đổi thứ tự middleware hoặc tự kích hoạt hosted service nếu không thuộc nhiệm vụ; các thay đổi đó có thể ảnh hưởng toàn ứng dụng.

## Build và xác minh

Chạy các lệnh sau từ thư mục gốc repository (`D:\dev_web\toolJtechn`):

```powershell
dotnet restore JtechnApi/JtechnApi.csproj
dotnet build JtechnApi/JtechnApi.csproj --no-restore
git diff --check
```

- Nếu package đã restore và không thay đổi dependency, có thể chỉ chạy build với `--no-restore`.
- Kiểm tra `JtechnApi/global.json` và `JtechnApi.csproj` khi gặp lỗi SDK. `global.json` nằm trong thư mục project; SDK được chọn có thể khác tùy thư mục chạy lệnh. Không sửa pin SDK chỉ để bỏ qua lỗi môi trường.
- Chưa có project test chuyên biệt cho `JtechnApi` được xác định trong repository. Không xem các project tên `Test*` khác là bộ test của API này nếu chưa kiểm tra.
- Với thay đổi logic, chạy hoặc bổ sung kiểm tra có ý nghĩa cho hành vi thay đổi khi phù hợp. Với thay đổi giao diện, kiểm tra trang và lời gọi API liên quan nếu có môi trường chạy.
- Chỉ chạy ứng dụng khi có cấu hình môi trường phù hợp: `dotnet run --project JtechnApi/JtechnApi.csproj`. Không dùng dữ liệu thật để thử thao tác tạo/sửa/xóa ngoài phạm vi được giao.
- Phân biệt lỗi mã với lỗi thiếu SDK, package, kết nối hoặc cấu hình. Báo rõ bước chưa kiểm tra và nguyên nhân; không tuyên bố đã kiểm thử runtime nếu chỉ build.

## File cần giữ và cách kết thúc

- Không sửa thủ công đầu ra `bin/`, `obj/`, `publish/`, `Logs/`.
- Không chỉnh publish profiles, file `*.user`, cấu hình triển khai hoặc project khác ngoài `JtechnApi` khi không liên quan đến yêu cầu.
- Giữ encoding của file hiện có; đọc/ghi tiếng Việt bằng UTF-8 phù hợp. Nếu terminal hiển thị lỗi dấu, xác minh encoding trước khi chuyển đổi nội dung hàng loạt.
- Cập nhật `README.md` khi thay đổi cấu trúc, lệnh chạy hoặc quy ước phát triển; cập nhật tài liệu này khi hướng dẫn cho agent không còn khớp mã nguồn.
- Trước khi hoàn tất, xem diff để phát hiện thay đổi ngoài ý muốn. Báo ngắn gọn: đã thay đổi gì, kiểm tra nào đã chạy, kết quả và giới hạn còn lại.
