# 🏥 Database Schema — Hospital Management System

> **Database:** `HospitalManagementDB` — SQL Server  
> **Tổng số bảng:** 19 tables, nhóm theo 8 chức năng

---

## 📊 Tổng quan các nhóm bảng

| # | Nhóm | Bảng | Mô tả |
|---|------|-------|-------|
| 1 | Staff Management | `roles`, `staffs`, `accounts` | Nhân sự & phân quyền |
| 2 | Patient & Medical | `patients`, `medical_records` | Bệnh nhân & hồ sơ bệnh án |
| 3 | Appointment | `appointments`, `doctor_schedules` | Lịch hẹn & lịch làm việc |
| 4 | Prescription | `medicines`, `prescriptions`, `prescription_details` | Đơn thuốc & kho dược |
| 5 | Lab | `lab_tests`, `lab_results` | Xét nghiệm |
| 6 | Bed Management | `rooms`, `beds`, `room_allocations` | Phòng & giường bệnh |
| 7 | Inpatient | `admission_orders`, `inpatient_notes` | Nội trú & theo dõi |
| 8 | Billing | `bills`, `bill_items` | Hóa đơn & thanh toán |

---

## 🗺️ Sơ đồ quan hệ (ERD tóm tắt)

```
staffs ──────────── accounts (1-1)
  │                      │
  │ (doctor_id)           └── roles
  ├──────────────────────────────────────────┐
  │                                          │
  ▼                                          ▼
appointments ◄──── patients ──── room_allocations ──── beds ──── rooms
                      │                │
                      ▼                ▼
               medical_records    inpatient_notes
                  │      │
                  ▼      ▼
           prescriptions  lab_results ──── lab_tests
                  │
          prescription_details ──── medicines


patients ──── bills ──── bill_items
medical_records ──── admission_orders
doctor_schedules ──── staffs
```

---

## 📋 Chi tiết từng bảng

---

### NHÓM 1 — Quản lý Nhân sự & Phân quyền

---

#### `roles` — Vai trò hệ thống

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `role_id` | INT IDENTITY | PK | ID tự tăng |
| `role_name` | NVARCHAR(50) | NOT NULL, UNIQUE | Tên vai trò |

**Dữ liệu mẫu:** `Admin`, `Doctor`, `Nurse`, `Receptionist`, `Accountant`, `Pharmacist`

---

#### `staffs` — Nhân viên / Bác sĩ

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `staff_id` | INT IDENTITY | PK | ID tự tăng |
| `first_name` | NVARCHAR(50) | NOT NULL | Tên |
| `last_name` | NVARCHAR(50) | NOT NULL | Họ |
| `specialization` | NVARCHAR(100) | NULL | Chuyên khoa (NULL nếu không phải bác sĩ) |
| `phone` | VARCHAR(15) | NOT NULL, UNIQUE | Số điện thoại |
| `email` | VARCHAR(100) | NULL, UNIQUE | Email |
| `status` | NVARCHAR(20) | DEFAULT `'Active'` | `Active` / `Inactive` |

> `specialization` là NULL cho nhân viên không phải bác sĩ (admin, lễ tân…)

---

#### `accounts` — Tài khoản đăng nhập

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `account_id` | INT IDENTITY | PK | ID tự tăng |
| `staff_id` | INT | FK → `staffs`, UNIQUE | Quan hệ 1-1 với nhân viên |
| `role_id` | INT | FK → `roles` | Phân quyền |
| `username` | VARCHAR(50) | NOT NULL, UNIQUE | Tên đăng nhập |
| `password_hash` | VARCHAR(255) | NOT NULL | Mật khẩu (hash) |
| `is_active` | BIT | DEFAULT `1` | Tài khoản còn hoạt động không |
| `created_at` | DATETIME | DEFAULT `GETDATE()` | Thời gian tạo |

> Quan hệ 1-1 với `staffs` được đảm bảo bởi `UNIQUE(staff_id)`

---

### NHÓM 2 — Quản lý Bệnh nhân & Bệnh án

---

