using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class RoleService
{
    private readonly IRoleRepository _repo;

    public RoleService(IRoleRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(r => new RoleDto(r.RoleId, r.RoleName));
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        return r is null ? null : new RoleDto(r.RoleId, r.RoleName);
    }
    public async Task CreateAsync(CreateRoleRequest req)
    {
        var cleanName = req.RoleName.Trim();
        var existing = await _repo.GetAllAsync();
        if (existing.Any(r => r.RoleName.Equals(cleanName, System.StringComparison.OrdinalIgnoreCase)))
        {
            throw new System.ArgumentException($"Role name '{cleanName}' already exists.");
        }

        var role = new Role
        {
            RoleName = cleanName
        };
        await _repo.AddAsync(role);
    }

    public async Task UpdateAsync(int id, UpdateRoleRequest req)
    {
        var role = await _repo.GetByIdAsync(id)
            ?? throw new System.Collections.Generic.KeyNotFoundException($"Role with ID {id} not found");

        var cleanNewName = req.RoleName.Trim();

        // 1. Prevent editing core roles
        if (IsCoreRole(role.RoleName))
        {
            throw new System.ArgumentException($"Core system role '{role.RoleName}' cannot be renamed.");
        }

        // 2. Prevent renaming another role to a core role name or duplicate name
        if (!role.RoleName.Equals(cleanNewName, System.StringComparison.OrdinalIgnoreCase))
        {
            if (IsCoreRole(cleanNewName))
            {
                throw new System.ArgumentException($"Cannot rename to core system role name '{cleanNewName}'.");
            }

            var existing = await _repo.GetAllAsync();
            if (existing.Any(r => r.RoleName.Equals(cleanNewName, System.StringComparison.OrdinalIgnoreCase)))
            {
                throw new System.ArgumentException($"Role name '{cleanNewName}' already exists.");
            }
        }

        role.RoleName = cleanNewName;
        await _repo.UpdateAsync(role);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _repo.GetByIdAsync(id);
        if (role == null) return;

        // 1. Prevent deleting core roles
        if (IsCoreRole(role.RoleName))
        {
            throw new System.ArgumentException($"Core system role '{role.RoleName}' cannot be deleted.");
        }

        // 2. Prevent deleting roles in use
        if (await _repo.IsInUseAsync(id))
        {
            throw new System.ArgumentException($"Role '{role.RoleName}' cannot be deleted because it is currently assigned to one or more accounts.");
        }

        await _repo.DeleteAsync(id);
    }

    private bool IsCoreRole(string name)
    {
        var coreRoles = new[] { "Admin", "Doctor", "Nurse", "Receptionist" };
        return coreRoles.Any(cr => cr.Equals(name.Trim(), System.StringComparison.OrdinalIgnoreCase));
    }
}
