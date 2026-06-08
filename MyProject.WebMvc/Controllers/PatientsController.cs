using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly PatientApiService _patientService;
    private readonly AppointmentApiService _appointmentService;

    public PatientsController(PatientApiService patientService, AppointmentApiService appointmentService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index(
        string? searchName, string? searchPhone, string? searchInsurance,
        string? filterGender, DateOnly? filterDobStart, DateOnly? filterDobEnd)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are not authorized to view the general patient list.";
            return RedirectToAction("Queue", "Appointments");
        }

        var list = await _patientService.GetAllAsync();
        var appointments = await _appointmentService.GetAllAsync();
        var today = DateTime.Today;
        ViewBag.TodayAppointments = appointments.Where(a => a.AppointmentDate.Date == today).ToList();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var name = searchName.Trim().ToLower();
            list = list.Where(p => (p.FirstName + " " + p.LastName).ToLower().Contains(name)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(searchPhone))
            list = list.Where(p => p.Phone.Contains(searchPhone.Trim())).ToList();
        if (!string.IsNullOrWhiteSpace(searchInsurance))
            list = list.Where(p => p.InsuranceNo != null && p.InsuranceNo.Contains(searchInsurance.Trim())).ToList();
        if (!string.IsNullOrWhiteSpace(filterGender) && filterGender != "All")
            list = list.Where(p => p.Gender == filterGender).ToList();
        if (filterDobStart.HasValue)
            list = list.Where(p => p.Dob >= filterDobStart.Value).ToList();
        if (filterDobEnd.HasValue)
            list = list.Where(p => p.Dob <= filterDobEnd.Value).ToList();

        ViewBag.SearchName = searchName;
        ViewBag.SearchPhone = searchPhone;
        ViewBag.SearchInsurance = searchInsurance;
        ViewBag.FilterGender = filterGender;
        ViewBag.FilterDobStart = filterDobStart?.ToString("yyyy-MM-dd");
        ViewBag.FilterDobEnd = filterDobEnd?.ToString("yyyy-MM-dd");

        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are not authorized to view detailed patient profiles.";
            return RedirectToAction("Queue", "Appointments");
        }

        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientRequest request)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are not authorized to register patients.";
            return RedirectToAction("Queue", "Appointments");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _patientService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Patient '{request.FirstName} {request.LastName}' registered successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to register patient. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdatePatientRequest request)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are not authorized to edit patient profiles.";
            return RedirectToAction("Queue", "Appointments");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _patientService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = $"Patient '{request.FirstName} {request.LastName}' updated successfully.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Patient not found.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update patient. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Only Administrators can delete patients.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _patientService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Patient deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete patient. They may have existing appointments.";
        }
        return RedirectToAction(nameof(Index));
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
