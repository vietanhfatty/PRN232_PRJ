using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface IMedicineRepository
{
    Task<IEnumerable<Medicine>> GetAllAsync();
    Task<Medicine?> GetByIdAsync(int id);
    Task AddAsync(Medicine medicine);
    Task UpdateAsync(Medicine medicine);
    Task DeleteAsync(int id);
    IQueryable<Medicine> GetQueryable();
}
