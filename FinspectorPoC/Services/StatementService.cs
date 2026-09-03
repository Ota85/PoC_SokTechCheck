using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinspectorPoC.Models;

namespace FinspectorPoC.Services;

/// <summary>
/// Submits bank-statement PDFs to the Finspector API.
/// </summary>
public class StatementService(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Posts the statement request and returns the HTTP status, raw response body, and
    /// a best-effort typed response (null when the body cannot be deserialized).
    /// The raw body is always returned so the caller can display/download it even on failure.
    /// </summary>
    public async Task<(int httpStatus, string rawJson, StatementResponse? response)> SubmitAsync(
        AppSettings settings,
        string bearerToken,
        StatementRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(settings.BaseApiUrl))
            throw new InvalidOperationException("Base API URL is not configured.");
        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new InvalidOperationException("****** is missing. Authenticate first.");

        var url = settings.BaseApiUrl.TrimEnd('/') + "/" + settings.StatementPath.TrimStart('/');

        var client = httpClientFactory.CreateClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var httpResponse = await client.PostAsJsonAsync(url, request, ct);
        var rawJson = await httpResponse.Content.ReadAsStringAsync(ct);
        var httpStatus = (int)httpResponse.StatusCode;

        StatementResponse? typed = null;
        try
        {
            typed = JsonSerializer.Deserialize<StatementResponse>(rawJson);
        }
        catch (JsonException)
        {
            // Deserialization failure — caller displays the raw body as-is
        }

        return (httpStatus, rawJson, typed);
    }

    public static NormalizedResult Normalize(StatementResponse response) => new()
    {
        ResultCode = response.ResultCode,
        Message = response.Message,
        RequestId = response.RequestId,
        Statements = response.Statements?.Select(s => new NormalizedStatement
        {
            BankCode = s.BankCode,
            AccountName = s.AccountName,
            ProductName = s.ProductName,
            AccountHolder = s.Name,
            AccountNumber = s.AccountNumber,
            Period = $"{s.StartDate:yyyy-MM-dd} – {s.EndDate:yyyy-MM-dd}",
            Currency = s.StartBalance?.Currency ?? s.EndBalance?.Currency,
            IsTrusted = s.IsTrusted,
            StartBalance = s.StartBalance?.Value,
            EndBalance = s.EndBalance?.Value,
            Tags = s.Tags,
            Transactions = s.Transactions
        }).ToList() ?? []
    };
}
