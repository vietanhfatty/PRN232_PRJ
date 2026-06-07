# 🤖 AI Agent Coding Guide — Clean Architecture (.NET 8)

> **Dành cho:** `Project_PRN232` — Hospital Management System  
> **Nguyên tắc:** Plan trước → Code → Review sau mỗi task

---

## 📦 Cấu trúc Solution (thực tế)

```
Solution 'Project_PRN232'
├── Core & Logic
│   ├── MyProject.Domain
│   │   ├── Entities/              ← Plain C# entity classes
│   │   └── IRepositories/         ← Repository interfaces (ở ĐÂY, không phải Application)
│   └── MyProject.Application
│       ├── DTOs/                  ← Request / Response models
│       └── Services/              ← Business logic / Services
├── Data & Drivers
│   └── MyProject.Infrastructure
│       ├── HospitalManagementDbContext.cs  ← EF Core DbContext
│       └── Repositories/          ← Implement IRepositories từ Domain
└── Presentation Layers
    ├── MyProject.WebApi           ← REST API Controllers
    └── MyProject.WebMvc           ← Razor Pages / MVC Controllers
```

### ⚠️ Quy tắc quan trọng: IRepositories nằm trong Domain

> Đây là **Domain-Centric** variant của Clean Architecture.  
> Interface thuộc về Domain vì chúng là "cổng" mà Domain cần — Infrastructure chỉ là plugin.

### Dependency Rule (KHÔNG được vi phạm)

```
WebApi / WebMvc
       ↓
  Application
       ↓
    Domain  ←──── Infrastructure
                 (implements IRepositories từ Domain)
```

| Layer | Được phép reference | KHÔNG được reference |
|-------|--------------------|-----------------------|
| Domain | _(không ai)_ | Application, Infrastructure, EF Core |
| Application | Domain | Infrastructure, EF Core trực tiếp |
| Infrastructure | Domain, Application | WebApi, WebMvc |
| WebApi / WebMvc | Application, Infrastructure (DI only) | Domain trực tiếp |

---

## ⚙️ Quy trình thực thi Task

### Bước 1 — PLAN (bắt buộc trước khi code)

```
## 📋 PLAN: [Tên task]

**Mục tiêu:** [Mô tả ngắn gọn]

**Files sẽ tạo/sửa:**
- [ ] Domain/Entities/[Entity].cs              — Entity mới
- [ ] Domain/IRepositories/I[X]Repository.cs  — Interface (ở Domain!)
- [ ] Application/DTOs/[X]Dto.cs              — Request / Response DTO
- [ ] Application/UseCases/[X]Service.cs      — Business logic
- [ ] Infrastructure/Repositories/[X]Repository.cs  — EF Core impl
- [ ] Infrastructure/HospitalManagementDbContext.cs — thêm DbSet (nếu cần)
- [ ] WebApi/Controllers/[X]Controller.cs     — API endpoint
- [ ] WebApi/Program.cs                       — DI registration

**Không đụng đến:** [Liệt kê files không liên quan]
```

### Bước 2 — IMPLEMENT

Code theo thứ tự từ trong ra ngoài:

```
Domain/Entities  →  Domain/IRepositories  →  Application/DTOs
       →  Application/UseCases  →  Infrastructure  →  WebApi
```

### Bước 3 — REVIEW (bắt buộc sau khi code xong)

```
## ✅ REVIEW: [Tên task]

**Đã làm:**
- [x] Entity không có EF Core dependency
- [x] IRepository đặt đúng trong Domain/IRepositories
- [x] Service chỉ dùng IRepository, không gọi DbContext
- [x] Infrastructure implement đủ interface
- [x] DbSet đã thêm vào HospitalManagementDbContext
- [x] DI đã đăng ký trong Program.cs
- [x] Controller chỉ gọi Service, không có business logic

**Dependency check:**
- Domain references: [không có] ✓
- Application references: Domain ✓
- Infrastructure references: Domain + Application ✓
- WebApi references: Application ✓

**Tiếp theo (nếu có):**
- [ ] Validation
- [ ] Error handling
```

---

## 🧱 Code Templates

### 1. Domain — Entity

```csharp
// MyProject.Domain/Entities/Patient.cs
namespace MyProject.Domain.Entities;

public class Patient
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 2. Domain — IRepository Interface (⚠️ ở Domain, không phải Application)

```csharp
// MyProject.Domain/IRepositories/IPatientRepository.cs
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
}
```

### 3. Application — DTOs

```csharp
// MyProject.Application/DTOs/PatientDto.cs
namespace MyProject.Application.DTOs;

public record PatientDto(int Id, string FullName, DateTime DateOfBirth, string PhoneNumber);

public record CreatePatientRequest(string FullName, DateTime DateOfBirth, string PhoneNumber);

public record UpdatePatientRequest(string FullName, DateTime DateOfBirth, string PhoneNumber);
```

### 4. Application — Service (UseCase)

```csharp
// MyProject.Application/UseCases/PatientService.cs
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;  // ← reference Domain, không phải Application

