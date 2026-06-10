# Task 2: Application Layer (DTOs & Services)

Task này bao gồm việc định nghĩa các Data Transfer Objects (DTO) và xây dựng logic xử lý nghiệp vụ cho MedicalRecord, đồng thời cập nhật dịch vụ Appointment hiện có để bổ sung luồng kiểm tra (Status Flow).

---

## 📋 PLAN: DTOs & Services

### 1. Data Transfer Objects (DTOs)

#### [NEW] [MedicalRecordDto.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Application/DTOs/MedicalRecordDto.cs)
- Chứa thông tin đầy đủ về Hồ sơ bệnh án và Đơn thuốc.
- **Namespace:** `MyProject.Application.DTOs`
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;

  namespace MyProject.Application.DTOs;

  public record MedicalRecordDto(
      int MedicalRecordId,
      int AppointmentId,
      string PatientName,
      string DoctorName,
      string? Symptoms,
      string? Diagnosis,
      string? Treatment,
      string? Notes,
      DateTime CreatedAt,
      List<PrescriptionDto> Prescriptions
  );

  public record CreateMedicalRecordRequest(
      int AppointmentId,
      string? Symptoms,
      string? Diagnosis,
      string? Treatment,
      string? Notes
  );

  public record PrescriptionDto(
      int PrescriptionId,
      int MedicalRecordId,
      string MedicineName,
      string? Dosage,
      int? Quantity,
      string? Instruction
  );

  public record CreatePrescriptionRequest(
      string MedicineName,
      string? Dosage,
      int? Quantity,
      string? Instruction
  );
  ```

---

### 2. Business Services

#### [NEW] [MedicalRecordService.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Application/Services/MedicalRecordService.cs)
Cung cấp business logic cho `MedicalRecord` và `Prescription`.
- **Namespace:** `MyProject.Application.Services`
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using MyProject.Application.DTOs;
  using MyProject.Domain.Entities;
  using MyProject.Domain.IRepositories;

  namespace MyProject.Application.Services;

  public class MedicalRecordService
  {
      private readonly IMedicalRecordRepository _medicalRecordRepo;
      private readonly IPrescriptionRepository _prescriptionRepo;

      public MedicalRecordService(
          IMedicalRecordRepository medicalRecordRepo,
          IPrescriptionRepository prescriptionRepo)
      {
          _medicalRecordRepo = medicalRecordRepo;
          _prescriptionRepo = prescriptionRepo;
      }

      public async Task<IEnumerable<MedicalRecordDto>> GetAllAsync()
      {
          var list = await _medicalRecordRepo.GetAllAsync();
          return list.Select(MapToDto);
      }

      public async Task<MedicalRecordDto?> GetByIdAsync(int id)
      {
          var record = await _medicalRecordRepo.GetByIdAsync(id);
          return record == null ? null : MapToDto(record);
      }

      public async Task<MedicalRecordDto?> GetByAppointmentIdAsync(int appointmentId)
      {
          var record = await _medicalRecordRepo.GetByAppointmentIdAsync(appointmentId);
          return record == null ? null : MapToDto(record);
      }

      public async Task<IEnumerable<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
      {
          var list = await _medicalRecordRepo.GetByPatientIdAsync(patientId);
          return list.Select(MapToDto);
      }

      public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordRequest req)
      {
          var record = new MedicalRecord
          {
              AppointmentId = req.AppointmentId,
              Symptoms = req.Symptoms?.Trim(),
              Diagnosis = req.Diagnosis?.Trim(),
              Treatment = req.Treatment?.Trim(),
              Notes = req.Notes?.Trim(),
              CreatedAt = DateTime.UtcNow
          };

          await _medicalRecordRepo.AddAsync(record);

          // Lấy lại record kèm theo các Entity quan hệ (Patient, Doctor, v.v...)
          var addedRecord = await _medicalRecordRepo.GetByIdAsync(record.MedicalRecordId);
          return MapToDto(addedRecord!);
      }

      public async Task AddPrescriptionsAsync(int medicalRecordId, List<CreatePrescriptionRequest> reqs)
      {
          foreach (var req in reqs)
          {
              var prescription = new Prescription
              {
                  MedicalRecordId = medicalRecordId,
                  MedicineName = req.MedicineName.Trim(),
                  Dosage = req.Dosage?.Trim(),
                  Quantity = req.Quantity,
                  Instruction = req.Instruction?.Trim()
              };
              await _prescriptionRepo.AddAsync(prescription);
          }
      }

      private MedicalRecordDto MapToDto(MedicalRecord mr)
      {
          var patientName = mr.Appointment?.Patient?.User?.FullName ?? "Unknown Patient";
          var doctorName = mr.Appointment?.Doctor?.User?.FullName ?? "Unknown Doctor";

          var prescriptions = mr.Prescriptions.Select(p => new PrescriptionDto(
              p.PrescriptionId,
              p.MedicalRecordId,
              p.MedicineName,
              p.Dosage,
              p.Quantity,
              p.Instruction
          )).ToList();

          return new MedicalRecordDto(
              mr.MedicalRecordId,
              mr.AppointmentId,
              patientName,
              doctorName,
              mr.Symptoms,
              mr.Diagnosis,
              mr.Treatment,
              mr.Notes,
              mr.CreatedAt,
              prescriptions
          );
      }
  }
  ```

