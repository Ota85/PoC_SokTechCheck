using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinspectorPoC.Models;

namespace FinspectorPoC.Services;

/// <summary>Calls the public Sokordia/Finspector PSD2 endpoints defined in its Swagger document.</summary>
public sealed class Psd2Service(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public Task<(int, string, ProviderListResponse?)> GetProvidersAsync(AppSettings settings, string token, CancellationToken ct = default) =>
        PostAsync<ProviderListRequest, ProviderListResponse>(settings, settings.Psd2ProvidersPath, token,
            new ProviderListRequest { CountryCode = settings.CountryCode }, ct);

    public Task<(int, string, AccountAuthResponse?)> StartAuthorizationAsync(AppSettings settings, string token, string providerCode, CancellationToken ct = default) =>
        PostAsync<AccountAuthRequest, AccountAuthResponse>(settings, settings.Psd2AccountAuthPath, token,
            new AccountAuthRequest
            {
                ClientCode = settings.ClientCode, ProviderCode = providerCode,
                ReturnUrl = settings.Psd2CallbackUrl, RefNo = settings.Psd2ReferenceNumber,
                CountryCode = settings.CountryCode, UserIp = settings.Psd2UserIp,
                UserBrowserAgent = settings.Psd2UserBrowserAgent, Scope = settings.Psd2Scope
            }, ct);

    public Task<(int, string, AccountListResponse?)> GetAccountsAsync(AppSettings settings, string token, string providerCode, CancellationToken ct = default) =>
        PostAsync<AccountListRequest, AccountListResponse>(settings, settings.Psd2AccountsPath, token, ProviderRequest(settings, providerCode), ct);

    public Task<(int, string, AccountInfoResponse?)> GetAccountInfoAsync(AppSettings settings, string token, string providerCode, string? accountId, CancellationToken ct = default) =>
        PostAsync<AccountInfoRequest, AccountInfoResponse>(settings, settings.Psd2AccountInfoPath, token,
            new AccountInfoRequest
            {
                ClientCode = settings.ClientCode, ProviderCode = providerCode, RefNo = settings.Psd2ReferenceNumber,
                CountryCode = settings.CountryCode, AccountId = accountId
            }, ct);

    public Task<(int, string, RevokeAuthResponse?)> RevokeAuthorizationAsync(AppSettings settings, string token, CancellationToken ct = default) =>
        PostAsync<RevokeAuthRequest, RevokeAuthResponse>(settings, settings.Psd2RevokeAuthPath, token,
            new RevokeAuthRequest { ClientCode = settings.ClientCode, RefNo = settings.Psd2ReferenceNumber }, ct);

    private static AccountListRequest ProviderRequest(AppSettings settings, string providerCode) => new()
    {
        ClientCode = settings.ClientCode, ProviderCode = providerCode,
        RefNo = settings.Psd2ReferenceNumber, CountryCode = settings.CountryCode
    };

    private async Task<(int, string, TResponse?)> PostAsync<TRequest, TResponse>(AppSettings settings, string path, string token, TRequest request, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.PostAsJsonAsync(BuildUrl(settings.BaseApiUrl, path), request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        TResponse? typed = default;
        try { typed = JsonSerializer.Deserialize<TResponse>(raw); } catch (JsonException) { }
        try { raw = JsonSerializer.Serialize(JsonDocument.Parse(raw), Json); } catch (JsonException) { }
        return ((int)response.StatusCode, raw, typed);
    }

    private static string BuildUrl(string baseUrl, string path) => baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
}
