using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

public class DoctorScheduleApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DoctorScheduleApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<List<DoctorScheduleDto>> GetAllAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("doctorschedules");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        if (json.TrimStart().StartsWith("{") && json.Contains("\"value\""))
        {
            var odata = JsonSerializer.Deserialize<ODataResponse<DoctorScheduleDto>>(json, _jsonOptions);
            return odata?.Value ?? new List<DoctorScheduleDto>();
        }
        return JsonSerializer.Deserialize<List<DoctorScheduleDto>>(json, _jsonOptions) ?? new List<DoctorScheduleDto>();
    }

    public async Task<DoctorScheduleDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"doctorschedules/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DoctorScheduleDto>();
    }

    public async Task CreateAsync(CreateDoctorScheduleRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("doctorschedules", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task UpdateAsync(int id, UpdateDoctorScheduleRequest request)
    {
        var client = GetClient();
        var response = await client.PutAsJsonAsync($"doctorschedules/{id}", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task DeleteAsync(int id)
    {
        var client = GetClient();
        var response = await client.DeleteAsync($"doctorschedules/{id}");
        response.EnsureSuccessStatusCode();
    }

    private static async Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
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
