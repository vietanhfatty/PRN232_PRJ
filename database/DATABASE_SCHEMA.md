# DATABASE_SCHEMA.md — HospitalManagementDB

> SQL Server | 6 tables | Last updated: 2026-06-10

---

## Overview

```
HospitalManagementDB
├── Roles
├── Users          → FK → Roles
├── Doctors        → FK → Users
├── Patients       → FK → Users
├── Appointments   → FK → Patients, Doctors
├── MedicalRecords → FK → Appointments
└── Prescriptions  → FK → MedicalRecords
```

Luồng nghiệp vụ chính:
`User (có Role)` → được profile hóa thành `Doctor` hoặc `Patient` → `Patient` đặt `Appointment` với `Doctor` → sau khám tạo `MedicalRecord` → bác sĩ kê `Prescription` trong record.

---

## Table Details

### 1. `Roles`

Danh mục vai trò hệ thống (Admin, Doctor, Patient, ...).

| Column   | Type          | Constraints          | Note              |
|----------|---------------|----------------------|-------------------|
| RoleId   | INT           | PK, IDENTITY(1,1)    | Khóa chính        |
| RoleName | VARCHAR(50)   | NOT NULL, UNIQUE     | Tên role duy nhất |

---

### 2. `Users`

Tài khoản đăng nhập dùng chung cho tất cả vai trò.

| Column       | Type           | Constraints              | Note                      |
|--------------|----------------|--------------------------|---------------------------|
| UserId       | INT            | PK, IDENTITY(1,1)        | Khóa chính                |
| FullName     | NVARCHAR(100)  | NOT NULL                 | Họ tên đầy đủ             |
| Username     | VARCHAR(100)   | NOT NULL, UNIQUE         | Tên đăng nhập             |
| PasswordHash | NVARCHAR(255)  | NOT NULL                 | Mật khẩu đã hash          |
| Phone        | VARCHAR(20)    | NULL                     | Số điện thoại             |
| RoleId       | INT            | NOT NULL, FK → Roles     | Vai trò của user          |
| IsActive     | BIT            | NOT NULL, DEFAULT 1      | Trạng thái tài khoản      |
| CreatedAt    | DATETIME       | NOT NULL, DEFAULT GETDATE() | Thời điểm tạo          |
| UpdatedAt    | DATETIME       | NULL                     | Thời điểm cập nhật cuối   |

**Relationships:** `RoleId` → `Roles.RoleId`

---

### 3. `Doctors`

Profile mở rộng cho user có role Doctor.

| Column          | Type           | Constraints              | Note                        |
|-----------------|----------------|--------------------------|-----------------------------|
| DoctorId        | INT            | PK, IDENTITY(1,1)        | Khóa chính                  |
| UserId          | INT            | NOT NULL, UNIQUE, FK → Users | 1-1 với Users           |
| Specialization  | NVARCHAR(100)  | NOT NULL                 | Chuyên khoa                 |
| ExperienceYears | INT            | DEFAULT 0                | Số năm kinh nghiệm          |
| Description     | NVARCHAR(1000) | NULL                     | Mô tả giới thiệu bác sĩ    |

**Relationships:** `UserId` → `Users.UserId` (UNIQUE → quan hệ 1-1)

---

### 4. `Patients`

Profile mở rộng cho user có role Patient.

| Column                | Type           | Constraints              | Note                        |
|-----------------------|----------------|--------------------------|-----------------------------|
| PatientId             | INT            | PK, IDENTITY(1,1)        | Khóa chính                  |
| UserId                | INT            | NOT NULL, UNIQUE, FK → Users | 1-1 với Users           |
| DateOfBirth           | DATE           | NULL                     | Ngày sinh                   |
| Gender                | NVARCHAR(10)   | NULL                     | Giới tính                   |
| Address               | NVARCHAR(255)  | NULL                     | Địa chỉ                     |
| BloodType             | NVARCHAR(10)   | NULL                     | Nhóm máu (A, B, AB, O, ±)  |
| EmergencyContactName  | NVARCHAR(100)  | NULL                     | Tên người liên hệ khẩn cấp |
| EmergencyContactPhone | VARCHAR(20)    | NULL                     | SĐT người liên hệ khẩn cấp |

**Relationships:** `UserId` → `Users.UserId` (UNIQUE → quan hệ 1-1)

---

### 5. `Appointments`

