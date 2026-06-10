using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

public class MedicalRecordApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    public MedicalRecordApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<List<MedicalRecordDto>> GetAllAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("medicalrecords");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>() ?? new List<MedicalRecordDto>();
    }

    public async Task<MedicalRecordDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"medicalrecords/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
    }

    public async Task<MedicalRecordDto?> GetByAppointmentIdAsync(int appointmentId)
    {
        var client = GetClient();
        var response = await client.GetAsync($"medicalrecords/by-appointment/{appointmentId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
    }

    public async Task<List<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
    {
        var client = GetClient();
        var response = await client.GetAsync($"medicalrecords/by-patient/{patientId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>() ?? new List<MedicalRecordDto>();
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("medicalrecords", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MedicalRecordDto>() ?? throw new InvalidOperationException("Failed to deserialize created record.");
    }

    public async Task AddPrescriptionsAsync(int medicalRecordId, List<CreatePrescriptionRequest> prescriptions)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync($"medicalrecords/{medicalRecordId}/prescriptions", prescriptions);
        response.EnsureSuccessStatusCode();
    }
}
