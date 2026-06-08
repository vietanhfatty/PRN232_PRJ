using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize]
public class LabTestsController : Controller
{
    private readonly LabTestApiService _labTestService;

    public LabTestsController(LabTestApiService labTestService)
    {
        _labTestService = labTestService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _labTestService.GetAllAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var labTest = await _labTestService.GetByIdAsync(id);
        if (labTest == null) return NotFound();
        return View(labTest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLabTestRequest request)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are only allowed to view lab tests, not modify them.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _labTestService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Lab test '{request.TestName}' added successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to add lab test. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateLabTestRequest request)
    {
        if (User.IsInRole("Doctor"))
        {
            TempData["ErrorMessage"] = "Doctors are only allowed to view lab tests, not modify them.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _labTestService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = $"Lab test '{request.TestName}' updated successfully.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Lab test not found.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update lab test. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Only Administrators can delete lab tests.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _labTestService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Lab test deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete lab test. It may be referenced by existing records.";
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
