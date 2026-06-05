using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Staff
{
    public int StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Specialization { get; set; }

    public string? Position { get; set; }

    public int? AccountId { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
