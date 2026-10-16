# JtechnApi

Hướng dẫn dành cho AI agent: [AGENTS.md](AGENTS.md).

Ứng dụng ASP.NET Core MVC + API, tổ chức theo module nghiệp vụ trong một project.

```text
JtechnApi/
├── Configuration/              # Đăng ký dependency injection
│   ├── InfrastructureServiceExtensions.cs
│   ├── ModuleServiceExtensions.cs
│   └── WebServiceExtensions.cs
├── Modules/                    # Chức năng theo nghiệp vụ
│   ├── Accessorys/
│   ├── Departments/
│   ├── Employees/
│   ├── Exams/
│   └── Users/
├── Infrastructure/
│   ├── Caching/                # Redis
│   ├── HostedServices/         # Tác vụ nền
│   ├── Middleware/             # Xử lý exception
│   └── Persistence/
│       ├── AdoHelper/
│       ├── BaseRepository/     # EF contexts và repository dùng chung
│       ├── Connects/           # Kết nối SQL Server, Oracle
│       └── SqlServerHelper/
├── Common/                     # Phân trang, validation, tiện ích, settings
├── Controllers/                # Controller dùng chung và tích hợp
├── ViewModels/                 # Model giao diện dùng chung
├── Views/                      # View và layout dùng chung
├── wwwroot/                    # Tài nguyên tĩnh
├── Properties/                 # Launch settings và publish profiles
├── Program.cs                  # Host và logging
├── Startup.cs                  # Điều phối DI và HTTP pipeline
└── JtechnApi.csproj
```

## Quy ước phát triển

- Mỗi module chứa `Controllers`, `Dtos`, `Models`, `Repositories`, `Views` theo nhu cầu; đăng ký dependency trong `<Module>Module.cs`.
- Khi thêm module, gọi phương thức đăng ký tại `Configuration/ModuleServiceExtensions.cs`.
- Đặt thành phần truy cập dữ liệu, cache và tích hợp hệ thống trong `Infrastructure`; tiện ích dùng chung trong `Common`.
- View riêng của module được controller tham chiếu bằng đường dẫn `~/Modules/<Module>/Views/<View>.cshtml`. Layout chung tiếp tục nằm trong `Views/Shared`.
- Namespace cũ (`JtechnApi.Employees`, `JtechnApi.Shares`, `JtechnApi.Infra`, ...) được giữ để tương thích. Đường dẫn thư mục mới không bắt buộc trùng namespace cũ.
- `bin`, `obj`, `publish`, `Logs` là đầu ra build hoặc runtime, không phải nơi đặt mã nguồn.

## Build và chạy

Chạy từ thư mục gốc repository:

```powershell
dotnet restore JtechnApi/JtechnApi.csproj
dotnet build JtechnApi/JtechnApi.csproj --no-restore
dotnet run --project JtechnApi/JtechnApi.csproj
```

Project giữ nguyên target `net6.0` và package versions. Cần cấu hình kết nối trong cấu hình môi trường trước khi chạy; MySQL sử dụng `ServerVersion.AutoDetect` khi khởi tạo và cần truy cập được server. Chức năng thực tế còn phụ thuộc SQL Server, Oracle, Redis và các dịch vụ tích hợp được sử dụng.
