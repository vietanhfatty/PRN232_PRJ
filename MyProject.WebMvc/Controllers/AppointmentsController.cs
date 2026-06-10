using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppointmentApiService _appointmentService;
    private readonly PatientApiService _patientService;
    private readonly DoctorApiService _doctorService;

    public AppointmentsController(
        AppointmentApiService appointmentService,
        PatientApiService patientService,
        DoctorApiService doctorService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Doctor"))
        {
            return RedirectToAction(nameof(Queue));
        }

        var list = await _appointmentService.GetAllAsync();
        await PopulateDropdownsViewBag();
        return View(list);
    }

    public async Task<IActionResult> Queue()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Could not identify logged-in user.";
            return RedirectToAction(nameof(Index));
        }

        var doctors = await _doctorService.GetAllAsync();
        var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Could not identify logged-in doctor profile.";
            return RedirectToAction(nameof(Index));
        }

        var list = await _appointmentService.GetAllAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var queue = list
            .Where(a => a.DoctorId == doctor.DoctorId && a.Status == "Confirmed" && a.AppointmentDate == today)
            .OrderBy(a => a.AppointmentTime)
            .ToList();

        ViewBag.DoctorName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name;
        return View(queue);
    }

    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> MyAppointments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Could not identify logged-in user.";
            return RedirectToAction(nameof(Index));
        }

        var doctors = await _doctorService.GetAllAsync();
        var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Could not identify logged-in doctor profile.";
            return RedirectToAction(nameof(Index));
        }

        var list = await _appointmentService.GetAllAsync();
        var myAppointments = list
            .Where(a => a.DoctorId == doctor.DoctorId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToList();

        ViewBag.DoctorName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name;
        return View(myAppointments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? patientId)
    {
        await PopulateDropdownsViewBag();
        var request = new CreateAppointmentRequest(
            PatientId: patientId ?? 0,
            DoctorId: 0,
            AppointmentDate: DateOnly.FromDateTime(DateTime.Now),
            AppointmentTime: TimeSpan.FromHours(9),
            Status: "Pending",
            Reason: null
        );
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _appointmentService.CreateAsync(request);
            TempData["SuccessMessage"] = "Appointment booked successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to book appointment. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var appt = await _appointmentService.GetByIdAsync(id);
        if (appt == null) return NotFound();

        await PopulateDropdownsViewBag();

        var request = new UpdateAppointmentRequest(
            PatientId: appt.PatientId,
            DoctorId: appt.DoctorId,
            AppointmentDate: appt.AppointmentDate,
            AppointmentTime: appt.AppointmentTime,
            Status: appt.Status,
            Reason: appt.Reason
        );

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateAppointmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _appointmentService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = "Appointment updated successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update appointment. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Only Administrators can delete appointments.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _appointmentService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Appointment deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete appointment. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id)
    {
        try
        {
            await _appointmentService.CheckInAsync(id);
            TempData["SuccessMessage"] = "Check-in successful! Patient has been queued.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleFollowup(int patientId, DateTime appointmentDate, string? reason)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Could not identify logged-in user.";
            return RedirectToAction(nameof(Index));
        }

        var doctors = await _doctorService.GetAllAsync();
        var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
        if (doctor == null)
        {
            TempData["ErrorMessage"] = "Could not identify logged-in doctor profile.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var request = new CreateAppointmentRequest(
                PatientId: patientId,
                DoctorId: doctor.DoctorId,
                AppointmentDate: DateOnly.FromDateTime(appointmentDate),
                AppointmentTime: TimeSpan.FromHours(9),
                Status: "Pending",
                Reason: string.IsNullOrWhiteSpace(reason) ? "Follow-up consultation booked by Doctor" : reason
            );

            await _appointmentService.CreateAsync(request);
            TempData["SuccessMessage"] = "Follow-up appointment scheduled successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(MyAppointments));
    }

    private async Task PopulateDropdownsViewBag()
    {
        ViewBag.Patients = await _patientService.GetAllAsync();
        ViewBag.Doctors = await _doctorService.GetAllAsync();
    }

    private string GetFirstModelError()
    {
        foreach (var state in ModelState.Values)
            foreach (var error in state.Errors)
                if (!string.IsNullOrEmpty(error.ErrorMessage))
                    return error.ErrorMessage;
        return "Invalid input. Please check your data.";
    }
}
