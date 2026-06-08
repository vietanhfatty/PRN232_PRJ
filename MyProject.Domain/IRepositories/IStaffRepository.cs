using System.Collections.Generic;
using System.Threading.Tasks;
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task AddAsync(Staff staff);
    Task UpdateAsync(Staff staff);
    Task DeleteAsync(int id);
    Task<Staff?> GetByPhoneAsync(string phone);
    Task<Staff?> GetByEmailAsync(string email);
}
