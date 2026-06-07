using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

public class RoleApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RoleApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var client = GetClient();
        var response = await client.GetAsync("roles");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RoleDto>>() ?? new List<RoleDto>();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"roles/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RoleDto>();
    }

    public async Task CreateAsync(CreateRoleRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("roles", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task UpdateAsync(int id, UpdateRoleRequest request)
    {
        var client = GetClient();
        var response = await client.PutAsJsonAsync($"roles/{id}", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task DeleteAsync(int id)
    {
        var client = GetClient();
        var response = await client.DeleteAsync($"roles/{id}");
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
