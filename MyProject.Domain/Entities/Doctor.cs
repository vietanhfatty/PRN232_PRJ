using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public int UserId { get; set; }

    public string Specialization { get; set; } = null!;

    public int ExperienceYears { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
