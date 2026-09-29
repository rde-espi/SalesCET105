using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using ProjetoFinalCet105.Mobile.Models;

namespace ProjetoFinalCet105.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.0.2.2:5219/")
        };
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync( "api/Auth/login", request);

        var enderecoFinal = response.RequestMessage?.RequestUri?.ToString();
        var status = response.StatusCode;
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await AdicionarTokenAsync();

        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>( string endpoint, TRequest dados)
    {
        await AdicionarTokenAsync();

        var response = await _httpClient.PostAsJsonAsync( endpoint, dados);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await AdicionarTokenAsync();

        var response = await _httpClient.DeleteAsync(endpoint);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PutAsync<TRequest>( string endpoint, TRequest dados)
    {
        await AdicionarTokenAsync();

        var response = await _httpClient.PutAsJsonAsync( endpoint, dados);

        return response.IsSuccessStatusCode;
    }



    private async Task AdicionarTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");

        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }


}
