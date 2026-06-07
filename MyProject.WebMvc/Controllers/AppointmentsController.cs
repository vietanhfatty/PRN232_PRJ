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
    private readonly StaffApiService _staffService;

    public AppointmentsController(
        AppointmentApiService appointmentService,
        PatientApiService patientService,
        StaffApiService staffService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _appointmentService.GetAllAsync();
        await PopulateDropdownsViewBag();
        return View(list);
    }

    public async Task<IActionResult> Queue()
    {
        var staffIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(staffIdString) || !int.TryParse(staffIdString, out int doctorId))
        {
            TempData["ErrorMessage"] = "Could not identify logged-in staff member.";
            return RedirectToAction(nameof(Index));
        }

        var list = await _appointmentService.GetAllAsync();
        var today = DateTime.Today;
        var queue = list
            .Where(a => a.DoctorId == doctorId && a.Status == "Waiting" && a.AppointmentDate.Date == today)
            .OrderBy(a => a.QueueNumber ?? 0)
            .ToList();

        ViewBag.DoctorName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name;
        return View(queue);
    }

    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return View(appointment);
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

    private async Task PopulateDropdownsViewBag()
    {
        ViewBag.Patients = await _patientService.GetAllAsync();
        var staffs = await _staffService.GetAllAsync();
        ViewBag.Doctors = staffs
            .Where(s => s.RoleName == "Doctor" || !string.IsNullOrEmpty(s.Specialization))
            .ToList();
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
