using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Services;

public class AuthService
{
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "user_id";
    private const string NomeKey = "user_nome";
    private const string EmailKey = "user_email";

    public async Task GuardarSessaoAsync( string token, string userId, string nome, string email)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UserIdKey, userId);
        await SecureStorage.Default.SetAsync(NomeKey, nome);
        await SecureStorage.Default.SetAsync(EmailKey, email);
    }

    public async Task<string?> ObterTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }

    public async Task<string?> ObterUserIdAsync()
    {
        return await SecureStorage.Default.GetAsync(UserIdKey);
    }

    public async Task<string?> ObterNomeAsync()
    {
        return await SecureStorage.Default.GetAsync(NomeKey);
    }

    public async Task<string?> ObterEmailAsync()
    {
        return await SecureStorage.Default.GetAsync(EmailKey);
    }

    public void Logout()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(NomeKey);
        SecureStorage.Default.Remove(EmailKey);
    }

    public async Task<bool> EstaAutenticadoAsync()
    {
        var token = await ObterTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
}