Lịch hẹn khám giữa bệnh nhân và bác sĩ.

| Column          | Type         | Constraints                   | Note                                          |
|-----------------|--------------|-------------------------------|-----------------------------------------------|
| AppointmentId   | INT          | PK, IDENTITY(1,1)             | Khóa chính                                    |
| PatientId       | INT          | NOT NULL, FK → Patients       | Bệnh nhân đặt lịch                            |
| DoctorId        | INT          | NOT NULL, FK → Doctors        | Bác sĩ khám                                   |
| AppointmentDate | DATE         | NOT NULL                      | Ngày hẹn                                      |
| AppointmentTime | TIME         | NOT NULL                      | Giờ hẹn                                       |
| Reason          | NVARCHAR(500)| NULL                          | Lý do khám                                    |
| Status          | VARCHAR(20)  | NOT NULL, DEFAULT 'Pending'   | Trạng thái: `Pending`, `Confirmed`, `Completed`, `Cancelled` |
| CreatedAt       | DATETIME     | NOT NULL, DEFAULT GETDATE()   | Thời điểm tạo lịch                            |

**Relationships:**
- `PatientId` → `Patients.PatientId`
- `DoctorId` → `Doctors.DoctorId`

---

### 6. `MedicalRecords`

Hồ sơ bệnh án sau mỗi lần khám (1-1 với Appointment).

| Column          | Type          | Constraints                     | Note                        |
|-----------------|---------------|---------------------------------|-----------------------------|
| MedicalRecordId | INT           | PK, IDENTITY(1,1)               | Khóa chính                  |
| AppointmentId   | INT           | NOT NULL, UNIQUE, FK → Appointments | 1-1 với Appointments    |
| Symptoms        | NVARCHAR(MAX) | NULL                            | Triệu chứng                 |
| Diagnosis       | NVARCHAR(MAX) | NULL                            | Chẩn đoán                   |
| Treatment       | NVARCHAR(MAX) | NULL                            | Phác đồ điều trị            |
| Notes           | NVARCHAR(MAX) | NULL                            | Ghi chú thêm                |
| CreatedAt       | DATETIME      | NOT NULL, DEFAULT GETDATE()     | Thời điểm tạo record        |

**Relationships:** `AppointmentId` → `Appointments.AppointmentId` (UNIQUE → quan hệ 1-1)

---

### 7. `Prescriptions`

Đơn thuốc kê trong một hồ sơ bệnh án (1 MedicalRecord có nhiều Prescription).

| Column         | Type           | Constraints                    | Note                         |
|----------------|----------------|--------------------------------|------------------------------|
| PrescriptionId | INT            | PK, IDENTITY(1,1)              | Khóa chính                   |
| MedicalRecordId| INT            | NOT NULL, FK → MedicalRecords  | Thuộc hồ sơ bệnh án nào     |
| MedicineName   | NVARCHAR(100)  | NOT NULL                       | Tên thuốc                    |
| Dosage         | NVARCHAR(100)  | NULL                           | Liều lượng (ví dụ: 500mg)   |
| Quantity       | INT            | NULL                           | Số lượng viên/gói            |
| Instruction    | NVARCHAR(500)  | NULL                           | Hướng dẫn dùng thuốc        |

**Relationships:** `MedicalRecordId` → `MedicalRecords.MedicalRecordId`

---

## Entity Relationship Summary

```
Roles (1) ──────────────── (N) Users
Users (1) ──────────────── (1) Doctors
Users (1) ──────────────── (1) Patients
Patients (1) ───────────── (N) Appointments
Doctors (1) ────────────── (N) Appointments
Appointments (1) ───────── (1) MedicalRecords
MedicalRecords (1) ──────── (N) Prescriptions
```

---

## Notes

- **User/Profile pattern:** `Users` là bảng xác thực trung tâm. `Doctors` và `Patients` là bảng profile 1-1 mở rộng — tránh null columns, dễ thêm field mà không ảnh hưởng nhau.
- **Appointment.Status values:** `Pending` → `Confirmed` → `Completed` / `Cancelled`
- **MedicalRecord** chỉ được tạo sau khi Appointment hoàn thành (`Completed`).
- **Prescriptions** là detail lines của MedicalRecord — một record có thể có nhiều loại thuốc.
- Tất cả text tiếng Việt dùng `NVARCHAR`, phone/username dùng `VARCHAR` (ASCII).
