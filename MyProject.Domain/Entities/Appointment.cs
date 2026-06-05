using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Staff? Doctor { get; set; }

    public virtual MedicalRecord? MedicalRecord { get; set; }

    public virtual Patient? Patient { get; set; }
}
