using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int? RecordId { get; set; }

    public int? MedicineId { get; set; }

    public int Quantity { get; set; }

    public string Dosage { get; set; } = null!;

    public virtual Medicine? Medicine { get; set; }

    public virtual MedicalRecord? Record { get; set; }
}
