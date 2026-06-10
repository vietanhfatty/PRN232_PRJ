# Task 3: WebAPI Controllers

Task này bao gồm việc bổ sung các API endpoint cho lịch hẹn và tạo mới Controller cho Hồ sơ bệnh án trong dự án `MyProject.WebApi`.

---

## 📋 PLAN: WebAPI Controllers

### 1. Cập nhật AppointmentsController

#### [MODIFY] [AppointmentsController.cs (WebApi)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebApi/Controllers/AppointmentsController.cs)
Bổ sung các endpoint điều khiển trạng thái cuộc hẹn.
- **Các Endpoint cần thêm:**
  - `POST /api/appointments/{id}/confirm` — Bác sĩ/Admin xác nhận.
  - `POST /api/appointments/{id}/start` — Bác sĩ bắt đầu khám.
  - `POST /api/appointments/{id}/complete` — Bác sĩ hoàn thành khám.
  - `GET /api/appointments/by-patient/{patientId}` — Lấy danh sách lịch hẹn của bệnh nhân.
- **Code Template bổ sung:**
  ```csharp
  [HttpPost("{id}/confirm")]
  [Authorize(Roles = "Doctor,Admin")]
  public async Task<IActionResult> Confirm(int id)
  {
      try
      {
          await _service.ConfirmAsync(id);
          return Ok(new { Message = "Appointment confirmed successfully." });
      }
      catch (KeyNotFoundException ex)
      {
          return NotFound(new { Message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
          return BadRequest(new { Message = ex.Message });
      }
  }

  [HttpPost("{id}/start")]
  [Authorize(Roles = "Doctor")]
  public async Task<IActionResult> StartExamination(int id)
  {
      try
      {
          await _service.StartExaminationAsync(id);
          return Ok(new { Message = "Examination started." });
      }
      catch (KeyNotFoundException ex)
      {
          return NotFound(new { Message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
          return BadRequest(new { Message = ex.Message });
      }
  }

  [HttpPost("{id}/complete")]
  [Authorize(Roles = "Doctor")]
  public async Task<IActionResult> Complete(int id)
  {
      try
      {
          await _service.CompleteAsync(id);
          return Ok(new { Message = "Appointment completed." });
      }
      catch (KeyNotFoundException ex)
      {
          return NotFound(new { Message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
          return BadRequest(new { Message = ex.Message });
      }
  }

  [HttpGet("by-patient/{patientId}")]
  [Authorize]
  public async Task<IActionResult> GetByPatient(int patientId)
  {
      return Ok(await _service.GetByPatientIdAsync(patientId));
  }
  ```

---

### 2. Tạo mới MedicalRecordsController

#### [NEW] [MedicalRecordsController.cs (WebApi)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebApi/Controllers/MedicalRecordsController.cs)
Quản lý các thao tác xem và tạo hồ sơ bệnh án, kê đơn.
- **Namespace:** `MyProject.WebApi.Controllers`
- **Quy tắc phân quyền:**
  - `GET /api/medicalrecords` -> Admin duy nhất.
  - `GET /api/medicalrecords/{id}` -> Cho phép bất kỳ ai đã đăng nhập (sẽ được phân quyền chi tiết hơn ở MVC nếu cần).
  - `POST /api/medicalrecords` -> Chỉ Doctor.
  - `POST /api/medicalrecords/{id}/prescriptions` -> Chỉ Doctor.
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using MyProject.Application.DTOs;
  using MyProject.Application.Services;

  namespace MyProject.WebApi.Controllers;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class MedicalRecordsController : ControllerBase
  {
      private readonly MedicalRecordService _service;

      public MedicalRecordsController(MedicalRecordService service)
      {
          _service = service;
      }

      [HttpGet]
      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> GetAll()
      {
          return Ok(await _service.GetAllAsync());
      }

      [HttpGet("{id}")]
      public async Task<IActionResult> GetById(int id)
      {
          var result = await _service.GetByIdAsync(id);
          if (result == null) return NotFound();
          return Ok(result);
      }

      [HttpGet("by-appointment/{appointmentId}")]
      public async Task<IActionResult> GetByAppointmentId(int appointmentId)
      {
          var result = await _service.GetByAppointmentIdAsync(appointmentId);
          if (result == null) return NotFound();
          return Ok(result);
      }

      [HttpGet("by-patient/{patientId}")]
      public async Task<IActionResult> GetByPatientId(int patientId)
      {
          var result = await _service.GetByPatientIdAsync(patientId);
          return Ok(result);
      }

      [HttpPost]
      [Authorize(Roles = "Doctor")]
      public async Task<IActionResult> Create([FromBody] CreateMedicalRecordRequest request)
      {
          try
          {
              var result = await _service.CreateAsync(request);
              return CreatedAtAction(nameof(GetById), new { id = result.MedicalRecordId }, result);
          }
          catch (Exception ex)
          {
              return BadRequest(new { Message = ex.Message });
          }
      }

      [HttpPost("{id}/prescriptions")]
      [Authorize(Roles = "Doctor")]
      public async Task<IActionResult> AddPrescriptions(int id, [FromBody] List<CreatePrescriptionRequest> prescriptions)
      {
          try
          {
              await _service.AddPrescriptionsAsync(id, prescriptions);
              return Ok(new { Message = "Prescriptions added successfully." });
          }
          catch (Exception ex)
          {
              return BadRequest(new { Message = ex.Message });
          }
      }
  }
  ```

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Các Endpoint phân quyền đúng vai trò (`[Authorize(Roles = "Doctor")]`, `[Authorize(Roles = "Admin")]`).
- [ ] Các Controller WebAPI chỉ gọi trực tiếp `AppointmentService` hoặc `MedicalRecordService` (không tham chiếu DbContext).
- [ ] Kết quả trả về chứa đầy đủ HTTP status (200 OK, 201 Created, 400 BadRequest, 404 NotFound).
