using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinspectorPoC.Models;

namespace FinspectorPoC.Services;

/// <summary>
/// Makes PSD2 / Open Banking calls against the SokordiaTech/Finspector API.
/// Every method returns the raw HTTP status and raw JSON body so the page can
/// display them for developer diagnostics. Typed DTOs are a convenience overlay.
/// </summary>
public class Psd2Service(IHttpClientFactory httpClientFactory, LocalSettingsService settingsService)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    // ── Banks ─────────────────────────────────────────────────────────────────

    public async Task<(int httpStatus, string rawJson, Psd2BanksResponse? typed)> GetBanksAsync(
        AppSettings settings, string bearerToken, CancellationToken ct = default)
    {
        var url = BuildUrl(settings.BaseApiUrl, settings.Psd2BanksPath);
        var client = AuthorizedClient(bearerToken);
        using var resp = await client.GetAsync(url, ct);
        return await ReadAsync<Psd2BanksResponse>(resp, ct);
    }

    // ── Auth Init ─────────────────────────────────────────────────────────────

    public async Task<(int httpStatus, string rawJson, Psd2AuthInitResponse? typed)> InitAuthAsync(
        AppSettings settings, string bearerToken, string bankId, CancellationToken ct = default)
    {
        var url = BuildUrl(settings.BaseApiUrl, settings.Psd2AuthInitPath);
        var request = new Psd2AuthInitRequest
        {
            BankId = bankId,
            ClientCode = settings.ClientCode,
            RedirectUrl = settings.Psd2CallbackUrl,
            CountryCode = settings.CountryCode
        };
        var client = AuthorizedClient(bearerToken);
        using var resp = await client.PostAsJsonAsync(url, request, ct);
        var (httpStatus, rawJson, typed) = await ReadAsync<Psd2AuthInitResponse>(resp, ct);

        // Persist consentId and state when available
        if (typed?.ConsentId is { } cid || typed?.State is { } st)
        {
            if (typed?.ConsentId is { } consentId) settings.Psd2ConsentId = consentId;
            if (typed?.State is { } state) settings.Psd2State = state;
            settingsService.Save(settings);
        }

        return (httpStatus, rawJson, typed);
    }

    // ── Accounts ─────────────────────────────────────────────────────────────

    public async Task<(int httpStatus, string rawJson, Psd2AccountsResponse? typed)> GetAccountsAsync(
        AppSettings settings, string bearerToken, CancellationToken ct = default)
    {
        var url = BuildUrl(settings.BaseApiUrl, settings.Psd2AccountsPath);
        var request = new Psd2AccountsRequest
        {
            ConsentId = settings.Psd2ConsentId ?? "",
            AccessToken = settings.Psd2AccessToken
        };
        var client = AuthorizedClient(bearerToken);
        using var resp = await client.PostAsJsonAsync(url, request, ct);
        return await ReadAsync<Psd2AccountsResponse>(resp, ct);
    }

    // ── Account Info ─────────────────────────────────────────────────────────

    public async Task<(int httpStatus, string rawJson, Psd2AccountInfoResponse? typed)> GetAccountInfoAsync(
        AppSettings settings, string bearerToken, string accountId, CancellationToken ct = default)
    {
        var path = settings.Psd2AccountInfoPath.Replace("{accountId}", Uri.EscapeDataString(accountId));
        var url = BuildUrl(settings.BaseApiUrl, path);
        var client = AuthorizedClient(bearerToken, settings.Psd2AccessToken);

        // Pass consentId as query param only (not secret)
        var uriBuilder = new UriBuilder(url);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        if (!string.IsNullOrWhiteSpace(settings.Psd2ConsentId))
            query["consentId"] = settings.Psd2ConsentId;
        uriBuilder.Query = query.ToString();

        using var resp = await client.GetAsync(uriBuilder.Uri, ct);
        return await ReadAsync<Psd2AccountInfoResponse>(resp, ct);
    }

    // ── Transactions ─────────────────────────────────────────────────────────

    public async Task<(int httpStatus, string rawJson, Psd2TransactionsResponse? typed)> GetTransactionsAsync(
        AppSettings settings, string bearerToken, string accountId,
        string? dateFrom, string? dateTo, CancellationToken ct = default)
    {
        var path = settings.Psd2TransactionsPath.Replace("{accountId}", Uri.EscapeDataString(accountId));
        var url = BuildUrl(settings.BaseApiUrl, path);

        var uriBuilder = new UriBuilder(url);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        if (!string.IsNullOrWhiteSpace(settings.Psd2ConsentId))
            query["consentId"] = settings.Psd2ConsentId;
        if (!string.IsNullOrWhiteSpace(dateFrom))
            query["dateFrom"] = dateFrom;
        if (!string.IsNullOrWhiteSpace(dateTo))
            query["dateTo"] = dateTo;
        uriBuilder.Query = query.ToString();

        var client = AuthorizedClient(bearerToken, settings.Psd2AccessToken);
        using var resp = await client.GetAsync(uriBuilder.Uri, ct);
        return await ReadAsync<Psd2TransactionsResponse>(resp, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private HttpClient AuthorizedClient(string bearerToken, string? psd2AccessToken = null)
    {
        var client = httpClientFactory.CreateClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        if (!string.IsNullOrWhiteSpace(psd2AccessToken))
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Psd2-Access-Token", psd2AccessToken);
        return client;
    }

    private static string BuildUrl(string baseUrl, string path) =>
        baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

    private async Task<(int, string, T?)> ReadAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        var raw = await resp.Content.ReadAsStringAsync(ct);
        var status = (int)resp.StatusCode;
        T? typed = default;
        // Pretty-print the raw JSON for display; fall back to original on failure
        try
        {
            using var doc = JsonDocument.Parse(raw);
            raw = JsonSerializer.Serialize(doc, _json);
        }
        catch (JsonException) { /* keep raw as-is */ }
        try { typed = JsonSerializer.Deserialize<T>(raw); }
        catch (JsonException) { /* caller will show raw */ }
        return (status, raw, typed);
    }
}
