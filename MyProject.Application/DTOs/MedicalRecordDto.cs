using System;
using System.Collections.Generic;

namespace MyProject.Application.DTOs;

public record MedicalRecordDto(
    int MedicalRecordId,
    int AppointmentId,
    string PatientName,
    string DoctorName,
    string? Symptoms,
    string? Diagnosis,
    string? Treatment,
    string? Notes,
    DateTime CreatedAt,
    List<PrescriptionDto> Prescriptions
);

public record CreateMedicalRecordRequest(
    int AppointmentId,
    string? Symptoms,
    string? Diagnosis,
    string? Treatment,
    string? Notes
);

public record PrescriptionDto(
    int PrescriptionId,
    int MedicalRecordId,
    string MedicineName,
    string? Dosage,
    int? Quantity,
    string? Instruction
);

public record CreatePrescriptionRequest(
    string MedicineName,
    string? Dosage,
    int? Quantity,
    string? Instruction
);
