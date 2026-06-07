using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class LabTestService
{
    private readonly ILabTestRepository _repo;

    public LabTestService(ILabTestRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<LabTestDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<LabTestDto?> GetByIdAsync(int id)
    {
        var lt = await _repo.GetByIdAsync(id);
        return lt is null ? null : MapToDto(lt);
    }

    public async Task CreateAsync(CreateLabTestRequest req)
    {
        if (req.Cost < 0)
        {
            throw new ArgumentException("Cost must be greater than or equal to 0.");
        }

        var labTest = new LabTest
        {
            TestName = req.TestName.Trim(),
            Cost = req.Cost
        };

        await _repo.AddAsync(labTest);
    }

    public async Task UpdateAsync(int id, UpdateLabTestRequest req)
    {
        var labTest = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"LabTest with ID {id} not found");

        if (req.Cost < 0)
        {
            throw new ArgumentException("Cost must be greater than or equal to 0.");
        }

        labTest.TestName = req.TestName.Trim();
        labTest.Cost = req.Cost;

        await _repo.UpdateAsync(labTest);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    private LabTestDto MapToDto(LabTest lt)
    {
        return new LabTestDto(
            lt.TestId,
            lt.TestName,
            lt.Cost
        );
    }
}