#### `patients` — Bệnh nhân

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `patient_id` | INT IDENTITY | PK | ID tự tăng |
| `first_name` | NVARCHAR(50) | NOT NULL | Tên |
| `last_name` | NVARCHAR(50) | NOT NULL | Họ |
| `gender` | NVARCHAR(10) | CHECK | `Male` / `Female` / `Other` |
| `dob` | DATE | NOT NULL | Ngày sinh |
| `phone` | VARCHAR(15) | NOT NULL, UNIQUE | Số điện thoại |
| `address` | NVARCHAR(255) | NULL | Địa chỉ |
| `insurance_no` | VARCHAR(30) | NULL, UNIQUE | Số BHYT (có thể NULL) |

---

#### `medical_records` — Hồ sơ bệnh án

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `record_id` | INT IDENTITY | PK | ID tự tăng |
| `patient_id` | INT | FK → `patients` | Bệnh nhân |
| `doctor_id` | INT | FK → `staffs` | Bác sĩ phụ trách |
| `visit_date` | DATETIME | DEFAULT `GETDATE()` | Ngày khám |
| `symptoms` | NVARCHAR(MAX) | NOT NULL | Triệu chứng |
| `diagnosis` | NVARCHAR(MAX) | NOT NULL | Chẩn đoán |
| `treatment_plan` | NVARCHAR(MAX) | NULL | Phác đồ điều trị |

> Mỗi lần khám = 1 record. Bệnh nhân có thể có nhiều records.

---

### NHÓM 3 — Lịch hẹn & Tái khám

---

#### `appointments` — Lịch hẹn khám

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `appointment_id` | INT IDENTITY | PK | ID tự tăng |
| `patient_id` | INT | FK → `patients` | Bệnh nhân |
| `doctor_id` | INT | FK → `staffs` | Bác sĩ |
| `appointment_date` | DATETIME | NOT NULL | Ngày giờ hẹn |
| `queue_number` | INT | NULL | Số thứ tự hàng chờ |
| `type` | NVARCHAR(20) | DEFAULT `'First Visit'` | `First Visit` / `Follow-up` |
| `status` | NVARCHAR(20) | DEFAULT `'Scheduled'` | `Scheduled` / `Completed` / `Cancelled` |
| `reason` | NVARCHAR(MAX) | NULL | Lý do khám |

---

#### `doctor_schedules` — Lịch làm việc bác sĩ

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `schedule_id` | INT IDENTITY | PK | ID tự tăng |
| `doctor_id` | INT | FK → `staffs` | Bác sĩ |
| `work_date` | DATE | NOT NULL | Ngày làm việc |
| `shift_name` | NVARCHAR(20) | CHECK | `Morning` / `Afternoon` / `Night` |
| `max_patients` | INT | DEFAULT `20` | Số bệnh nhân tối đa trong ca |

> UNIQUE `(doctor_id, work_date, shift_name)` — 1 bác sĩ không thể có 2 ca trùng ngày+ca

---

### NHÓM 4 — Đơn thuốc & Kho dược

---

#### `medicines` — Danh mục thuốc

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `medicine_id` | INT IDENTITY | PK | ID tự tăng |
| `name` | NVARCHAR(100) | NOT NULL | Tên thuốc |
| `unit` | NVARCHAR(20) | NOT NULL | Đơn vị (Viên, Hộp, Lọ…) |
| `price` | DECIMAL(10,2) | CHECK `>= 0` | Đơn giá |
| `stock_quantity` | INT | DEFAULT `0`, CHECK `>= 0` | Tồn kho hiện tại |

---

#### `prescriptions` — Đơn thuốc

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `prescription_id` | INT IDENTITY | PK | ID tự tăng |
| `record_id` | INT | FK → `medical_records` | Thuộc bệnh án nào |
| `created_at` | DATETIME | DEFAULT `GETDATE()` | Ngày kê đơn |
| `is_dispensed` | BIT | DEFAULT `0` | Đã phát thuốc chưa |
| `dispensed_at` | DATETIME | NULL | Thời điểm phát thuốc |

---

#### `prescription_details` — Chi tiết đơn thuốc

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `prescription_id` | INT | PK + FK → `prescriptions` | Composite PK |
| `medicine_id` | INT | PK + FK → `medicines` | Composite PK |
| `quantity` | INT | CHECK `> 0` | Số lượng |
| `dosage` | NVARCHAR(255) | NOT NULL | Hướng dẫn liều dùng |

