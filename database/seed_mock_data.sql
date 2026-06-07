-- 🏥 Seed Mock Data Script for HospitalManagementDB
-- Password for all accounts is: 123456 (SHA256 Hash: 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92)

USE HospitalManagementDB;
GO

-- 0. Update any legacy plain-text '123456' passwords to correct SHA256 hashes
UPDATE accounts
SET password_hash = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'
WHERE password_hash = '123456';
GO

-- 1. Insert Roles (if they do not exist)
IF NOT EXISTS (SELECT * FROM roles WHERE role_name = 'Admin')
    INSERT INTO roles (role_name) VALUES ('Admin');
IF NOT EXISTS (SELECT * FROM roles WHERE role_name = 'Doctor')
    INSERT INTO roles (role_name) VALUES ('Doctor');
IF NOT EXISTS (SELECT * FROM roles WHERE role_name = 'Nurse')
    INSERT INTO roles (role_name) VALUES ('Nurse');
IF NOT EXISTS (SELECT * FROM roles WHERE role_name = 'Receptionist')
    INSERT INTO roles (role_name) VALUES ('Receptionist');
IF NOT EXISTS (SELECT * FROM roles WHERE role_name = 'Pharmacist')
    INSERT INTO roles (role_name) VALUES ('Pharmacist');
GO

-- 2. Insert Staff members
-- Admin Staff
IF NOT EXISTS (SELECT * FROM staffs WHERE phone = '0987654321' OR email = 'admin@hospital.com')
    INSERT INTO staffs (first_name, last_name, specialization, phone, email, status)
    VALUES (N'Admin', N'User', NULL, '0987654321', 'admin@hospital.com', 'Active');

-- Doctor Staff
IF NOT EXISTS (SELECT * FROM staffs WHERE phone = '0912345678' OR email = 'fleming@hospital.com')
    INSERT INTO staffs (first_name, last_name, specialization, phone, email, status)
    VALUES (N'Alexander', N'Fleming', N'Cardiology', '0912345678', 'fleming@hospital.com', 'Active');

-- Receptionist Staff
IF NOT EXISTS (SELECT * FROM staffs WHERE phone = '0901234567' OR email = 'sarah@hospital.com')
    INSERT INTO staffs (first_name, last_name, specialization, phone, email, status)
    VALUES (N'Sarah', N'Connor', NULL, '0901234567', 'sarah@hospital.com', 'Active');
GO

-- 3. Insert Accounts linked to the Staff members
DECLARE @AdminRoleId INT = (SELECT role_id FROM roles WHERE role_name = 'Admin');
DECLARE @DoctorRoleId INT = (SELECT role_id FROM roles WHERE role_name = 'Doctor');
DECLARE @ReceptionistRoleId INT = (SELECT role_id FROM roles WHERE role_name = 'Receptionist');

DECLARE @AdminStaffId INT = (SELECT staff_id FROM staffs WHERE phone = '0987654321' OR email = 'admin@hospital.com');
DECLARE @DoctorStaffId INT = (SELECT staff_id FROM staffs WHERE phone = '0912345678' OR email = 'fleming@hospital.com');
DECLARE @ReceptionistStaffId INT = (SELECT staff_id FROM staffs WHERE phone = '0901234567' OR email = 'sarah@hospital.com');

-- Insert Admin Account (Username: admin, Password: 123456)
IF @AdminStaffId IS NOT NULL AND NOT EXISTS (SELECT * FROM accounts WHERE staff_id = @AdminStaffId)
    INSERT INTO accounts (staff_id, role_id, username, password_hash, is_active, created_at)
    VALUES (@AdminStaffId, @AdminRoleId, 'admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 1, GETDATE());

-- Insert Doctor Account (Username: doctor, Password: 123456)
IF @DoctorStaffId IS NOT NULL AND NOT EXISTS (SELECT * FROM accounts WHERE staff_id = @DoctorStaffId)
    INSERT INTO accounts (staff_id, role_id, username, password_hash, is_active, created_at)
    VALUES (@DoctorStaffId, @DoctorRoleId, 'doctor', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 1, GETDATE());

-- Insert Receptionist Account (Username: receptionist, Password: 123456)
IF @ReceptionistStaffId IS NOT NULL AND NOT EXISTS (SELECT * FROM accounts WHERE staff_id = @ReceptionistStaffId)
    INSERT INTO accounts (staff_id, role_id, username, password_hash, is_active, created_at)
    VALUES (@ReceptionistStaffId, @ReceptionistRoleId, 'receptionist', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 1, GETDATE());
GO

-- 4. Insert Mock Patients
IF NOT EXISTS (SELECT * FROM patients WHERE phone = '0944444444' OR insurance_no = 'GD4012345678')
    INSERT INTO patients (first_name, last_name, gender, dob, phone, address, insurance_no)
    VALUES (N'Anh', N'Nguyen Van', 'Male', '1995-05-15', '0944444444', N'123 Nguyen Trai, Hanoi', 'GD4012345678');

IF NOT EXISTS (SELECT * FROM patients WHERE phone = '0955555555' OR insurance_no = 'GD4087654321')
    INSERT INTO patients (first_name, last_name, gender, dob, phone, address, insurance_no)
    VALUES (N'Binh', N'Tran Thi', 'Female', '1998-08-20', '0955555555', N'456 Le Loi, Da Nang', 'GD4087654321');
GO

-- 5. Insert Mock Medicines
IF NOT EXISTS (SELECT * FROM medicines WHERE name = 'Paracetamol 500mg')
    INSERT INTO medicines (name, unit, price, stock_quantity)
    VALUES ('Paracetamol 500mg', 'Tablet', 2000.00, 500);

IF NOT EXISTS (SELECT * FROM medicines WHERE name = 'Amoxicillin 250mg')
    INSERT INTO medicines (name, unit, price, stock_quantity)
    VALUES ('Amoxicillin 250mg', 'Capsule', 5000.00, 150);

IF NOT EXISTS (SELECT * FROM medicines WHERE name = 'Decolgen Forte')
    INSERT INTO medicines (name, unit, price, stock_quantity)
    VALUES ('Decolgen Forte', 'Tablet', 3000.00, 200);
GO

-- 6. Insert Mock Lab Tests
IF NOT EXISTS (SELECT * FROM lab_tests WHERE test_name = 'Complete Blood Count (CBC)')
    INSERT INTO lab_tests (test_name, cost)
    VALUES ('Complete Blood Count (CBC)', 120000.00);

IF NOT EXISTS (SELECT * FROM lab_tests WHERE test_name = 'Urine Analysis')
    INSERT INTO lab_tests (test_name, cost)
    VALUES ('Urine Analysis', 80000.00);

IF NOT EXISTS (SELECT * FROM lab_tests WHERE test_name = 'Chest X-Ray')
    INSERT INTO lab_tests (test_name, cost)
    VALUES ('Chest X-Ray', 250000.00);
GO

PRINT 'Mock data seeded successfully!';
