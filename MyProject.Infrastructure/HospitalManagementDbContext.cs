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

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server= VIETANHFATTY\\SQLEXPRESS;uid=sa;password=1234567890;database=HospitalManagementDB;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA586F08E997B");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E47FF69D03").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D1053410430AF6").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Accounts__RoleID__3D5E1FD2");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2EABAE927");

            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK__Appointme__Docto__47DBAE45");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Appointme__Patie__46E78A0C");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C93C01FFC0");

            entity.HasIndex(e => e.AppointmentId, "UQ__MedicalR__8ECDFCA3D9BD1C97").IsUnique();

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.Diagnosis).HasMaxLength(500);
            entity.Property(e => e.DoctorNotes).HasMaxLength(500);
            entity.Property(e => e.TreatmentPlan).HasMaxLength(1000);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__MedicalRe__Appoi__4D94879B");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("PK__Medicine__4F2128F03E9EC125");

            entity.Property(e => e.MedicineId).HasColumnName("MedicineID");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.MedicineName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC3460D9C43DF");

            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HealthInsuranceCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__40130812FD764810");

            entity.Property(e => e.PrescriptionId).HasColumnName("PrescriptionID");
            entity.Property(e => e.Dosage).HasMaxLength(250);
            entity.Property(e => e.MedicineId).HasColumnName("MedicineID");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");

            entity.HasOne(d => d.Medicine).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.MedicineId)
                .HasConstraintName("FK__Prescript__Medic__5535A963");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Prescript__Recor__5441852A");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3AB9EC5165");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61609612719F").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staffs__96D4AAF77AE9E583");

            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Specialization).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.Staff)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Staffs__AccountI__412EB0B6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