> PK tổ hợp `(prescription_id, medicine_id)` — 1 đơn không trùng thuốc

---

### NHÓM 5 — Xét nghiệm

---

#### `lab_tests` — Danh mục dịch vụ xét nghiệm

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `test_id` | INT IDENTITY | PK | ID tự tăng |
| `test_name` | NVARCHAR(100) | NOT NULL | Tên xét nghiệm |
| `cost` | DECIMAL(10,2) | CHECK `>= 0` | Chi phí |

---

#### `lab_results` — Kết quả xét nghiệm

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `result_id` | INT IDENTITY | PK | ID tự tăng |
| `record_id` | INT | FK → `medical_records` | Thuộc bệnh án nào |
| `test_id` | INT | FK → `lab_tests` | Loại xét nghiệm |
| `test_date` | DATETIME | DEFAULT `GETDATE()` | Ngày xét nghiệm |
| `result_summary` | NVARCHAR(MAX) | NOT NULL | Kết quả |
| `attachment_url` | VARCHAR(255) | NULL | Link file đính kèm |

---

### NHÓM 6 — Quản lý Giường bệnh

---

#### `rooms` — Phòng bệnh

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `room_id` | INT IDENTITY | PK | ID tự tăng |
| `room_number` | VARCHAR(10) | NOT NULL, UNIQUE | Số phòng |
| `room_type` | NVARCHAR(20) | CHECK | `VIP` / `Normal` / `ICU` |
| `daily_rate` | DECIMAL(10,2) | CHECK `>= 0` | Giá thuê/ngày |

---

#### `beds` — Giường bệnh

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `bed_id` | INT IDENTITY | PK | ID tự tăng |
| `room_id` | INT | FK → `rooms` | Thuộc phòng nào |
| `bed_number` | VARCHAR(10) | NOT NULL | Số giường trong phòng |
| `is_available` | BIT | DEFAULT `1` | Còn trống không |

---

#### `room_allocations` — Phân phối giường

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `allocation_id` | INT IDENTITY | PK | ID tự tăng |
| `patient_id` | INT | FK → `patients` | Bệnh nhân |
| `bed_id` | INT | FK → `beds` | Giường được phân |
| `admission_date` | DATETIME | DEFAULT `GETDATE()` | Ngày nhập viện |
| `discharge_date` | DATETIME | NULL | Ngày xuất viện (NULL = đang nằm viện) |

> CHECK: `discharge_date IS NULL OR discharge_date >= admission_date`

---

### NHÓM 7 — Nội trú & Theo dõi

---

#### `admission_orders` — Lệnh nhập viện

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `order_id` | INT IDENTITY | PK | ID tự tăng |
| `record_id` | INT | FK → `medical_records` | Từ bệnh án nào |
| `doctor_id` | INT | FK → `staffs` | Bác sĩ ký lệnh |
| `order_date` | DATETIME | DEFAULT `GETDATE()` | Ngày ra lệnh |
| `reason` | NVARCHAR(MAX) | NOT NULL | Lý do nhập viện |
| `status` | NVARCHAR(20) | DEFAULT `'Pending'` | `Pending` / `Allocated` / `Cancelled` |

---

#### `inpatient_notes` — Theo dõi nội trú hàng ngày

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `note_id` | INT IDENTITY | PK | ID tự tăng |
| `allocation_id` | INT | FK → `room_allocations` | Đợt nằm viện nào |
| `pulse` | VARCHAR(10) | NULL | Mạch (nhịp/phút) |
| `temperature` | VARCHAR(10) | NULL | Nhiệt độ (°C) |
| `blood_pressure` | VARCHAR(20) | NULL | Huyết áp (vd: `120/80`) |
| `doctor_notes` | NVARCHAR(MAX) | NULL | Ghi chú bác sĩ |
| `nurse_notes` | NVARCHAR(MAX) | NULL | Ghi chú điều dưỡng |
| `created_at` | DATETIME | DEFAULT `GETDATE()` | Thời điểm ghi |

---

### NHÓM 8 — Hóa đơn & Thanh toán

---

