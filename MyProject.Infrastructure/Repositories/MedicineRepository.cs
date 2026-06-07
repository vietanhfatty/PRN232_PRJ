using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Infrastructure.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly HospitalManagementDbContext _context;

    public MedicineRepository(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Medicine>> GetAllAsync()
    {
        return await _context.Medicines.ToListAsync();
    }

    public async Task<Medicine?> GetByIdAsync(int id)
    {
        return await _context.Medicines.FindAsync(id);
    }

    public async Task AddAsync(Medicine medicine)
    {
        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Medicine medicine)
    {
        _context.Entry(medicine).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine != null)
        {
            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<Medicine> GetQueryable()
    {
        return _context.Medicines.AsQueryable();
    }
}
