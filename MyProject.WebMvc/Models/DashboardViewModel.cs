using System.Collections.Generic;
using MyProject.Application.DTOs;

namespace MyProject.WebMvc.Models;

public class DashboardViewModel
{
    public int TotalPatientsCount { get; set; }
    public int ActiveDoctorsCount { get; set; }
    public int WaitingQueueCount { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public List<AppointmentDto> RecentCheckIns { get; set; } = new();
}
