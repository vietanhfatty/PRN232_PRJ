using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

public class LabTestApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LabTestApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<List<LabTestDto>> GetAllAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("labtests");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        // Handle both plain array and OData wrapper
        if (json.TrimStart().StartsWith("{") && json.Contains("\"value\""))
        {
            var odata = JsonSerializer.Deserialize<ODataResponse<LabTestDto>>(json, _jsonOptions);
            return odata?.Value ?? new List<LabTestDto>();
        }
        return JsonSerializer.Deserialize<List<LabTestDto>>(json, _jsonOptions) ?? new List<LabTestDto>();
    }

    public async Task<LabTestDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"labtests/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LabTestDto>();
    }

    public async Task CreateAsync(CreateLabTestRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("labtests", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task UpdateAsync(int id, UpdateLabTestRequest request)
    {
        var client = GetClient();
        var response = await client.PutAsJsonAsync($"labtests/{id}", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task DeleteAsync(int id)
    {
        var client = GetClient();
        var response = await client.DeleteAsync($"labtests/{id}");
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
