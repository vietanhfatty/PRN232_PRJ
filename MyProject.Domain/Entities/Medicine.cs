using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
