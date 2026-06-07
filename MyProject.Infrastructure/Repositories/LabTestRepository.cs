using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Infrastructure.Repositories;

public class LabTestRepository : ILabTestRepository
{
    private readonly HospitalManagementDbContext _context;

    public LabTestRepository(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LabTest>> GetAllAsync()
    {
        return await _context.LabTests.ToListAsync();
    }

    public async Task<LabTest?> GetByIdAsync(int id)
    {
        return await _context.LabTests.FindAsync(id);
    }

    public async Task AddAsync(LabTest labTest)
    {
        _context.LabTests.Add(labTest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LabTest labTest)
    {
        _context.Entry(labTest).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var labTest = await _context.LabTests.FindAsync(id);
        if (labTest != null)
        {
            _context.LabTests.Remove(labTest);
            await _context.SaveChangesAsync();
        }
    }
}
