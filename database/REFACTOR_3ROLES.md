# Tái cấu trúc hệ thống: 3 Role (Admin / Doctor / Patient)

## Mô tả

Hiện tại hệ thống có các role: Admin, Doctor, và nhầm lẫn một phần là "Staff" (không có role riêng, nhưng code đang để mọi user đã đăng nhập đều truy cập được chức năng của nhân viên lễ tân). Yêu cầu tái cấu trúc lại thành 3 role rõ ràng:

- **Admin**: Quản lý Doctor, Patient, Appointment, Xem Medical Record
- **Doctor**: Xem lịch hẹn, Xác nhận lịch hẹn, Khám bệnh, Tạo Medical Record, Kê đơn thuốc
- **Patient**: Đăng ký/Đăng nhập, Đặt lịch khám, Xem lịch sử khám, Xem hồ sơ bệnh án

> **IMPORTANT**: Cần thêm mới hoàn toàn: Repository + Service + API Controller + MVC Controller + Views cho `MedicalRecord` và `Prescription` (2 entity đã có trong DB nhưng chưa có code xử lý).

> **IMPORTANT**: Patient cần có trang tự đăng ký (register) khi chưa có account — hiện tại chỉ Admin mới tạo được Patient.

---

## ⚠️ User Review Required

> **WARNING — Bỏ role Staff**: Hiện tại code không có role "Staff" riêng — logic đang là: ai đăng nhập được (không phải Doctor) thì được quản lý Patient và Appointment. Sau khi tái cấu trúc, **chỉ Admin** mới làm được điều đó.

> **IMPORTANT — Patient tự đăng ký**: Cần thêm trang `/Account/Register` public (không cần đăng nhập) để Patient tự đăng ký tài khoản. Confirm: bạn có muốn trang này hay chỉ Admin mới tạo Patient?

---

## Proposed Changes

---

### Layer 1: Domain — New Repository Interfaces

#### [NEW] `IMedicalRecordRepository.cs`
`MyProject.Domain/IRepositories/IMedicalRecordRepository.cs`

- `GetAllAsync()`
- `GetByIdAsync(int)`
- `GetByAppointmentIdAsync(int)`
- `GetByPatientIdAsync(int)`
- `AddAsync()`
- `UpdateAsync()`

#### [NEW] `IPrescriptionRepository.cs`
`MyProject.Domain/IRepositories/IPrescriptionRepository.cs`

- `GetByMedicalRecordIdAsync(int)`
- `AddAsync()`
- `DeleteAsync(int)`

---

### Layer 2: Infrastructure — New Repository Implementations

#### [NEW] `MedicalRecordRepository.cs`
`MyProject.Infrastructure/Repositories/MedicalRecordRepository.cs`

- Include `Appointment.Patient.User`, `Appointment.Doctor.User`, `Prescriptions`

#### [NEW] `PrescriptionRepository.cs`
`MyProject.Infrastructure/Repositories/PrescriptionRepository.cs`

---

### Layer 3: Application — DTOs + Services

#### [NEW] `MedicalRecordDto.cs`
`MyProject.Application/DTOs/MedicalRecordDto.cs`

```
MedicalRecordDto(MedicalRecordId, AppointmentId, PatientName, DoctorName, Symptoms, Diagnosis, Treatment, Notes, CreatedAt, Prescriptions)
CreateMedicalRecordRequest(AppointmentId, Symptoms, Diagnosis, Treatment, Notes)
PrescriptionDto(PrescriptionId, MedicineName, Dosage, Quantity, Instruction)
CreatePrescriptionRequest(MedicineName, Dosage, Quantity, Instruction)
```

#### [NEW] `MedicalRecordService.cs`
`MyProject.Application/Services/MedicalRecordService.cs`

- `GetAllAsync()`
- `GetByIdAsync()`
- `GetByPatientIdAsync()`
- `CreateAsync()`
- `AddPrescriptionsAsync()`

#### [NEW] `MedicalRecordApiService.cs`
`MyProject.Application/Services/MedicalRecordApiService.cs`

- HTTP client gọi API `/api/medicalrecords`

#### [MODIFY] `AppointmentService.cs`

- Thêm `ConfirmAsync(int id)` — đổi status `Pending → Confirmed`
- Thêm `StartExaminationAsync(int id)` — đổi status `Confirmed → InProgress`
- Thêm `CompleteAsync(int id)` — đổi status `InProgress → Completed`
- Thêm `GetByPatientUserIdAsync(int userId)` — lấy lịch hẹn theo userId của Patient

#### [MODIFY] `AppointmentApiService.cs`

- Thêm `ConfirmAsync(int id)`, `StartExaminationAsync(int id)`, `CompleteAsync(int id)`
- Thêm `GetByPatientAsync(int patientId)`

---

### Layer 4: WebAPI — New Controllers

#### [MODIFY] `AppointmentsController.cs` (WebApi)

- Thêm `POST /{id}/confirm` → `ConfirmAsync` *(Doctor only)*
- Thêm `POST /{id}/start` → `StartExaminationAsync` *(Doctor only)*
- Thêm `POST /{id}/complete` → `CompleteAsync` *(Doctor only)*
- Thêm `GET /by-patient/{patientId}` → lọc theo patient

#### [NEW] `MedicalRecordsController.cs` (WebApi)

| Method | Route | Auth | Mô tả |
|--------|-------|------|-------|
| GET | `/api/medicalrecords` | Admin only | Lấy tất cả hồ sơ |
| GET | `/api/medicalrecords/{id}` | — | Lấy theo ID |
| GET | `/api/medicalrecords/by-appointment/{appointmentId}` | — | Lấy theo lịch hẹn |
| GET | `/api/medicalrecords/by-patient/{patientId}` | Patient | Xem hồ sơ của mình |
| POST | `/api/medicalrecords` | Doctor only | Tạo hồ sơ khám |
| POST | `/api/medicalrecords/{id}/prescriptions` | Doctor only | Thêm đơn thuốc |

