# Task 4: WebMVC Controllers & Authorization

Task này thực hiện việc tái cấu trúc phân quyền trên các controller cũ, bổ sung trang Đăng ký tài khoản cho Patient, và tạo các Controller mới phục vụ Cổng thông tin Patient (`PatientPortalController`) cùng trang quản lý Hồ sơ bệnh án (`MedicalRecordsController`).

---

## 📋 PLAN: WebMvc Controllers

### 1. Cập nhật các Controller cũ

#### [MODIFY] [HomeController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/HomeController.cs)
Cập nhật action `Index` để chuyển hướng Patient về Dashboard của họ:
```csharp
if (User.IsInRole("Patient"))
{
    return RedirectToAction("Dashboard", "PatientPortal");
}
```

#### [MODIFY] [AccountController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/AccountController.cs)
- Inject thêm `PatientService` (dùng local service để đăng ký tài khoản trực tiếp vào cơ sở dữ liệu).
- Thêm action `Register` (GET/POST) không yêu cầu xác thực (`[AllowAnonymous]`):
```csharp
[HttpGet]
[AllowAnonymous]
public IActionResult Register()
{
    if (User.Identity?.IsAuthenticated == true)
    {
        return RedirectToAction("Index", "Home");
    }
    return View();
}

[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(CreatePatientRequest request)
{
    if (!ModelState.IsValid)
    {
        return View(request);
    }

    try
    {
        await _patientService.CreateAsync(request);
        TempData["SuccessMessage"] = "Registration successful! Please log in.";
        return RedirectToAction(nameof(Login));
    }
    catch (ArgumentException ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(request);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", "Registration failed: " + ex.Message);
        return View(request);
    }
}
```

#### [MODIFY] [PatientsController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/PatientsController.cs)
Chỉ cho phép **Admin** truy cập. Thay thế `[Authorize]` ở đầu class thành:
```csharp
[Authorize(Roles = "Admin")]
```
(Có thể lược bỏ các kiểm tra thủ công `User.IsInRole("Doctor")` trong các action vì bộ lọc Role đã chặn các role khác ở mức lớp học).

#### [MODIFY] [AppointmentsController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/AppointmentsController.cs)
Tái cấu trúc phân quyền và thêm các Action thay đổi trạng thái:
- Phân quyền đầu class: `[Authorize(Roles = "Admin,Doctor")]`.
- Xóa Action `ScheduleFollowup` cũ.
- Thêm các Action đổi trạng thái (sử dụng `AppointmentApiService`):
```csharp
[HttpPost]
[Authorize(Roles = "Doctor,Admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Confirm(int id)
{
    try
    {
        await _appointmentService.ConfirmAsync(id);
        TempData["SuccessMessage"] = "Appointment confirmed successfully.";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;
    }
    return RedirectToAction(nameof(Queue));
}

[HttpPost]
[Authorize(Roles = "Doctor")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> StartExamination(int id)
{
    try
    {
        await _appointmentService.StartExaminationAsync(id);
        TempData["SuccessMessage"] = "Examination started.";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;
    }
    return RedirectToAction(nameof(Queue));
}

[HttpPost]
[Authorize(Roles = "Doctor")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Complete(int id)
{
    try
    {
        await _appointmentService.CompleteAsync(id);
        TempData["SuccessMessage"] = "Appointment completed. Please create a medical record.";
        return RedirectToAction("Create", "MedicalRecords", new { appointmentId = id });
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = ex.Message;
        return RedirectToAction(nameof(Queue));
    }
}
```

---

### 2. Tạo mới các Controller

