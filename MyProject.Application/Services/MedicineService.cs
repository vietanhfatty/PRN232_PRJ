using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class MedicineService
{
    private readonly IMedicineRepository _repo;

    public MedicineService(IMedicineRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MedicineDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var m = await _repo.GetByIdAsync(id);
        return m is null ? null : MapToDto(m);
    }

    public async Task CreateAsync(CreateMedicineRequest req)
    {
        if (req.Price < 0)
        {
            throw new ArgumentException("Price must be greater than or equal to 0.");
        }
        if (req.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity must be greater than or equal to 0.");
        }

        var medicine = new Medicine
        {
            Name = req.Name.Trim(),
            Unit = req.Unit.Trim(),
            Price = req.Price,
            StockQuantity = req.StockQuantity
        };

        await _repo.AddAsync(medicine);
    }

    public async Task UpdateAsync(int id, UpdateMedicineRequest req)
    {
        var medicine = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medicine with ID {id} not found");

        if (req.Price < 0)
        {
            throw new ArgumentException("Price must be greater than or equal to 0.");
        }
        if (req.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity must be greater than or equal to 0.");
        }

        medicine.Name = req.Name.Trim();
        medicine.Unit = req.Unit.Trim();
        medicine.Price = req.Price;
        medicine.StockQuantity = req.StockQuantity;

        await _repo.UpdateAsync(medicine);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    public IQueryable<Medicine> GetQueryable()
    {
        return _repo.GetQueryable();
    }

    private MedicineDto MapToDto(Medicine m)
    {
        return new MedicineDto(
            m.MedicineId,
            m.Name,
            m.Unit,
            m.Price,
            m.StockQuantity
        );
    }
}
