using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Infrastructure.Repositories;

public class DoctorScheduleRepository : IDoctorScheduleRepository
{
    private readonly HospitalManagementDbContext _context;

    public DoctorScheduleRepository(HospitalManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DoctorSchedule>> GetAllAsync()
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Doctor)
            .ToListAsync();
    }

    public async Task<DoctorSchedule?> GetByIdAsync(int id)
    {
        return await _context.DoctorSchedules
            .Include(ds => ds.Doctor)
            .FirstOrDefaultAsync(ds => ds.ScheduleId == id);
    }

    public async Task AddAsync(DoctorSchedule schedule)
    {
        _context.DoctorSchedules.Add(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DoctorSchedule schedule)
    {
        _context.Entry(schedule).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule != null)
        {
            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<DoctorSchedule?> GetByDoctorDateShiftAsync(int doctorId, DateOnly workDate, string shiftName)
    {
        return await _context.DoctorSchedules
            .FirstOrDefaultAsync(ds => ds.DoctorId == doctorId && ds.WorkDate == workDate && ds.ShiftName == shiftName);
    }
}
