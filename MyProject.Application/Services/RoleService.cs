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
        var role = new Role
        {
            RoleName = req.RoleName
        };
        await _repo.AddAsync(role);
    }

    public async Task UpdateAsync(int id, UpdateRoleRequest req)
    {
        var role = await _repo.GetByIdAsync(id)
            ?? throw new System.Collections.Generic.KeyNotFoundException($"Role with ID {id} not found");
        
        role.RoleName = req.RoleName;
        await _repo.UpdateAsync(role);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }
}
