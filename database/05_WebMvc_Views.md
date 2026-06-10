# Task 5: WebMVC Views (Razor Views)

Task này hướng dẫn tạo các trang giao diện người dùng (.cshtml) cho Cổng thông tin Patient, trang Quản lý Hồ sơ bệnh án và Đơn thuốc, cập nhật sidebar phân quyền và bổ sung nút điều hướng trạng thái trong Queue.

---

## 📋 PLAN: WebMvc Views

### 1. Cập nhật Sidebar và Menu Điều Hướng

#### [MODIFY] [_Layout.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/Shared/_Layout.cshtml)
Thay đổi danh sách liên kết trong menu Sidebar `<ul class="nav-menu">` để hiển thị chính xác theo 3 role:
```cshtml
<ul class="nav-menu">
    @if (User.IsInRole("Admin"))
    {
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Home" ? "active" : "")" asp-controller="Home" asp-action="Index">
                <i class="fa-solid fa-chart-line"></i> Dashboard
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Patients" ? "active" : "")" asp-controller="Patients" asp-action="Index">
                <i class="fa-solid fa-user-injured"></i> Patients
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Appointments" && ViewContext.RouteData.Values["Action"]?.ToString() == "Index" ? "active" : "")" asp-controller="Appointments" asp-action="Index">
                <i class="fa-solid fa-calendar-check"></i> Appointments
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "MedicalRecords" && ViewContext.RouteData.Values["Action"]?.ToString() == "Index" ? "active" : "")" asp-controller="MedicalRecords" asp-action="Index">
                <i class="fa-solid fa-file-medical"></i> Medical Records
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Roles" ? "active" : "")" asp-controller="Roles" asp-action="Index">
                <i class="fa-solid fa-shield-alt"></i> Administration
            </a>
        </li>
    }
    else if (User.IsInRole("Doctor"))
    {
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Appointments" && ViewContext.RouteData.Values["Action"]?.ToString() == "Queue" ? "active" : "")" asp-controller="Appointments" asp-action="Queue">
                <i class="fa-solid fa-people-queue"></i> My Waiting Queue
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Appointments" && ViewContext.RouteData.Values["Action"]?.ToString() == "MyAppointments" ? "active" : "")" asp-controller="Appointments" asp-action="MyAppointments">
                <i class="fa-solid fa-calendar-check"></i> My Appointments
            </a>
        </li>
    }
    else if (User.IsInRole("Patient"))
    {
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PatientPortal" && ViewContext.RouteData.Values["Action"]?.ToString() == "Dashboard" ? "active" : "")" asp-controller="PatientPortal" asp-action="Dashboard">
                <i class="fa-solid fa-chart-line"></i> Dashboard
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PatientPortal" && ViewContext.RouteData.Values["Action"]?.ToString() == "BookAppointment" ? "active" : "")" asp-controller="PatientPortal" asp-action="BookAppointment">
                <i class="fa-solid fa-calendar-plus"></i> Book Appointment
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PatientPortal" && ViewContext.RouteData.Values["Action"]?.ToString() == "MyHistory" ? "active" : "")" asp-controller="PatientPortal" asp-action="MyHistory">
                <i class="fa-solid fa-history"></i> My History
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PatientPortal" && ViewContext.RouteData.Values["Action"]?.ToString() == "MyMedicalRecords" ? "active" : "")" asp-controller="PatientPortal" asp-action="MyMedicalRecords">
                <i class="fa-solid fa-file-prescription"></i> Medical Records
            </a>
        </li>
        <li class="nav-menu-item">
            <a class="nav-menu-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PatientPortal" && ViewContext.RouteData.Values["Action"]?.ToString() == "MyProfile" ? "active" : "")" asp-controller="PatientPortal" asp-action="MyProfile">
                <i class="fa-solid fa-user-circle"></i> My Profile
            </a>
        </li>
    }
</ul>
```

---

### 2. Cập nhật trang hàng đợi của Doctor

