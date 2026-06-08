using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize(Roles = "Admin")]
public class StaffsController : Controller
{
    private readonly StaffApiService _staffService;
    private readonly RoleApiService _roleService;

    public StaffsController(StaffApiService staffService, RoleApiService roleService)
    {
        _staffService = staffService;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _staffService.GetAllAsync();
        ViewBag.Roles = await _roleService.GetAllAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var staff = await _staffService.GetByIdAsync(id);
        if (staff == null) return NotFound();
        return View(staff);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _staffService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Staff member '{request.FirstName} {request.LastName}' created successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to create staff member. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStaffRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        // Self-modification safeguards
        var currentStaffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(currentStaffIdClaim, out var currentStaffId) && currentStaffId == id)
        {
            if (request.Status != "Active")
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own staff record.";
                return RedirectToAction(nameof(Index));
            }

            var currentStaff = await _staffService.GetByIdAsync(id);
            if (currentStaff != null)
            {
                if (currentStaff.RoleId != request.RoleId)
                {
                    TempData["ErrorMessage"] = "You cannot change your own system role.";
                    return RedirectToAction(nameof(Index));
                }

                if (currentStaff.Username != request.Username)
                {
                    TempData["ErrorMessage"] = "You cannot change your own username.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        try
        {
            await _staffService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = $"Staff member '{request.FirstName} {request.LastName}' updated successfully.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Staff member not found.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update staff member. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Self-deletion safeguard
        var currentStaffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(currentStaffIdClaim, out var currentStaffId) && currentStaffId == id)
        {
            TempData["ErrorMessage"] = "You cannot delete your own staff record.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _staffService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Staff member deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete staff member. They may have existing records.";
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
