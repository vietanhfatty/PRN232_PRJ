using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

// Used to deserialize OData-style response { "value": [...] }
public class ODataResponse<T>
{
    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();
}

// Mutable class used for deserializing from both Medicine entity and MedicineDto JSON
public class MedicineDtoRaw
{
    public int MedicineId { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class MedicineApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MedicineApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<List<MedicineDto>> GetAllAsync(string? queryString = null)
    {
        var client = GetClient();
        var url = "medicines";
        if (!string.IsNullOrWhiteSpace(queryString))
            url += $"?{queryString}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        List<MedicineDtoRaw>? rawList = null;

        // Thử parse plain array trước (API không có EDM model trả về [...])
        if (json.TrimStart().StartsWith("["))
        {
            rawList = JsonSerializer.Deserialize<List<MedicineDtoRaw>>(json, _jsonOptions);
        }
        else
        {
            // Fallback: OData format { "value": [...] }
            var odataResult = JsonSerializer.Deserialize<ODataResponse<MedicineDtoRaw>>(json, _jsonOptions);
            rawList = odataResult?.Value;
        }

        rawList ??= new List<MedicineDtoRaw>();

        var result = new List<MedicineDto>();
        foreach (var raw in rawList)
            result.Add(new MedicineDto(raw.MedicineId, raw.Name, raw.Unit, raw.Price, raw.StockQuantity));
        return result;
    }

    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var client = GetClient();
        var response = await client.GetAsync($"medicines/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var raw = JsonSerializer.Deserialize<MedicineDtoRaw>(json, _jsonOptions);
        if (raw == null) return null;
        return new MedicineDto(raw.MedicineId, raw.Name, raw.Unit, raw.Price, raw.StockQuantity);
    }

    public async Task CreateAsync(CreateMedicineRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("medicines", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task UpdateAsync(int id, UpdateMedicineRequest request)
    {
        var client = GetClient();
        var response = await client.PutAsJsonAsync($"medicines/{id}", request);
        await ThrowIfErrorAsync(response);
    }

    public async Task DeleteAsync(int id)
    {
        var client = GetClient();
        var response = await client.DeleteAsync($"medicines/{id}");
        response.EnsureSuccessStatusCode();
    }

    private static async System.Threading.Tasks.Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("Message", out var msg))
                throw new System.ArgumentException(msg.GetString());
        }
        catch (System.ArgumentException) { throw; }
        catch { }
        response.EnsureSuccessStatusCode();
    }
}
