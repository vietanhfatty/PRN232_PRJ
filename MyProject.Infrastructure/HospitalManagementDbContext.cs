using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyProject.Domain.Entities;

public partial class HospitalManagementDbContext : DbContext
{
    public HospitalManagementDbContext()
    {
    }

    public HospitalManagementDbContext(DbContextOptions<HospitalManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AdmissionOrder> AdmissionOrders { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Bed> Beds { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillItem> BillItems { get; set; }

    public virtual DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public virtual DbSet<InpatientNote> InpatientNotes { get; set; }

    public virtual DbSet<LabResult> LabResults { get; set; }

    public virtual DbSet<LabTest> LabTests { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAllocation> RoomAllocations { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=VIETANHFATTY\\SQLEXPRESS;uid=sa;password=1234567890;database=HospitalManagementDB;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");

            entity.HasIndex(e => e.StaffId, "UC_accounts_staff").IsUnique();

            entity.HasIndex(e => e.Username, "UC_accounts_username").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accounts_roles");

            entity.HasOne(d => d.Staff).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accounts_staffs");
        });

        modelBuilder.Entity<AdmissionOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.ToTable("admission_orders");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Doctor).WithMany(p => p.AdmissionOrders)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_admissions_staffs");

            entity.HasOne(d => d.Record).WithMany(p => p.AdmissionOrders)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_admissions_records");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");

            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.AppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("appointment_date");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.QueueNumber).HasColumnName("queue_number");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Scheduled")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasDefaultValue("First Visit")
                .HasColumnName("type");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appointments_staffs");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_appointments_patients");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.ToTable("beds");

            entity.Property(e => e.BedId).HasColumnName("bed_id");
            entity.Property(e => e.BedNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("bed_number");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(true)
                .HasColumnName("is_available");
            entity.Property(e => e.RoomId).HasColumnName("room_id");

            entity.HasOne(d => d.Room).WithMany(p => p.Beds)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_beds_rooms");
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.ToTable("bills");

            entity.Property(e => e.BillId).HasColumnName("bill_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InsuranceDiscount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("insurance_discount");
            entity.Property(e => e.NetAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("net_amount");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid")
                .HasColumnName("payment_status");
            entity.Property(e => e.TaxAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Patient).WithMany(p => p.Bills)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bills_patients");
        });

        modelBuilder.Entity<BillItem>(entity =>
        {
            entity.HasKey(e => e.ItemId);

            entity.ToTable("bill_items");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.BillId).HasColumnName("bill_id");
            entity.Property(e => e.IsPaidGate)
                .HasDefaultValue(false)
                .HasColumnName("is_paid_gate");
            entity.Property(e => e.ItemType)
                .HasMaxLength(50)
                .HasColumnName("item_type");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([quantity]*[unit_price])", false)
                .HasColumnType("decimal(21, 2)")
                .HasColumnName("sub_total");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillItems)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_items_bills");
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);

            entity.ToTable("doctor_schedules");

            entity.HasIndex(e => new { e.DoctorId, e.WorkDate, e.ShiftName }, "UC_doctor_date_shift").IsUnique();

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.MaxPatients)
                .HasDefaultValue(20)
                .HasColumnName("max_patients");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(20)
                .HasColumnName("shift_name");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorSchedules)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedules_staffs");
        });

        modelBuilder.Entity<InpatientNote>(entity =>
        {
            entity.HasKey(e => e.NoteId);

            entity.ToTable("inpatient_notes");

            entity.Property(e => e.NoteId).HasColumnName("note_id");
            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.BloodPressure)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("blood_pressure");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorNotes).HasColumnName("doctor_notes");
            entity.Property(e => e.NurseNotes).HasColumnName("nurse_notes");
            entity.Property(e => e.Pulse)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("pulse");
            entity.Property(e => e.Temperature)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("temperature");

            entity.HasOne(d => d.Allocation).WithMany(p => p.InpatientNotes)
                .HasForeignKey(d => d.AllocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_notes_allocations");
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.ToTable("lab_results");

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("attachment_url");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.ResultSummary).HasColumnName("result_summary");
            entity.Property(e => e.TestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("test_date");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Record).WithMany(p => p.LabResults)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_results_records");

            entity.HasOne(d => d.Test).WithMany(p => p.LabResults)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_results_tests");
        });

        modelBuilder.Entity<LabTest>(entity =>
        {
            entity.HasKey(e => e.TestId);

            entity.ToTable("lab_tests");

            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.TestName)
                .HasMaxLength(100)
                .HasColumnName("test_name");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId);

            entity.ToTable("medical_records");

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.TreatmentPlan).HasColumnName("treatment_plan");
            entity.Property(e => e.VisitDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("visit_date");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_records_staffs");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_records_patients");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.ToTable("medicines");

            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");

            entity.HasIndex(e => e.InsuranceNo, "UC_patients_insurance").IsUnique();

            entity.HasIndex(e => e.Phone, "UC_patients_phone").IsUnique();

            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.InsuranceNo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("insurance_no");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.ToTable("prescriptions");

            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DispensedAt)
                .HasColumnType("datetime")
                .HasColumnName("dispensed_at");
            entity.Property(e => e.IsDispensed)
                .HasDefaultValue(false)
                .HasColumnName("is_dispensed");
            entity.Property(e => e.RecordId).HasColumnName("record_id");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_prescriptions_records");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => new { e.PrescriptionId, e.MedicineId });

            entity.ToTable("prescription_details");

            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.MedicineId).HasColumnName("medicine_id");
            entity.Property(e => e.Dosage)
                .HasMaxLength(255)
                .HasColumnName("dosage");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Medicine).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_details_medicines");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_details_prescriptions");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UC_roles_name").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");

            entity.HasIndex(e => e.RoomNumber, "UC_rooms_number").IsUnique();

            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.DailyRate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("daily_rate");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("room_number");
            entity.Property(e => e.RoomType)
                .HasMaxLength(20)
                .HasColumnName("room_type");
        });

        modelBuilder.Entity<RoomAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId);

            entity.ToTable("room_allocations");

            entity.Property(e => e.AllocationId).HasColumnName("allocation_id");
            entity.Property(e => e.AdmissionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("admission_date");
            entity.Property(e => e.BedId).HasColumnName("bed_id");
            entity.Property(e => e.DischargeDate)
                .HasColumnType("datetime")
                .HasColumnName("discharge_date");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");

            entity.HasOne(d => d.Bed).WithMany(p => p.RoomAllocations)
                .HasForeignKey(d => d.BedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_allocations_beds");

            entity.HasOne(d => d.Patient).WithMany(p => p.RoomAllocations)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_allocations_patients");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("staffs");

            entity.HasIndex(e => e.Email, "UC_staffs_email").IsUnique();

            entity.HasIndex(e => e.Phone, "UC_staffs_phone").IsUnique();

            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
