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

    public async Task<(StatementResponse response, string rawJson)> SubmitAsync(
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

        if (!httpResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Statement API failed ({(int)httpResponse.StatusCode}): {rawJson}");

        var result = JsonSerializer.Deserialize<StatementResponse>(rawJson)
            ?? throw new InvalidOperationException("Empty response from statement API.");

        return (result, rawJson);
    }

    public static NormalizedResult Normalize(StatementResponse response) => new()
    {
        ResultCode = response.ResultCode,
        Message = response.Message,
        RequestId = response.RequestId,
        Statements = response.Statements?.Select(s => new NormalizedStatement
        {
            DocumentId = s.DocumentId,
            AccountHolder = s.AccountHolder,
            AccountNumber = s.AccountNumber,
            BankName = s.BankName,
            Period = $"{s.StatementFromDate} – {s.StatementToDate}",
            Currency = s.OpeningBalance?.Currency
                       ?? s.ClosingBalance?.Currency
                       ?? s.TotalCredits?.Currency
                       ?? s.TotalDebits?.Currency,
            OpeningBalance = s.OpeningBalance?.Value,
            ClosingBalance = s.ClosingBalance?.Value,
            TotalCredits = s.TotalCredits?.Value,
            TotalDebits = s.TotalDebits?.Value,
            Tags = s.Tags,
            Transactions = s.Transactions
        }).ToList() ?? []
    };
}
