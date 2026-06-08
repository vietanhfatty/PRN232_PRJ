using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly HospitalManagementDbContext _context;

    public StaffRepository(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _context.Staffs
            .Include(s => s.Account)
                .ThenInclude(a => a.Role)
            .ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _context.Staffs
            .Include(s => s.Account)
                .ThenInclude(a => a.Role)
            .FirstOrDefaultAsync(s => s.StaffId == id);
    }

    public async Task AddAsync(Staff staff)
    {
        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Staff staff)
    {
        _context.Entry(staff).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await _context.Staffs.FindAsync(id);
        if (staff != null)
        {
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Staff?> GetByPhoneAsync(string phone)
    {
        return await _context.Staffs
            .Include(s => s.Account)
                .ThenInclude(a => a.Role)
            .FirstOrDefaultAsync(s => s.Phone == phone);
    }

    public async Task<Staff?> GetByEmailAsync(string email)
    {
        return await _context.Staffs
            .Include(s => s.Account)
                .ThenInclude(a => a.Role)
            .FirstOrDefaultAsync(s => s.Email == email);
    }
}