#### [NEW] [MedicalRecordsController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/MedicalRecordsController.cs)
- **Namespace:** `MyProject.WebMvc.Controllers`
- **Quyền:** Admin, Doctor, Patient.
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using MyProject.Application.DTOs;
  using MyProject.Application.Services;

  namespace MyProject.WebMvc.Controllers;

  [Authorize]
  public class MedicalRecordsController : Controller
  {
      private readonly MedicalRecordApiService _medicalRecordService;
      private readonly PatientApiService _patientApiService;
      private readonly AppointmentApiService _appointmentApiService;

      public MedicalRecordsController(
          MedicalRecordApiService medicalRecordService,
          PatientApiService patientApiService,
          AppointmentApiService appointmentApiService)
      {
          _medicalRecordService = medicalRecordService;
          _patientApiService = patientApiService;
          _appointmentApiService = appointmentApiService;
      }

      [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Index()
      {
          var list = await _medicalRecordService.GetAllAsync();
          return View(list);
      }

      public async Task<IActionResult> Details(int id)
      {
          var record = await _medicalRecordService.GetByIdAsync(id);
          if (record == null) return NotFound();

          if (User.IsInRole("Patient"))
          {
              var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
              var patients = await _patientApiService.GetAllAsync();
              var patient = patients.FirstOrDefault(p => p.UserId == userId);
              if (patient == null || record.PatientName != patient.FullName)
              {
                  return Forbid();
              }
          }

          return View(record);
      }

      [Authorize(Roles = "Doctor")]
      [HttpGet]
      public async Task<IActionResult> Create(int appointmentId)
      {
          var appt = await _appointmentApiService.GetByIdAsync(appointmentId);
          if (appt == null) return NotFound();

          ViewBag.Appointment = appt;
          var request = new CreateMedicalRecordRequest(
              AppointmentId: appointmentId,
              Symptoms: appt.Reason,
              Diagnosis: "",
              Treatment: "",
              Notes: ""
          );
          return View(request);
      }

      [Authorize(Roles = "Doctor")]
      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> Create(CreateMedicalRecordRequest request, string? medicinesJson)
      {
          if (!ModelState.IsValid)
          {
              var appt = await _appointmentApiService.GetByIdAsync(request.AppointmentId);
              ViewBag.Appointment = appt;
              return View(request);
          }

          try
          {
              var record = await _medicalRecordService.CreateAsync(request);

              // Thêm đơn thuốc nếu có danh sách truyền lên dạng JSON
              if (!string.IsNullOrEmpty(medicinesJson))
              {
                  var prescriptions = System.Text.Json.JsonSerializer.Deserialize<List<CreatePrescriptionRequest>>(medicinesJson);
                  if (prescriptions != null && prescriptions.Any())
                  {
                      await _medicalRecordService.AddPrescriptionsAsync(record.MedicalRecordId, prescriptions);
                  }
              }

              TempData["SuccessMessage"] = "Medical record created successfully.";
              return RedirectToAction("Details", new { id = record.MedicalRecordId });
          }
          catch (Exception ex)
          {
              TempData["ErrorMessage"] = ex.Message;
              var appt = await _appointmentApiService.GetByIdAsync(request.AppointmentId);
              ViewBag.Appointment = appt;
              return View(request);
          }
      }
  }
  ```

#### [NEW] [PatientPortalController.cs (MVC)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Controllers/PatientPortalController.cs)
Cổng thông tin chuyên biệt dành cho bệnh nhân.
- **Namespace:** `MyProject.WebMvc.Controllers`
- **Quyền:** `[Authorize(Roles = "Patient")]`
- **Code Template:**
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using MyProject.Application.DTOs;
  using MyProject.Application.Services;

  namespace MyProject.WebMvc.Controllers;

  [Authorize(Roles = "Patient")]
  public class PatientPortalController : Controller
  {
      private readonly AppointmentApiService _appointmentService;
      private readonly PatientApiService _patientService;
      private readonly DoctorApiService _doctorService;
      private readonly MedicalRecordApiService _medicalRecordService;

      public PatientPortalController(
          AppointmentApiService appointmentService,
          PatientApiService patientService,
          DoctorApiService doctorService,
          MedicalRecordApiService medicalRecordService)
      {
          _appointmentService = appointmentService;
          _patientService = patientService;
          _doctorService = doctorService;
          _medicalRecordService = medicalRecordService;
      }

      private async Task<PatientDto?> GetCurrentPatientAsync()
      {
          var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
          if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
              return null;

          var patients = await _patientService.GetAllAsync();
          return patients.FirstOrDefault(p => p.UserId == userId);
      }

      public async Task<IActionResult> Dashboard()
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          var appointments = await _appointmentService.GetAllAsync();
          var myAppointments = appointments
              .Where(a => a.PatientId == patient.PatientId)
              .OrderByDescending(a => a.AppointmentDate)
              .ThenBy(a => a.AppointmentTime)
              .ToList();

          ViewBag.UpcomingAppointments = myAppointments
              .Where(a => a.AppointmentDate >= DateOnly.FromDateTime(DateTime.Today) && a.Status != "Completed")
              .ToList();

          return View(patient);
      }

      [HttpGet]
      public async Task<IActionResult> BookAppointment()
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          ViewBag.Doctors = await _doctorService.GetAllAsync();
          var request = new CreateAppointmentRequest(
              PatientId: patient.PatientId,
              DoctorId: 0,
              AppointmentDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
              AppointmentTime: TimeSpan.FromHours(9),
              Status: "Pending",
              Reason: ""
          );
          return View(request);
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> BookAppointment(CreateAppointmentRequest request)
      {
          if (!ModelState.IsValid)
          {
              ViewBag.Doctors = await _doctorService.GetAllAsync();
              return View(request);
          }

          try
          {
              await _appointmentService.CreateAsync(request);
              TempData["SuccessMessage"] = "Appointment requested successfully! Waiting for confirmation.";
              return RedirectToAction(nameof(Dashboard));
          }
          catch (Exception ex)
          {
              TempData["ErrorMessage"] = ex.Message;
              ViewBag.Doctors = await _doctorService.GetAllAsync();
              return View(request);
          }
      }

      public async Task<IActionResult> MyHistory()
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          var appointments = await _appointmentService.GetAllAsync();
          var list = appointments.Where(a => a.PatientId == patient.PatientId)
              .OrderByDescending(a => a.AppointmentDate)
              .ToList();

          return View(list);
      }

      public async Task<IActionResult> MyMedicalRecords()
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          var list = await _medicalRecordService.GetByPatientIdAsync(patient.PatientId);
          return View(list);
      }

      [HttpGet]
      public async Task<IActionResult> MyProfile()
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          var request = new UpdatePatientRequest(
              FullName: patient.FullName,
              Phone: patient.Phone,
              DateOfBirth: patient.DateOfBirth,
              Gender: patient.Gender,
              Address: patient.Address,
              BloodType: patient.BloodType,
              EmergencyContactName: patient.EmergencyContactName,
              EmergencyContactPhone: patient.EmergencyContactPhone
          );
          return View(request);
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> MyProfile(UpdatePatientRequest request)
      {
          var patient = await GetCurrentPatientAsync();
          if (patient == null) return RedirectToAction("Logout", "Account");

          if (!ModelState.IsValid) return View(request);

          try
          {
              await _patientService.UpdateAsync(patient.PatientId, request);
              TempData["SuccessMessage"] = "Profile updated successfully.";
              return RedirectToAction(nameof(Dashboard));
          }
          catch (Exception ex)
          {
              TempData["ErrorMessage"] = ex.Message;
              return View(request);
          }
      }
  }
  ```

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Phân quyền bằng `[Authorize(Roles = "...")]` chính xác ở đầu class và các action.
- [ ] Chuyển hướng Dashboard tự động cho Patient trong `HomeController`.
- [ ] Đăng ký tài khoản trong `AccountController` gọi trực tiếp `PatientService` để tạo user & patient.
- [ ] Các Controller gọi `ApiService` để tương tác với WebAPI đúng kiến trúc.
