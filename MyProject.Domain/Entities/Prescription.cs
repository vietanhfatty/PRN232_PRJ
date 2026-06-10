using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int MedicalRecordId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string? Dosage { get; set; }

    public int? Quantity { get; set; }

    public string? Instruction { get; set; }

    public virtual MedicalRecord MedicalRecord { get; set; } = null!;
}
