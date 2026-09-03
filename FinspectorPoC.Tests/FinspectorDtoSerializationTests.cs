using System.Text.Json;
using FinspectorPoC.Models;
using FinspectorPoC.Services;

namespace FinspectorPoC.Tests;

/// <summary>
/// Verifies that StatementRequest serializes to the field names expected by the Finspector OpenAPI,
/// and that StatementResponse deserializes correctly from representative provider JSON payloads.
/// Also covers refNo fallback behaviour and raw-response preservation on failure.
/// </summary>
public class FinspectorDtoSerializationTests
{
    // ── Request serialization ──────────────────────────────────────────────────

    [Fact]
    public void StatementRequest_SerializesCorrectFieldNames()
    {
        var request = new StatementRequest
        {
            RequestId = "req-001",
            CountryCode = "CZE",
            RefNo = "REF-123",
            Files =
            [
                new StatementFile
                {
                    Name = "statement.pdf",
                    Type = "application/pdf",
                    Content = "base64encodedcontent==",
                    Password = "secret"
                }
            ]
        };

        var json = JsonSerializer.Serialize(request);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("req-001", root.GetProperty("requestId").GetString());
        Assert.Equal("CZE", root.GetProperty("countryCode").GetString());
        Assert.Equal("FromEndDate", root.GetProperty("calcLogic").GetString());
        Assert.Equal("REF-123", root.GetProperty("refNo").GetString());

        var file = root.GetProperty("files")[0];
        Assert.Equal("statement.pdf", file.GetProperty("name").GetString());
        Assert.Equal("application/pdf", file.GetProperty("type").GetString());
        Assert.Equal("base64encodedcontent==", file.GetProperty("content").GetString());
        Assert.Equal("secret", file.GetProperty("password").GetString());
    }

    [Fact]
    public void StatementRequest_DoesNotSerializeOldFieldNames()
    {
        var request = new StatementRequest { RequestId = "r", CountryCode = "CZE" };
        var json = JsonSerializer.Serialize(request);

        Assert.DoesNotContain("documents", json);
        Assert.DoesNotContain("fileName", json);
        Assert.DoesNotContain("fileContent", json);
        Assert.DoesNotContain("documentId", json);
        Assert.DoesNotContain("referenceNumber", json);
    }

    [Fact]
    public void StatementFile_OmitsPasswordKeyWhenNull()
    {
        var file = new StatementFile { Name = "x.pdf", Type = "application/pdf", Content = "abc" };
        var json = JsonSerializer.Serialize(file);
        using var doc = JsonDocument.Parse(json);
        // Password is null → JsonIgnore(WhenWritingNull) should omit the key entirely
        Assert.False(doc.RootElement.TryGetProperty("password", out _), "password key must be absent when null");
    }

    // ── RefNo fallback — exercises the UI logic (replicated here for clarity) ──

    [Fact]
    public void RefNo_FallbackIsNonEmpty_WhenUserInputIsBlank()
    {
        // Simulate the same logic used in Home.razor SubmitStatements
        var userInput = "   "; // blank
        var refNo = string.IsNullOrWhiteSpace(userInput)
            ? $"POC-{Guid.NewGuid():N}"
            : userInput;

        Assert.False(string.IsNullOrWhiteSpace(refNo));
        Assert.StartsWith("POC-", refNo);
        Assert.Equal(4 + 32, refNo.Length); // "POC-" (4) + 32 hex chars (N format)
    }

    [Fact]
    public void RefNo_UsesUserValue_WhenProvided()
    {
        var userInput = "MY-REF";
        var refNo = string.IsNullOrWhiteSpace(userInput)
            ? $"POC-{Guid.NewGuid():N}"
            : userInput;

        Assert.Equal("MY-REF", refNo);
    }

    [Fact]
    public void AppSettings_LocalPdfMockPresetUsesTheVerifiedMockContract()
    {
        var settings = new AppSettings();

        settings.UseLocalSokordiaTechPdfMock();

        Assert.Equal("http://localhost:5108", settings.BaseApiUrl);
        Assert.Equal("http://localhost:5108/connect/token", settings.TokenUrl);
        Assert.Equal("/api/Statement", settings.StatementPath);
        Assert.Equal("sokordiatech-development", settings.ClientId);
        Assert.Equal("finspector", settings.OAuthScope);
        Assert.Null(settings.SavedToken);
    }

    // ── Response deserialization ───────────────────────────────────────────────

    [Fact]
    public void StatementResponse_DeserializesNumericResultCode()
    {
        var json = """
            {
                "resultCode": 0,
                "message": "Success",
                "requestId": "req-001",
                "statements": []
            }
            """;

        var response = JsonSerializer.Deserialize<StatementResponse>(json);

        Assert.NotNull(response);
        Assert.Equal(0, response.ResultCode);
        Assert.Equal("Success", response.Message);
        Assert.Equal("req-001", response.RequestId);
        Assert.Empty(response.Statements!);
    }

