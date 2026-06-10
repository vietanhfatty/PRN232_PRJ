# Task 6: Program.cs Configuration & Verification

Mục tiêu của task này là đăng ký các Repository và Service mới vào DI Container của cả hai dự án `MyProject.WebApi` và `MyProject.WebMvc` để đảm bảo hệ thống hoạt động thống nhất.

---

## 📋 PLAN: Dependency Injection & Verification

### 1. Cập nhật Program.cs trong WebApi

#### [MODIFY] [Program.cs (WebApi)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebApi/Program.cs)
Đăng ký các Repository và Service mới cho WebAPI:
- **Repositories:**
  ```csharp
  builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
  builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
  ```
- **Services:**
  ```csharp
  builder.Services.AddScoped<MedicalRecordService>();
  ```

---

### 2. Cập nhật Program.cs trong WebMvc

#### [MODIFY] [Program.cs (WebMvc)](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Program.cs)
Đăng ký các Repository, Service trực tiếp và API Client Service:
- **Repositories:**
  ```csharp
  builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
  builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
  ```
- **Services (Local):**
  ```csharp
  builder.Services.AddScoped<MedicalRecordService>();
  ```
- **API Services (HttpClient Clients):**
  ```csharp
  builder.Services.AddScoped<MedicalRecordApiService>();
  ```

---

## 🛠️ Quy Trình Chạy Thử Nghiệm và Xác Minh (Verification)

Sau khi hoàn thành tất cả các thay đổi từ Task 1 đến Task 6, tiến hành các bước kiểm tra sau để nghiệm thu:

### 1. Build Dự Án
Chạy lệnh sau tại thư mục gốc của project để đảm bảo toàn bộ solution build thành công và không lỗi cú pháp:
```bash
dotnet build
```

### 2. Khởi chạy Hệ Thống
Chạy đồng thời cả `MyProject.WebApi` và `MyProject.WebMvc`.
- WebApi chạy cổng mặc định: `https://localhost:7281`
- WebMvc chạy cổng mặc định: `https://localhost:7087` (hoặc cổng cấu hình tương đương).

### 3. Kịch Bản Kiểm Thử Thủ Công (Manual Testing Scenario)
1. **Public Patient Registration:**
   - Truy cập `/Account/Register` (không đăng nhập).
   - Điền đầy đủ thông tin để đăng ký một Patient mới (Ví dụ: `patient_test` / `123456`).
   - Sau khi submit, hệ thống phải thông báo đăng ký thành công và chuyển hướng đến trang `/Account/Login`.
2. **Patient Login & Booking:**
   - Đăng nhập với tài khoản Patient vừa tạo.
   - Kiểm tra menu Sidebar: chỉ có Dashboard, Book Appointment, My History, Medical Records, My Profile. Các menu Admin/Doctor phải bị ẩn.
   - Truy cập "Book Appointment", chọn bác sĩ, điền lý do và gửi đặt lịch.
   - Hệ thống hiển thị lịch hẹn vừa tạo ở dạng `Pending` trên Dashboard và trong "My History".
3. **Doctor Confirmation & Treatment:**
   - Đăng nhập tài khoản Bác sĩ (Ví dụ: `doctora` / `123456`).
   - Vào mục "My Waiting Queue". Do cuộc hẹn của Patient mới tạo đang ở trạng thái `Pending`, nó sẽ không nằm trong Hàng đợi hôm nay (vì hàng đợi lọc theo trạng thái Confirmed).
   - (Tùy chọn) Admin hoặc Bác sĩ duyệt lịch hẹn bằng cách bấm xác nhận trong màn hình quản lý lịch hẹn (đổi sang `Confirmed`).
   - Sau khi cuộc hẹn chuyển sang `Confirmed`, bác sĩ mở "My Waiting Queue" sẽ thấy lịch hẹn của bệnh nhân đó xuất hiện.
   - Bấm nút **"Start Exam"** để bắt đầu khám bệnh. Trạng thái đổi sang `InProgress`.
   - Bấm nút **"Complete"** để kết thúc cuộc khám. Hệ thống tự động đổi trạng thái sang `Completed` và chuyển hướng bác sĩ đến trang tạo Hồ sơ bệnh án `/MedicalRecords/Create?appointmentId=...`.
4. **Create Medical Record & Prescriptions:**
   - Trên trang Tạo hồ sơ bệnh án, bác sĩ nhập: Symptoms (Triệu chứng), Diagnosis (Chẩn đoán), Treatment (Cách điều trị), Notes (Ghi chú).
   - Sử dụng bảng nhập đơn thuốc động để thêm 2 loại thuốc (ví dụ: Paracetamol 500mg - ngày uống 2 viên; Vitamin C - ngày uống 1 viên).
   - Bấm nút **"Save Record"**.
   - Hệ thống lưu thành công hồ sơ kèm đơn thuốc và chuyển hướng về trang chi tiết hồ sơ bệnh án vừa tạo.
5. **Patient Verification:**
   - Đăng nhập lại với tài khoản Patient.
   - Truy cập mục "Medical Records". Hệ thống hiển thị hồ sơ bệnh án vừa được bác sĩ tạo.
   - Bấm vào "Details" để kiểm tra xem chẩn đoán và đơn thuốc hiển thị đúng và đầy đủ không.
   - Thử truy cập thủ công link `/Patients` hoặc `/Roles` bằng tài khoản Patient, hệ thống phải từ chối quyền truy cập (Access Denied hoặc 403).

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Các Service và Repository đã được đăng ký đầy đủ vào DI container trong cả hai file `Program.cs`.
- [ ] Dự án biên dịch thành công mà không có bất kỳ lỗi biên dịch nào.
- [ ] Mọi kịch bản kiểm thử thủ công ở trên đều chạy trôi chảy mà không bị ném exception.