#### [MODIFY] [Queue.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/Appointments/Queue.cshtml)
Cập nhật bảng danh sách bệnh nhân để hiển thị các nút điều khiển trạng thái cuộc hẹn:
- Thêm cột trạng thái (Status) trong bảng.
- Hiển thị nút "Start Examination" nếu cuộc hẹn ở trạng thái **Confirmed**.
- Hiển thị nút "Complete" nếu cuộc hẹn ở trạng thái **InProgress**.
```cshtml
@if (item.Status == "Confirmed")
{
    <form asp-action="StartExamination" method="post" class="d-inline">
        <input type="hidden" name="id" value="@item.AppointmentId" />
        @Html.AntiForgeryToken()
        <button type="submit" class="btn btn-sm btn-primary rounded-pill px-3">
            <i class="fa-solid fa-play me-1"></i> Start Exam
        </button>
    </form>
}
else if (item.Status == "InProgress")
{
    <form asp-action="Complete" method="post" class="d-inline">
        <input type="hidden" name="id" value="@item.AppointmentId" />
        @Html.AntiForgeryToken()
        <button type="submit" class="btn btn-sm btn-success rounded-pill px-3">
            <i class="fa-solid fa-check me-1"></i> Complete
        </button>
    </form>
}
```

---

### 3. Tạo mới các View

#### [NEW] [Views/Account/Register.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/Account/Register.cshtml)
Trang đăng ký tài khoản công khai cho Patient (sử dụng giao diện kính mờ/glassmorphism hiện đại tương ứng với trang Login).
- Có đầy đủ các trường: Username, Password, FullName, Phone, DateOfBirth, Gender, Address, BloodType, Emergency Contact.

#### [NEW] [Views/MedicalRecords/Index.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/MedicalRecords/Index.cshtml)
- Giao diện Admin xem toàn bộ hồ sơ bệnh án trong hệ thống.
- Bảng danh sách hiển thị: ID, Bệnh nhân, Bác sĩ, Ngày khám, Chẩn đoán, Hành động (Details).

#### [NEW] [Views/MedicalRecords/Details.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/MedicalRecords/Details.cshtml)
- Hiển thị chi tiết thông tin Hồ sơ bệnh án: Triệu chứng, Chẩn đoán, Phương án điều trị, Ghi chú.
- Danh sách Đơn thuốc kèm theo (Tên thuốc, Liều lượng, Số lượng, Hướng dẫn sử dụng).

#### [NEW] [Views/MedicalRecords/Create.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/MedicalRecords/Create.cshtml)
- Trang Bác sĩ tạo hồ sơ bệnh án sau khi hoàn thành cuộc hẹn.
- Chứa form nhập thông tin hồ sơ và một phần **Dynamic Table** để bác sĩ thêm các dòng thuốc (Medicines) vào đơn thuốc trực tuyến trước khi submit. Danh sách thuốc sẽ được serialize thành JSON và gán vào trường ẩn `medicinesJson`.

#### [NEW] [Views/PatientPortal/Dashboard.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/PatientPortal/Dashboard.cshtml)
- Trang chủ của bệnh nhân: Hiển thị lời chào cá nhân hóa, tóm tắt các cuộc hẹn sắp tới (`UpcomingAppointments`), các nút truy cập nhanh (Đặt lịch khám, Xem Hồ sơ bệnh án, Cập nhật thông tin cá nhân).

#### [NEW] [Views/PatientPortal/BookAppointment.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/PatientPortal/BookAppointment.cshtml)
- Bệnh nhân đăng ký lịch hẹn trực tuyến.
- Dropdown chọn Bác sĩ, chọn Ngày & Giờ khám, nhập lý do khám bệnh.

#### [NEW] [Views/PatientPortal/MyHistory.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/PatientPortal/MyHistory.cshtml)
- Bệnh nhân xem danh sách toàn bộ lịch sử các cuộc hẹn (Pending, Confirmed, Completed, Cancelled).

#### [NEW] [Views/PatientPortal/MyMedicalRecords.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/PatientPortal/MyMedicalRecords.cshtml)
- Bệnh nhân xem danh sách và chi tiết các Hồ sơ bệnh án mà bác sĩ đã chẩn đoán cho họ.

#### [NEW] [Views/PatientPortal/MyProfile.cshtml](file:///d:/PRN212_demo/232/Project_PRN232/MyProject.WebMvc/Views/PatientPortal/MyProfile.cshtml)
- Form cho phép bệnh nhân cập nhật thông tin liên hệ, địa chỉ, nhóm máu và liên hệ khẩn cấp của mình.

---

## ✅ REVIEW: Hướng dẫn kiểm tra

- [ ] Sidebar ẩn/hiện chính xác các danh mục theo Role sau khi đăng nhập.
- [ ] Bấm vào "Complete" trong hàng đợi của bác sĩ sẽ đưa tới trang tạo Hồ sơ bệnh án thành công.
- [ ] Trang tạo Hồ sơ bệnh án hỗ trợ thêm mới/xóa dòng thuốc linh hoạt trước khi gửi đi.
- [ ] Các trang của Cổng thông tin Patient hoạt động bình thường, không xảy ra lỗi Layout.