---

### Layer 5: WebMVC — Controllers

#### [MODIFY] `HomeController.cs`

- Redirect Patient → `/Patient/Dashboard` thay vì Doctor → Queue
- Admin giữ nguyên dashboard tổng quan

#### [MODIFY] `AccountController.cs` (MVC)

- Thêm `GET/POST /Account/Register` — trang đăng ký cho Patient mới

#### [MODIFY] `AppointmentsController.cs` (MVC)

Phân quyền lại:

| Role | Actions |
|------|---------|
| Admin | Index (tất cả), Create, Edit, Delete |
| Doctor | Queue (hàng đợi), MyAppointments, Confirm, StartExamination, Complete |
| Patient | BookAppointment (đặt lịch), MyHistory (lịch sử) |

- Bỏ `ScheduleFollowup` (thay thế bằng flow Doctor → Complete → tạo MedicalRecord)

#### [MODIFY] `PatientsController.cs` (MVC)

- **Admin only**: Index, Details, Create, Edit, Delete
- Bỏ hoàn toàn quyền của role khác

#### [NEW] `MedicalRecordsController.cs` (MVC)

| Method | Route | Auth | Mô tả |
|--------|-------|------|-------|
| GET | `/MedicalRecords` | Admin | Xem tất cả hồ sơ |
| GET | `/MedicalRecords/Details/{id}` | Admin + Doctor + Patient (chủ sở hữu) | Xem chi tiết |
| GET | `/MedicalRecords/Create/{appointmentId}` | Doctor only | Tạo hồ sơ sau khám |
| POST | `/MedicalRecords/Create` | Doctor only | Submit tạo hồ sơ |
| POST | `/MedicalRecords/{id}/AddPrescription` | Doctor only | Thêm đơn thuốc |

#### [NEW] `PatientPortalController.cs` (MVC)

| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/PatientPortal/Dashboard` | Trang chủ Patient |
| GET | `/PatientPortal/BookAppointment` | Đặt lịch khám |
| POST | `/PatientPortal/BookAppointment` | Submit đặt lịch |
| GET | `/PatientPortal/MyHistory` | Xem lịch sử lịch hẹn |
| GET | `/PatientPortal/MyMedicalRecords` | Xem hồ sơ bệnh án |
| GET | `/PatientPortal/MyProfile` | Xem/sửa thông tin cá nhân |

---

### Layer 6: Views (Razor)

#### [NEW] `Views/Account/Register.cshtml`
Trang đăng ký Patient

#### [NEW] `Views/MedicalRecords/Index.cshtml`
Admin xem tất cả Medical Records (có filter theo Patient)

#### [NEW] `Views/MedicalRecords/Details.cshtml`
Xem chi tiết hồ sơ + danh sách đơn thuốc

#### [NEW] `Views/MedicalRecords/Create.cshtml`
Doctor tạo hồ sơ khám bệnh (Symptoms, Diagnosis, Treatment, Notes + danh sách thuốc)

#### [NEW] `Views/PatientPortal/Dashboard.cshtml`
Trang chủ Patient: tóm tắt lịch hẹn sắp tới, thông báo

#### [NEW] `Views/PatientPortal/BookAppointment.cshtml`
Patient chọn Doctor + Ngày giờ + Lý do

#### [NEW] `Views/PatientPortal/MyHistory.cshtml`
Patient xem danh sách lịch hẹn của mình

#### [NEW] `Views/PatientPortal/MyMedicalRecords.cshtml`
Patient xem danh sách + chi tiết hồ sơ bệnh án

#### [MODIFY] `Views/Appointments/Queue.cshtml`
Doctor: thêm nút **"Xác nhận"** (Confirm), **"Bắt đầu khám"** (Start), **"Hoàn thành"** (Complete → redirect tạo MedicalRecord)

#### [MODIFY] `Views/Shared/_Layout.cshtml`
Cập nhật menu điều hướng theo role:

| Role | Menu items |
|------|-----------|
| Admin | Patients, Doctors, Appointments, Medical Records, Roles |
| Doctor | Queue, My Appointments, My Patients' Records |
| Patient | Dashboard, Book Appointment, My History, My Medical Records, My Profile |

---

### Layer 7: Program.cs Updates

#### [MODIFY] `Program.cs` (WebMvc)

- Đăng ký `IMedicalRecordRepository`, `IPrescriptionRepository`
- Đăng ký `MedicalRecordService`, `MedicalRecordApiService`

#### [MODIFY] `Program.cs` (WebApi)

- Đăng ký `IMedicalRecordRepository`, `IPrescriptionRepository`
- Đăng ký `MedicalRecordService`

---

## Appointment Status Flow

```
Pending  →  Confirmed  →  InProgress  →  Completed
  (Patient đặt)  (Admin/Doctor confirm)  (Doctor bắt đầu khám)  (Hoàn thành → tạo MedicalRecord)
```

---

## Verification Plan

### Automated Tests

```bash
dotnet build
```

### Manual Verification

1. Đăng nhập với account **Admin** → kiểm tra menu và quyền truy cập
2. Đăng nhập với account **Doctor** → kiểm tra Queue, luồng khám, tạo MedicalRecord + Prescription
3. **Đăng ký** tài khoản Patient mới → đăng nhập → đặt lịch → xem lịch sử + hồ sơ
4. Kiểm tra **Patient không vào được** trang quản lý Admin/Doctor
