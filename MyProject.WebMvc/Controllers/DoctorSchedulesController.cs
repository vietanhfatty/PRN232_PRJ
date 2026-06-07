using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize]
public class DoctorSchedulesController : Controller
{
    private readonly DoctorScheduleApiService _scheduleService;
    private readonly StaffApiService _staffService;

    public DoctorSchedulesController(DoctorScheduleApiService scheduleService, StaffApiService staffService)
    {
        _scheduleService = scheduleService;
        _staffService = staffService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _scheduleService.GetAllAsync();
        await PopulateDoctorsViewBag();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule == null) return NotFound();
        return View(schedule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDoctorScheduleRequest request)
    {
        // Doctor chỉ được đăng ký schedule cho chính mình
        if (User.IsInRole("Doctor"))
        {
            var staffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdClaim, out var myStaffId))
            {
                TempData["ErrorMessage"] = "Unable to identify your staff account.";
                return RedirectToAction(nameof(Index));
            }
            request = request with { DoctorId = myStaffId };
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _scheduleService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Shift '{request.ShiftName}' on {request.WorkDate:yyyy-MM-dd} assigned successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to assign shift. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDoctorScheduleRequest request)
    {
        // Doctor chỉ được sửa schedule của chính mình
        if (User.IsInRole("Doctor"))
        {
            var staffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdClaim, out var myStaffId))
            {
                TempData["ErrorMessage"] = "Unable to identify your staff account.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _scheduleService.GetByIdAsync(id);
            if (existing == null || existing.DoctorId != myStaffId)
            {
                TempData["ErrorMessage"] = "You can only edit your own schedules.";
                return RedirectToAction(nameof(Index));
            }

            request = request with { DoctorId = myStaffId };
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _scheduleService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = "Shift updated successfully.";
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
            TempData["ErrorMessage"] = "Failed to update shift. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Only Administrators can delete doctor schedules.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _scheduleService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Shift deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete shift. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDoctorsViewBag()
    {
        var staffs = await _staffService.GetAllAsync();
        ViewBag.Doctors = staffs
            .Where(s => s.RoleName == "Doctor" || !string.IsNullOrEmpty(s.Specialization))
            .ToList();

        // Truyền StaffId của user hiện tại để View ẩn dropdown khi là Doctor
        var staffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        ViewBag.CurrentDoctorId = int.TryParse(staffIdClaim, out var sid) ? sid : (int?)null;
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
