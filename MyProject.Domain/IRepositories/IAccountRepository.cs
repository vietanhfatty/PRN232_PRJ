using System.Threading.Tasks;
using MyProject.Domain.Entities;

namespace MyProject.Domain.IRepositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByUsernameAsync(string username);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id);
}
