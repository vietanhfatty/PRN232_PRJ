using Microsoft.AspNetCore.Mvc;
using MyProject.WebMvc.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using MyProject.Application.Services;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace MyProject.WebMvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PatientService _patientService;
        private readonly StaffService _staffService;
        private readonly AppointmentService _appointmentService;

        public HomeController(
            ILogger<HomeController> logger,
            PatientService patientService,
            StaffService staffService,
            AppointmentService appointmentService)
        {
            _logger = logger;
            _patientService = patientService;
            _staffService = staffService;
            _appointmentService = appointmentService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Doctor"))
            {
                return RedirectToAction("Queue", "Appointments");
            }

            var patients = await _patientService.GetAllAsync();
            var staff = await _staffService.GetAllAsync();
            var appointments = await _appointmentService.GetAllAsync();

            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                TotalPatientsCount = patients.Count(),
                ActiveDoctorsCount = staff.Count(s => s.Status == "Active" && (s.RoleName == "Doctor" || !string.IsNullOrEmpty(s.Specialization))),
                WaitingQueueCount = appointments.Count(a => a.Status == "Waiting" && a.AppointmentDate.Date == today),
                TodayAppointmentsCount = appointments.Count(a => a.AppointmentDate.Date == today),
                RecentCheckIns = appointments
                    .Where(a => a.AppointmentDate.Date == today)
                    .OrderByDescending(a => a.AppointmentId)
                    .Take(5)
                    .ToList()
            };

            // Fallback recent check-ins if no appointments today to make UI look complete
            if (!viewModel.RecentCheckIns.Any())
            {
                viewModel.RecentCheckIns = appointments
                    .OrderByDescending(a => a.AppointmentId)
                    .Take(5)
                    .ToList();
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