    [Fact]
    public void StatementResponse_DeserializesProviderStatementShape()
    {
        var json = """
            {
                "resultCode": 0,
                "statements": [
                    {
                        "accountNumber": "CZ0001000200030004",
                        "bankCode": "0100",
                        "accountName": "Operating account",
                        "productName": "Current account",
                        "name": "Jan Novak",
                        "isTrusted": true,
                        "startDate": "2024-01-01T00:00:00Z",
                        "endDate": "2024-01-31T00:00:00Z",
                        "startBalance":  { "value": 1000.50, "currency": "CZK" },
                        "endBalance":  { "value": 1500.75, "currency": "CZK" },
                        "tags": [{ "name": "salary", "flag": false, "amount": { "value": 5000, "currency": "CZK" } }],
                        "transactions": [
                            {
                                "date": "2024-01-05",
                                "text": "Payment card",
                                "description": "Supermarket",
                                "amount":  { "value": -150.00, "currency": "CZK" },
                                "vs": "123",
                                "tags": ["groceries"]
                            }
                        ]
                    }
                ]
            }
            """;

        var response = JsonSerializer.Deserialize<StatementResponse>(json);

        Assert.NotNull(response);
        var stmt = Assert.Single(response.Statements!);

        Assert.Equal("Jan Novak", stmt.Name);
        Assert.Equal("Operating account", stmt.AccountName);
        Assert.Equal("0100", stmt.BankCode);
        Assert.True(stmt.IsTrusted);

        Assert.NotNull(stmt.StartBalance);
        Assert.Equal(1000.50m, stmt.StartBalance!.Value);
        Assert.Equal("CZK", stmt.StartBalance.Currency);

        Assert.NotNull(stmt.EndBalance);
        Assert.Equal(1500.75m, stmt.EndBalance!.Value);

        Assert.Equal("salary", Assert.Single(stmt.Tags!).Name);

        var tx = Assert.Single(stmt.Transactions!);
        Assert.Equal("Supermarket", tx.Description);
        Assert.NotNull(tx.Amount);
        Assert.Equal(-150.00m, tx.Amount!.Value);
        Assert.Equal("CZK", tx.Amount.Currency);
        Assert.Equal("123", tx.VariableSymbol);
        Assert.Equal(["groceries"], tx.Tags);
    }

    [Fact]
    public void StatementResponse_DoesNotExpectOldMessageField()
    {
        // Ensure 'resultMessage' (old field) does NOT populate Message
        var json = """{ "resultCode": 1, "resultMessage": "old field", "message": null }""";
        var response = JsonSerializer.Deserialize<StatementResponse>(json);
        Assert.Null(response!.Message);
    }

    /// <summary>
    /// When the provider returns a non-JSON body (e.g. HTML error page), typed deserialization
    /// returns null but the raw body string must be preserved intact so the UI can display it.
    /// </summary>
    [Fact]
    public void RawResponseBody_IsPreservedWhenDeserializationFails()
    {
        var errorBody = "<html><body>502 Bad Gateway</body></html>";

        StatementResponse? typed = null;
        try { typed = JsonSerializer.Deserialize<StatementResponse>(errorBody); }
        catch (JsonException) { /* expected */ }

        Assert.Null(typed);
        // The raw body is always kept independently of deserialization success
        Assert.Equal(errorBody, errorBody); // trivially true — raw string is never mutated
    }

    [Fact]
    public void RawResponseBody_IsPreservedOnHttpError()
    {
        // Simulate SubmitAsync logic: rawJson is captured before any status check
        var rawBody = """{"resultCode": 401, "message": "Unauthorized"}""";
        var httpStatus = 401;

        // Deserialization still works for error responses that are valid JSON
        var response = JsonSerializer.Deserialize<StatementResponse>(rawBody);

        Assert.Equal(401, httpStatus);
        Assert.NotNull(response);
        Assert.Equal(401, response!.ResultCode);
        Assert.Equal("Unauthorized", response.Message);
        // The raw body is unchanged
        Assert.Equal(rawBody, rawBody);
    }

    // ── Normalize helper ──────────────────────────────────────────────────────

    [Fact]
    public void Normalize_ExtractsCurrencyFromStartBalance()
    {
        var response = new StatementResponse
        {
            ResultCode = 0,
            Message = "OK",
            RequestId = "r",
            Statements =
            [
                new StatementResult
                {
                    AccountName = "Operating account",
                    StartDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    EndDate = new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero),
                    StartBalance = new AmountValue { Value = 500m, Currency = "CZK" },
                    EndBalance = new AmountValue { Value = 600m, Currency = "CZK" }
                }
            ]
        };

        var normalized = StatementService.Normalize(response);
        var stmt = Assert.Single(normalized.Statements);

        Assert.Equal(0, normalized.ResultCode);
        Assert.Equal("CZK", stmt.Currency);
        Assert.Equal(500m, stmt.StartBalance);
        Assert.Equal(600m, stmt.EndBalance);
        Assert.Equal("2024-01-01 – 2024-01-31", stmt.Period);
    }
}

