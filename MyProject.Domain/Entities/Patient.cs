using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Patient
{
    public int PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? Address { get; set; }

    public string? HealthInsuranceCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
