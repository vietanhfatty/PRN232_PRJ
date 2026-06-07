using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.DTOs;
using MyProject.Application.Services;

namespace MyProject.WebMvc.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleApiService _service;

    public RolesController(RoleApiService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _service.GetAllAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Role name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _service.CreateAsync(request);
            TempData["SuccessMessage"] = $"Role '{request.RoleName}' created successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to create role. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Role name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _service.UpdateAsync(id, request);
            TempData["SuccessMessage"] = $"Role updated to '{request.RoleName}' successfully.";
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to update role. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Role deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to delete role. It may be in use.";
        }
        return RedirectToAction(nameof(Index));
    }
}
