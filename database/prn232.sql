CREATE DATABASE HospitalManagementDB_V1;
GO

USE HospitalManagementDB_V1;
GO

-- ====================================
-- ROLES
-- ====================================
CREATE TABLE Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

-- ====================================
-- USERS
-- ====================================
CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL,

    Username VARCHAR(100) NOT NULL UNIQUE,

    PasswordHash NVARCHAR(255) NOT NULL,

    Phone VARCHAR(20),

    RoleId INT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME NULL,

    CONSTRAINT FK_Users_Roles
        FOREIGN KEY(RoleId)
        REFERENCES Roles(RoleId)
);

-- ====================================
-- DOCTORS
-- ====================================
CREATE TABLE Doctors
(
    DoctorId INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL UNIQUE,

    Specialization NVARCHAR(100) NOT NULL,

    ExperienceYears INT DEFAULT 0,

    Description NVARCHAR(1000),

    CONSTRAINT FK_Doctors_Users
        FOREIGN KEY(UserId)
        REFERENCES Users(UserId)
);

-- ====================================
-- PATIENTS
-- ====================================
CREATE TABLE Patients
(
    PatientId INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL UNIQUE,

    DateOfBirth DATE,

    Gender NVARCHAR(10),

    Address NVARCHAR(255),

    BloodType NVARCHAR(10),

    EmergencyContactName NVARCHAR(100),

    EmergencyContactPhone VARCHAR(20),

    CONSTRAINT FK_Patients_Users
        FOREIGN KEY(UserId)
        REFERENCES Users(UserId)
);

-- ====================================
-- APPOINTMENTS
-- ====================================
CREATE TABLE Appointments
(
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,

    PatientId INT NOT NULL,

    DoctorId INT NOT NULL,

    AppointmentDate DATE NOT NULL,

    AppointmentTime TIME NOT NULL,

    Reason NVARCHAR(500),

    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Appointments_Patients
        FOREIGN KEY(PatientId)
        REFERENCES Patients(PatientId),

    CONSTRAINT FK_Appointments_Doctors
        FOREIGN KEY(DoctorId)
        REFERENCES Doctors(DoctorId)
);

-- ====================================
-- MEDICAL RECORDS
-- ====================================
CREATE TABLE MedicalRecords
(
    MedicalRecordId INT IDENTITY(1,1) PRIMARY KEY,

    AppointmentId INT NOT NULL UNIQUE,

    Symptoms NVARCHAR(MAX),

    Diagnosis NVARCHAR(MAX),

    Treatment NVARCHAR(MAX),

    Notes NVARCHAR(MAX),

    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MedicalRecords_Appointments
        FOREIGN KEY(AppointmentId)
        REFERENCES Appointments(AppointmentId)
);

-- ====================================
-- PRESCRIPTIONS
-- ====================================
CREATE TABLE Prescriptions
(
    PrescriptionId INT IDENTITY(1,1) PRIMARY KEY,

    MedicalRecordId INT NOT NULL,

    MedicineName NVARCHAR(100) NOT NULL,

    Dosage NVARCHAR(100),

    Quantity INT,

    Instruction NVARCHAR(500),

    CONSTRAINT FK_Prescriptions_MedicalRecords
        FOREIGN KEY(MedicalRecordId)
        REFERENCES MedicalRecords(MedicalRecordId)
);

-- ====================================
-- SEED DATA
-- ====================================

