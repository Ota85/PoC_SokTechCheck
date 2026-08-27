using System.Net.Http.Json;
using FinspectorPoC.Models;

namespace FinspectorPoC.Services;

/// <summary>
/// Handles OAuth client-credentials token acquisition and caching.
/// </summary>
public class TokenService(IHttpClientFactory httpClientFactory, LocalSettingsService settingsService)
{
    /// <summary>Returns a valid bearer token, requesting a new one if the saved token is missing or expired.</summary>
    public async Task<string> GetValidTokenAsync(AppSettings settings, bool forceNew, CancellationToken ct = default)
    {
        if (!forceNew && !string.IsNullOrWhiteSpace(settings.SavedToken) && settings.TokenExpiry > DateTimeOffset.UtcNow)
            return settings.SavedToken!;

        return await RequestNewTokenAsync(settings, ct);
    }

    /// <summary>Requests a new token from the token endpoint and persists it.</summary>
    public async Task<string> RequestNewTokenAsync(AppSettings settings, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(settings.TokenUrl))
            throw new InvalidOperationException("Token URL is not configured.");
        if (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.ClientSecret))
            throw new InvalidOperationException("Client ID and Client Secret must be configured.");

        var client = httpClientFactory.CreateClient("token");

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["scope"] = settings.OAuthScope
        };

        using var response = await client.PostAsync(settings.TokenUrl, new FormUrlEncodedContent(form), ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Token request failed ({(int)response.StatusCode}): {body}");

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new InvalidOperationException("Empty token response.");

        if (!string.IsNullOrWhiteSpace(token.Error))
            throw new InvalidOperationException($"Token error: {token.Error} – {token.ErrorDescription}");

        settings.SavedToken = token.AccessToken;
        settings.TokenExpiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 30); // 30s safety margin
        settingsService.Save(settings);

        return token.AccessToken;
    }
}
