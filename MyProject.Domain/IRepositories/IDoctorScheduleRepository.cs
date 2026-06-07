using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface IDoctorScheduleRepository
{
    Task<IEnumerable<DoctorSchedule>> GetAllAsync();
    Task<DoctorSchedule?> GetByIdAsync(int id);
    Task AddAsync(DoctorSchedule schedule);
    Task UpdateAsync(DoctorSchedule schedule);
    Task DeleteAsync(int id);
    Task<DoctorSchedule?> GetByDoctorDateShiftAsync(int doctorId, DateOnly workDate, string shiftName);
}