#### `bills` — Hóa đơn

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `bill_id` | INT IDENTITY | PK | ID tự tăng |
| `patient_id` | INT | FK → `patients` | Bệnh nhân |
| `total_amount` | DECIMAL(10,2) | DEFAULT `0` | Tổng tiền trước giảm |
| `insurance_discount` | DECIMAL(10,2) | DEFAULT `0` | Bảo hiểm chi trả |
| `tax_amount` | DECIMAL(10,2) | DEFAULT `0` | Thuế |
| `net_amount` | DECIMAL(10,2) | DEFAULT `0` | Thực thu = total - discount + tax |
| `payment_status` | NVARCHAR(20) | DEFAULT `'Unpaid'` | `Unpaid` / `Paid` / `Partially Paid` |
| `created_at` | DATETIME | DEFAULT `GETDATE()` | Ngày lập hóa đơn |

---

#### `bill_items` — Chi tiết hóa đơn

| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| `item_id` | INT IDENTITY | PK | ID tự tăng |
| `bill_id` | INT | FK → `bills` | Thuộc hóa đơn nào |
| `item_type` | NVARCHAR(50) | CHECK | `ExamFee` / `Medicine` / `LabTest` / `RoomRate` |
| `reference_id` | INT | NOT NULL | ID tham chiếu đến bảng tương ứng |
| `quantity` | INT | DEFAULT `1` | Số lượng |
| `unit_price` | DECIMAL(10,2) | NOT NULL | Đơn giá |
| `sub_total` | Computed | `quantity * unit_price` | Thành tiền (tự tính) |
| `is_paid_gate` | BIT | DEFAULT `0` | Đã qua thanh toán cổng chưa |

> `reference_id` là **polymorphic reference** — trỏ đến `medicines.medicine_id`, `lab_tests.test_id`, hoặc `rooms.room_id` tùy `item_type`

---

## 🔗 Tóm tắt toàn bộ Foreign Keys

| Bảng con | Cột FK | Bảng cha | Ghi chú |
|----------|--------|----------|---------|
| `accounts` | `staff_id` | `staffs` | 1-1 (UNIQUE) |
| `accounts` | `role_id` | `roles` | nhiều-1 |
| `medical_records` | `patient_id` | `patients` | |
| `medical_records` | `doctor_id` | `staffs` | |
| `appointments` | `patient_id` | `patients` | |
| `appointments` | `doctor_id` | `staffs` | |
| `doctor_schedules` | `doctor_id` | `staffs` | |
| `prescriptions` | `record_id` | `medical_records` | |
| `prescription_details` | `prescription_id` | `prescriptions` | Composite PK |
| `prescription_details` | `medicine_id` | `medicines` | Composite PK |
| `lab_results` | `record_id` | `medical_records` | |
| `lab_results` | `test_id` | `lab_tests` | |
| `beds` | `room_id` | `rooms` | |
| `room_allocations` | `patient_id` | `patients` | |
| `room_allocations` | `bed_id` | `beds` | |
| `inpatient_notes` | `allocation_id` | `room_allocations` | |
| `admission_orders` | `record_id` | `medical_records` | |
| `admission_orders` | `doctor_id` | `staffs` | |
| `bills` | `patient_id` | `patients` | |
| `bill_items` | `bill_id` | `bills` | |

---

## 📌 Các giá trị ENUM quan trọng

| Bảng | Cột | Giá trị hợp lệ |
|------|-----|----------------|
| `staffs` | `status` | `Active`, `Inactive` |
| `patients` | `gender` | `Male`, `Female`, `Other` |
| `appointments` | `type` | `First Visit`, `Follow-up` |
| `appointments` | `status` | `Scheduled`, `Completed`, `Cancelled` |
| `doctor_schedules` | `shift_name` | `Morning`, `Afternoon`, `Night` |
| `rooms` | `room_type` | `VIP`, `Normal`, `ICU` |
| `admission_orders` | `status` | `Pending`, `Allocated`, `Cancelled` |
| `bills` | `payment_status` | `Unpaid`, `Paid`, `Partially Paid` |
| `bill_items` | `item_type` | `ExamFee`, `Medicine`, `LabTest`, `RoomRate` |
