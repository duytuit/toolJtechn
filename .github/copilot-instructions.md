# AI Coding Instructions for toolJtechn

## Architecture Overview

This is a .NET solution comprising multiple interconnected applications for industrial/manufacturing operations:

- **JtechnApi**: Main ASP.NET Core 3.1 API with modular architecture (Accessorys, Departments, ProductionPlans, etc.)
- **Windows Forms Applications**: Desktop tools for specific functions (pdfOCR, LampWarningAgvDap, NotifycationApp, etc.)
- **WebSocket Services**: Real-time communication via WSChat backend components
- **Shared Libraries**: Reusable components in Library/ and websocket-sharp/

## Key Patterns & Conventions

### API Architecture
- **Modular Design**: Each domain (Accessorys, Umesens) has its own module with Controllers/, Dtos/, Models/, Repositories/
- **Dependency Injection**: Services registered via extension methods (e.g., `AddAccessorysModule()` in [JtechnApi/Startup.cs](JtechnApi/Startup.cs))
- **Base Repository Pattern**: All repositories inherit from `BaseRepository<T>` in [JtechnApi/Shares/BaseRepository/](JtechnApi/Shares/BaseRepository/) using Entity Framework Core
- **Multi-Database Support**: Simultaneous connections to MySQL (EF), Oracle (Dapper), SQL Server, and Redis
- **Logging**: Serilog with daily file rotation configured in [JtechnApi/Program.cs](JtechnApi/Program.cs)

### Data Access
- **Entity Framework**: Primary ORM for MySQL with retry logic and query tracking disabled
- **Dapper**: Used for Oracle connections via `OracleConnection` factory
- **Redis**: Singleton `IConnectionMultiplexer` for caching and pub/sub
- **Pagination**: Standard `PaginatedResult<T>` pattern across all list endpoints

### Real-Time Communication
- **WebSocket Clients**: Custom implementation with auto-reconnection and ping/pong in [NotifycationApp/WebSocketClient.cs](NotifycationApp/WebSocketClient.cs)
- **Server Components**: Modular WebSocket backend in Websocket/ directory

### Desktop Applications
- **OCR Processing**: Hybrid Tesseract + PaddleOCR with ONNX runtime in [pdfOCR/Form2.cs](pdfOCR/Form2.cs)
- **Image Processing**: SixLabors.ImageSharp for PDF-to-image conversion in pdftopng/
- **UI Patterns**: Standard Windows Forms with async data loading

## Build & Development Workflow

### Building the Solution
```bash
# Build entire solution
dotnet build toolJtechn.sln

# Or use MSBuild for .NET Framework projects
msbuild toolJtechn.sln /p:Configuration=Release
```

### Running APIs
```bash
# JtechnApi
cd JtechnApi
dotnet run
```

### Database Setup
- Configure connection strings in `appsettings.json` for MySQL, Oracle, Redis
- Ensure Oracle client libraries are installed for Oracle.ManagedDataAccess.Core
- MySQL uses Pomelo.EntityFrameworkCore.MySql with retry configuration

### External Dependencies
- **Tesseract Data**: Place `eng.traineddata` in `tessdata/` folder for OCR apps
- **PaddleOCR Models**: ONNX models in `Model/` directory for pdfOCR
- **Oracle Client**: Requires appropriate Oracle instant client for target platform

## Code Patterns

### Controller Structure
```csharp
[ApiController]
[Route("[controller]")]
public class AccessoryController : ControllerBase
{
    private readonly IAccessoryRepository repo;
    
    public AccessoryController(ConnectionStrings con, IAccessoryRepository repo)
    {
        // Constructor injection
    }
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await repo.GetPaginatedAsync(page, pageSize);
        return Ok(result);
    }
}
```

### Repository Pattern
```csharp
public class AccessoryRepository : BaseRepository<Accessory>, IAccessoryRepository
{
    public async Task<PaginatedResult<Accessory>> GetPaginatedAsync(int page, int pageSize)
    {
        var totalItems = await _context.Accessory.CountAsync();
        var items = await _context.Accessory
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PaginatedResult<Accessory> { /* ... */ };
    }
}
```

### Module Registration
```csharp
public static class AccessorysModule
{
    public static IServiceCollection AddAccessorysModule(this IServiceCollection services)
    {
        services.AddScoped<IAccessoryRepository, AccessoryRepository>();
        return services;
    }
}
```

## Integration Points

- **CORS**: Fully open policy for cross-origin requests
- **Exception Handling**: Custom `ExceptionMiddleware` for unified error responses
- **File Upload**: Support for various file types in UploadDatas/ and UploadKTNQ/
- **Excel Processing**: EPPlus for report generation and data import

## Common Gotchas

- **Database Connections**: Always use injected `ConnectionStrings` and `DBContext` rather than creating new instances
- **Async/Await**: All data operations are async; UI threads in Windows Forms must handle threading properly
- **Model Validation**: Custom `ValidateModelFilter` applied globally to all controllers
- **Path Handling**: Use `AppDomain.CurrentDomain.BaseDirectory` for relative paths in desktop apps</content>
<parameter name="filePath">d:\dev_web\toolJtechn\.github\copilot-instructions.md