#### [MODIFY] [AppointmentService.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Application/Services/AppointmentService.cs)
Thêm các phương thức thay đổi trạng thái cuộc hẹn của Doctor & Patient.
- **Thêm các phương thức:**
  ```csharp
  public async Task ConfirmAsync(int id)
  {
      var appointment = await _repo.GetByIdAsync(id)
          ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

      if (appointment.Status != "Pending")
      {
          throw new InvalidOperationException($"Cannot confirm. Current status is '{appointment.Status}' but must be 'Pending'.");
      }

      appointment.Status = "Confirmed";
      await _repo.UpdateAsync(appointment);
  }

  public async Task StartExaminationAsync(int id)
  {
      var appointment = await _repo.GetByIdAsync(id)
          ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

      if (appointment.Status != "Confirmed")
      {
          throw new InvalidOperationException($"Cannot start examination. Current status is '{appointment.Status}' but must be 'Confirmed'.");
      }

      appointment.Status = "InProgress";
      await _repo.UpdateAsync(appointment);
  }

  public async Task CompleteAsync(int id)
  {
      var appointment = await _repo.GetByIdAsync(id)
          ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

      if (appointment.Status != "InProgress")
      {
          throw new InvalidOperationException($"Cannot complete appointment. Current status is '{appointment.Status}' but must be 'InProgress'.");
      }

      appointment.Status = "Completed";
      await _repo.UpdateAsync(appointment);
  }

  public async Task<IEnumerable<AppointmentDto>> GetByPatientUserIdAsync(int userId)
  {
      var list = await _repo.GetAllAsync();
      return list
          .Where(a => a.Patient != null && a.Patient.UserId == userId)
          .Select(MapToDto);
  }
  ```

---

### 3. API Client Services (Dành cho WebMvc gọi WebApi)

#### [NEW] [MedicalRecordApiService.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Application/Services/MedicalRecordApiService.cs)
Hỗ trợ WebMvc gọi WebApi endpoint qua HttpClient.
- **Namespace:** `MyProject.Application.Services`
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Net.Http.Json;
  using System.Threading.Tasks;
  using MyProject.Application.DTOs;

  namespace MyProject.Application.Services;

  public class MedicalRecordApiService
  {
      private readonly IHttpClientFactory _httpClientFactory;
      private readonly string _clientName = "WebApiClient";

      public MedicalRecordApiService(IHttpClientFactory httpClientFactory)
      {
          _httpClientFactory = httpClientFactory;
      }

      private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

      public async Task<List<MedicalRecordDto>> GetAllAsync()
      {
          var client = GetClient();
          var response = await client.GetAsync("medicalrecords");
          response.EnsureSuccessStatusCode();
          return await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>() ?? new List<MedicalRecordDto>();
      }

      public async Task<MedicalRecordDto?> GetByIdAsync(int id)
      {
          var client = GetClient();
          var response = await client.GetAsync($"medicalrecords/{id}");
          if (!response.IsSuccessStatusCode) return null;
          return await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
      }

      public async Task<MedicalRecordDto?> GetByAppointmentIdAsync(int appointmentId)
      {
          var client = GetClient();
          var response = await client.GetAsync($"medicalrecords/by-appointment/{appointmentId}");
          if (!response.IsSuccessStatusCode) return null;
          return await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
      }

      public async Task<List<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
      {
          var client = GetClient();
          var response = await client.GetAsync($"medicalrecords/by-patient/{patientId}");
          response.EnsureSuccessStatusCode();
          return await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>() ?? new List<MedicalRecordDto>();
      }

      public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordRequest request)
      {
          var client = GetClient();
          var response = await client.PostAsJsonAsync("medicalrecords", request);
          response.EnsureSuccessStatusCode();
          return await response.Content.ReadFromJsonAsync<MedicalRecordDto>() ?? throw new InvalidOperationException("Failed to deserialize created record.");
      }

      public async Task AddPrescriptionsAsync(int medicalRecordId, List<CreatePrescriptionRequest> prescriptions)
      {
          var client = GetClient();
          var response = await client.PostAsJsonAsync($"medicalrecords/{medicalRecordId}/prescriptions", prescriptions);
          response.EnsureSuccessStatusCode();
      }
  }
  ```

#### [MODIFY] [AppointmentApiService.cs](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.Application/Services/AppointmentApiService.cs)
Bổ sung các phương thức gọi WebApi để đổi trạng thái cuộc hẹn.
- **Thêm các phương thức:**
  ```csharp
  public async Task ConfirmAsync(int id)
  {
      var client = GetClient();
      var response = await client.PostAsync($"appointments/{id}/confirm", null);
      await ThrowIfErrorAsync(response);
  }

  public async Task StartExaminationAsync(int id)
  {
      var client = GetClient();
      var response = await client.PostAsync($"appointments/{id}/start", null);
      await ThrowIfErrorAsync(response);
  }

  public async Task CompleteAsync(int id)
  {
      var client = GetClient();
      var response = await client.PostAsync($"appointments/{id}/complete", null);
      await ThrowIfErrorAsync(response);
  }

  public async Task<List<AppointmentDto>> GetByPatientAsync(int patientId)
  {
      var client = GetClient();
      var response = await client.GetAsync($"appointments/by-patient/{patientId}");
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<List<AppointmentDto>>() ?? new List<AppointmentDto>();
  }
  ```

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Lớp DTO được triển khai dưới dạng `record` trong `MyProject.Application/DTOs`.
- [ ] Dịch vụ `MedicalRecordService` chỉ tham chiếu đến `IMedicalRecordRepository` và `IPrescriptionRepository`.
- [ ] Không inject trực tiếp `DbContext` vào Service.
- [ ] Cả `AppointmentService` và `AppointmentApiService` đều bổ sung các tác vụ Confirm, Start, Complete.
