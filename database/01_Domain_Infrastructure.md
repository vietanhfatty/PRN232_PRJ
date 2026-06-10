# Task 1: Domain & Infrastructure Layers

Mục tiêu của task này là xây dựng các Interface Repository trong Layer Domain và các Implementation tương ứng trong Layer Infrastructure cho hai thực thể `MedicalRecord` và `Prescription`.

---

## 📋 PLAN: Domain & Infrastructure Repositories

### 1. Domain (Không phụ thuộc EF Core)

#### [NEW] [IMedicalRecordRepository.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Domain/IRepositories/IMedicalRecordRepository.cs)
Định nghĩa interface để truy xuất thông tin Hồ sơ bệnh án.
- **Namespace:** `MyProject.Domain.IRepositories`
- **Các phương thức:**
  ```csharp
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using MyProject.Domain.Entities;

  namespace MyProject.Domain.IRepositories;

  public interface IMedicalRecordRepository
  {
      Task<IEnumerable<MedicalRecord>> GetAllAsync();
      Task<MedicalRecord?> GetByIdAsync(int id);
      Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);
      Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
      Task AddAsync(MedicalRecord medicalRecord);
      Task UpdateAsync(MedicalRecord medicalRecord);
  }
  ```

#### [NEW] [IPrescriptionRepository.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Domain/IRepositories/IPrescriptionRepository.cs)
Định nghĩa interface để quản lý Đơn thuốc kèm theo Hồ sơ bệnh án.
- **Namespace:** `MyProject.Domain.IRepositories`
- **Các phương thức:**
  ```csharp
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using MyProject.Domain.Entities;

  namespace MyProject.Domain.IRepositories;

  public interface IPrescriptionRepository
  {
      Task<IEnumerable<Prescription>> GetByMedicalRecordIdAsync(int medicalRecordId);
      Task AddAsync(Prescription prescription);
      Task DeleteAsync(int id);
  }
  ```

---

### 2. Infrastructure (Hiện thực hóa các Repository)

#### [NEW] [MedicalRecordRepository.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Infrastructure/Repositories/MedicalRecordRepository.cs)
Hiện thực `IMedicalRecordRepository` sử dụng EF Core và DbContext hiện tại.
- **Namespace:** `MyProject.Infrastructure.Repositories`
- **Yêu cầu quan trọng:** Cần Include đầy đủ thông tin:
  - `Appointment` -> `Patient` -> `User`
  - `Appointment` -> `Doctor` -> `User`
  - `Prescriptions`
- **Code Template:**
  ```csharp
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using MyProject.Domain.Entities;
  using MyProject.Domain.IRepositories;

  namespace MyProject.Infrastructure.Repositories;

  public class MedicalRecordRepository : IMedicalRecordRepository
  {
      private readonly HospitalManagementDbContext _context;

      public MedicalRecordRepository(HospitalManagementDbContext context)
      {
          _context = context;
      }

      public async Task<IEnumerable<MedicalRecord>> GetAllAsync()
      {
          return await _context.MedicalRecords
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Patient)
                      .ThenInclude(p => p.User)
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Doctor)
                      .ThenInclude(d => d.User)
              .Include(mr => mr.Prescriptions)
              .ToListAsync();
      }

      public async Task<MedicalRecord?> GetByIdAsync(int id)
      {
          return await _context.MedicalRecords
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Patient)
                      .ThenInclude(p => p.User)
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Doctor)
                      .ThenInclude(d => d.User)
              .Include(mr => mr.Prescriptions)
              .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);
      }

      public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
      {
          return await _context.MedicalRecords
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Patient)
                      .ThenInclude(p => p.User)
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Doctor)
                      .ThenInclude(d => d.User)
              .Include(mr => mr.Prescriptions)
              .FirstOrDefaultAsync(mr => mr.AppointmentId == appointmentId);
      }

      public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
      {
          return await _context.MedicalRecords
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Patient)
                      .ThenInclude(p => p.User)
              .Include(mr => mr.Appointment)
                  .ThenInclude(a => a.Doctor)
                      .ThenInclude(d => d.User)
              .Include(mr => mr.Prescriptions)
              .Where(mr => mr.Appointment.PatientId == patientId)
              .ToListAsync();
      }

      public async Task AddAsync(MedicalRecord medicalRecord)
      {
          _context.MedicalRecords.Add(medicalRecord);
          await _context.SaveChangesAsync();
      }

      public async Task UpdateAsync(MedicalRecord medicalRecord)
      {
          _context.Entry(medicalRecord).State = EntityState.Modified;
          await _context.SaveChangesAsync();
      }
  }
  ```

#### [NEW] [PrescriptionRepository.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Infrastructure/Repositories/PrescriptionRepository.cs)
Hiện thực `IPrescriptionRepository` sử dụng EF Core và DbContext.
- **Namespace:** `MyProject.Infrastructure.Repositories`
- **Code Template:**
  ```csharp
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using MyProject.Domain.Entities;
  using MyProject.Domain.IRepositories;

  namespace MyProject.Infrastructure.Repositories;

  public class PrescriptionRepository : IPrescriptionRepository
  {
      private readonly HospitalManagementDbContext _context;

      public PrescriptionRepository(HospitalManagementDbContext context)
      {
          _context = context;
      }

      public async Task<IEnumerable<Prescription>> GetByMedicalRecordIdAsync(int medicalRecordId)
      {
          return await _context.Prescriptions
              .Where(p => p.MedicalRecordId == medicalRecordId)
              .ToListAsync();
      }

      public async Task AddAsync(Prescription prescription)
      {
          _context.Prescriptions.Add(prescription);
          await _context.SaveChangesAsync();
      }

      public async Task DeleteAsync(int id)
      {
          var prescription = await _context.Prescriptions.FindAsync(id);
          if (prescription != null)
          {
              _context.Prescriptions.Remove(prescription);
              await _context.SaveChangesAsync();
          }
      }
  }
  ```

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Các Interface được định nghĩa trong thư mục `MyProject.Domain/IRepositories`.
- [ ] Layer `Domain` **không** sử dụng `Microsoft.EntityFrameworkCore` hoặc `HospitalManagementDbContext`.
- [ ] Các class Repository nằm trong `MyProject.Infrastructure/Repositories` và implement đầy đủ interface.
- [ ] Repository sử dụng `HospitalManagementDbContext` qua Dependency Injection (Constructor).
