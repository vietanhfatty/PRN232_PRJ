using System.Collections.Generic;
using System.Threading.Tasks;
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface ILabTestRepository
{
    Task<IEnumerable<LabTest>> GetAllAsync();
    Task<LabTest?> GetByIdAsync(int id);
    Task AddAsync(LabTest labTest);
    Task UpdateAsync(LabTest labTest);
    Task DeleteAsync(int id);
}