namespace MyProject.Application.UseCases;

public class PatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo) => _repo = repo;

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(p => new PatientDto(p.Id, p.FullName, p.DateOfBirth, p.PhoneNumber));
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p is null ? null : new PatientDto(p.Id, p.FullName, p.DateOfBirth, p.PhoneNumber);
    }

    public async Task CreateAsync(CreatePatientRequest req)
    {
        var patient = new Patient
        {
            FullName = req.FullName,
            DateOfBirth = req.DateOfBirth,
            PhoneNumber = req.PhoneNumber
        };
        await _repo.AddAsync(patient);
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest req)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient {id} not found");
        patient.FullName = req.FullName;
        patient.DateOfBirth = req.DateOfBirth;
        patient.PhoneNumber = req.PhoneNumber;
        await _repo.UpdateAsync(patient);
    }

    public async Task DeleteAsync(int id) => await _repo.DeleteAsync(id);
}
```

### 5. Infrastructure — DbContext

```csharp
// MyProject.Infrastructure/HospitalManagementDbContext.cs
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure;

public class HospitalManagementDbContext : DbContext
{
    public HospitalManagementDbContext(DbContextOptions<HospitalManagementDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    // Thêm DbSet khác ở đây khi có entity mới
}
```

### 6. Infrastructure — Repository Implementation

```csharp
// MyProject.Infrastructure/Repositories/PatientRepository.cs
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;  // ← implement interface từ Domain

namespace MyProject.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly HospitalManagementDbContext _db;

    public PatientRepository(HospitalManagementDbContext db) => _db = db;

    public async Task<IEnumerable<Patient>> GetAllAsync()
        => await _db.Patients.ToListAsync();

    public async Task<Patient?> GetByIdAsync(int id)
        => await _db.Patients.FindAsync(id);

    public async Task AddAsync(Patient patient)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient patient)
    {
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Patients.FindAsync(id);
        if (entity is not null)
        {
            _db.Patients.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
```

### 7. WebApi — Controller

```csharp
// MyProject.WebApi/Controllers/PatientsController.cs
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.UseCases;

namespace MyProject.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly PatientService _service;

    public PatientsController(PatientService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        await _service.CreateAsync(request);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

### 8. DI Registration — Program.cs

```csharp
// MyProject.WebApi/Program.cs
using MyProject.Domain.IRepositories;
using MyProject.Infrastructure;
using MyProject.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<HospitalManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repositories (Domain interface ← Infrastructure impl)
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// Services (Application layer)
builder.Services.AddScoped<PatientService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

---

## 🚫 Những lỗi thường gặp — Agent KHÔNG được làm

| ❌ Sai | ✅ Đúng |
|--------|---------|
| Đặt `IXxxRepository` trong `Application/Interfaces` | Đặt trong `Domain/IRepositories` |
| Domain reference đến EF Core / DbContext | Domain chỉ chứa plain C# class |
| Application inject `HospitalManagementDbContext` trực tiếp | Application chỉ dùng `IXxxRepository` |
| Controller chứa business logic (`if/else`, mapping) | Logic nằm trong Service |
| Tạo DbContext mới bằng `new` | Luôn dùng DI |
| Quên thêm `DbSet` vào `HospitalManagementDbContext` | Thêm DbSet khi có entity mới |
| Infrastructure reference WebApi | Infrastructure không biết Presentation tồn tại |

---

## 📝 Ví dụ Task Hoàn Chỉnh — Thêm Doctor Management

```
## 📋 PLAN: Thêm Doctor Management

**Mục tiêu:** CRUD cơ bản cho bác sĩ trong hospital system

**Files sẽ tạo:**
- [ ] Domain/Entities/Doctor.cs
- [ ] Domain/IRepositories/IDoctorRepository.cs    ← interface ở Domain
- [ ] Application/DTOs/DoctorDto.cs
- [ ] Application/UseCases/DoctorService.cs
- [ ] Infrastructure/Repositories/DoctorRepository.cs
- [ ] Infrastructure/HospitalManagementDbContext.cs  (thêm DbSet<Doctor>)
- [ ] WebApi/Controllers/DoctorsController.cs
- [ ] WebApi/Program.cs  (thêm 2 dòng DI)

**Không đụng đến:** Patient files, Appointment files
```

---

## 💡 Checklist trước khi submit PR

- [ ] `IXxxRepository` nằm trong `Domain/IRepositories/`, không phải Application
- [ ] Domain không có `using Microsoft.EntityFrameworkCore`
- [ ] Application không inject `HospitalManagementDbContext`
- [ ] Mỗi entity mới có `DbSet` trong `HospitalManagementDbContext`
- [ ] Controller không có business logic
- [ ] DI đăng ký đủ cả Interface → Implementation và Service
- [ ] Naming: `IXxxRepository` (Domain) / `XxxRepository` (Infra) / `XxxService` (App)
