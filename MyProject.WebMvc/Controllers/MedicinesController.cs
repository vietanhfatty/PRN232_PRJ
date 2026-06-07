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
public class MedicinesController : Controller
{
    private readonly MedicineApiService _medicineService;

    public MedicinesController(MedicineApiService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<IActionResult> Index(string? searchName, string? filterUnit, decimal? minPrice, decimal? maxPrice)
    {
        var queryParams = new List<string>();
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchName))
            filters.Add($"contains(tolower(Name), '{searchName.Trim().ToLower()}')");
        if (!string.IsNullOrWhiteSpace(filterUnit) && filterUnit != "All")
            filters.Add($"Unit eq '{filterUnit}'");
        if (minPrice.HasValue)
            filters.Add($"Price ge {minPrice.Value}");
        if (maxPrice.HasValue)
            filters.Add($"Price le {maxPrice.Value}");

        if (filters.Any())
            queryParams.Add($"$filter={string.Join(" and ", filters)}");
        queryParams.Add("$orderby=Name");

        var queryString = string.Join("&", queryParams);

        List<MedicineDto> list = new();
        try
        {
            list = await _medicineService.GetAllAsync(queryString);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to load medicines: {ex.Message}";
        }

        ViewBag.SearchName = searchName;
        ViewBag.FilterUnit = filterUnit;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var medicine = await _medicineService.GetByIdAsync(id);
        if (medicine == null) return NotFound();
        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMedicineRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _medicineService.CreateAsync(request);
            TempData["SuccessMessage"] = $"Medicine '{request.Name}' added successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to add medicine. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMedicineRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = GetFirstModelError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _medicineService.UpdateAsync(id, request);
            TempData["SuccessMessage"] = $"Medicine '{request.Name}' updated successfully.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Medicine not found.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update medicine. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Only Administrators can delete medicines.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _medicineService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Medicine deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete medicine. It may be referenced by existing records.";
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
