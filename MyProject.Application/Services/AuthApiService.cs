using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MyProject.Application.DTOs;

namespace MyProject.Application.Services;

public class AuthApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _clientName = "WebApiClient";

    public AuthApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(_clientName);

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var client = GetClient();
        var response = await client.PostAsJsonAsync("account/login", request);
        
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (errorResult != null)
                {
                    return errorResult;
                }
            }
            catch
            {
                // Fallback if parsing fails
            }
            return new LoginResponse(false, "Invalid username or password.", null, null, null, null);
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>() 
            ?? new LoginResponse(false, "Failed to parse login response.", null, null, null, null);
    }

    public async Task LogoutAsync()
    {
        var client = GetClient();
        await client.PostAsync("account/logout", null);
    }
}
