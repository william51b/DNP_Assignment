using System.Net.Http.Json;
using System.Text.Json;
using ApiContracts;

namespace BlazorApp.Services;

public class HttpUserService : IUserService
{
    private readonly HttpClient client;

    public HttpUserService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<UserDto> AddUserAsync(CreateUserDto request)
    {
        HttpResponseMessage httpResponse = await client.PostAsJsonAsync("users", request);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<UserDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public async Task<List<UserDto>> GetUsersAsync(string? userNameContains = null)
    {
        string url = string.IsNullOrWhiteSpace(userNameContains)
            ? "users"
            : $"users?userNameContains={Uri.EscapeDataString(userNameContains)}";

        HttpResponseMessage httpResponse = await client.GetAsync(url);
        string response = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(response);
        }

        return JsonSerializer.Deserialize<List<UserDto>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}