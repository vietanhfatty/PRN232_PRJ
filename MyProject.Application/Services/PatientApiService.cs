using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

// Helper wrapper to handle OData response format
public class ODataResponse<T>
{
    public List<T> Value { get; set; } = new();
}

// Mutable raw class for deserializing Patient entity JSON (handles OData + plain)
public class PatientDtoRaw
{
    public int PatientId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Phone { get; set; }
    public string? DateOfBirth { get; set; } // DateOnly serialized as string
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class PatientApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PatientApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    private PatientDto MapRaw(PatientDtoRaw raw)
    {
        DateOnly? dob = null;
        if (!string.IsNullOrEmpty(raw.DateOfBirth) && DateOnly.TryParse(raw.DateOfBirth, out var parsedDob))
        {
            dob = parsedDob;
        }
        return new PatientDto(
            raw.PatientId,
            raw.UserId,
            raw.FullName,
            raw.Username,
            raw.Phone,
            dob,
            raw.Gender,
            raw.Address,
            raw.BloodType,
            raw.EmergencyContactName,
            raw.EmergencyContactPhone
        );
    }

    public async Task<List<PatientDto>> GetAllAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("patients");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // Try OData format first { "value": [...] }
        if (json.TrimStart().StartsWith("{") && json.Contains("\"value\""))
        {
            var odataResult = JsonSerializer.Deserialize<ODataResponse<PatientDtoRaw>>(json, _jsonOptions);
            var rawList = odataResult?.Value ?? new List<PatientDtoRaw>();
            var result = new List<PatientDto>();
            foreach (var raw in rawList) result.Add(MapRaw(raw));
            return result;
        }

        // Plain array format
        var plainList = JsonSerializer.Deserialize<List<PatientDtoRaw>>(json, _jsonOptions) ?? new List<PatientDtoRaw>();
        var list = new List<PatientDto>();
        foreach (var raw in plainList) list.Add(MapRaw(raw));
        return list;
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"patients/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var raw = JsonSerializer.Deserialize<PatientDtoRaw>(json, _jsonOptions);
        if (raw == null) return null;
        return MapRaw(raw);
    }

    public async Task CreateAsync(CreatePatientRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("patients", request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            // Try extract Message from { "Message": "..." }
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("Message", out var msg))
                    throw new ArgumentException(msg.GetString());
            }
            catch (ArgumentException) { throw; }
            catch { }
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest request)
    {
        var client = GetClient();
        var response = await client.PutAsJsonAsync($"patients/{id}", request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("Message", out var msg))
                    throw new ArgumentException(msg.GetString());
            }
            catch (ArgumentException) { throw; }
            catch { }
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var client = GetClient();
        var response = await client.DeleteAsync($"patients/{id}");
        response.EnsureSuccessStatusCode();
    }
}
