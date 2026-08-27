using System.Text.Json;
using FinspectorPoC.Models;
using FinspectorPoC.Services;

namespace FinspectorPoC.Tests;

/// <summary>
/// Verifies that StatementRequest serializes to the field names expected by the Finspector OpenAPI,
/// and that StatementResponse deserializes correctly from a representative provider JSON payload.
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
    public void StatementResponse_DeserializesAmountObjects()
    {
        var json = """
            {
                "resultCode": 0,
                "message": "OK",
                "requestId": "r1",
                "statements": [
                    {
                        "documentId": "doc-1",
                        "accountHolder": "Jan Novak",
                        "accountNumber": "CZ0001000200030004",
                        "bankName": "TestBank",
                        "statementFromDate": "2024-01-01",
                        "statementToDate": "2024-01-31",
                        "openingBalance":  { "value": 1000.50, "currency": "CZK" },
                        "closingBalance":  { "value": 1500.75, "currency": "CZK" },
                        "totalCredits":    { "value":  800.00, "currency": "CZK" },
                        "totalDebits":     { "value":  300.25, "currency": "CZK" },
                        "tags": ["salary", "rent"],
                        "transactions": [
                            {
                                "date": "2024-01-05",
                                "description": "Supermarket",
                                "amount":  { "value": -150.00, "currency": "CZK" },
                                "balance": { "value":  850.50, "currency": "CZK" },
                                "type": "debit",
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

        Assert.Equal("doc-1", stmt.DocumentId);
        Assert.Equal("Jan Novak", stmt.AccountHolder);
        Assert.Equal("TestBank", stmt.BankName);

        Assert.NotNull(stmt.OpeningBalance);
        Assert.Equal(1000.50m, stmt.OpeningBalance!.Value);
        Assert.Equal("CZK", stmt.OpeningBalance.Currency);

        Assert.NotNull(stmt.ClosingBalance);
        Assert.Equal(1500.75m, stmt.ClosingBalance!.Value);

        Assert.NotNull(stmt.TotalCredits);
        Assert.Equal(800.00m, stmt.TotalCredits!.Value);

        Assert.NotNull(stmt.TotalDebits);
        Assert.Equal(300.25m, stmt.TotalDebits!.Value);

        Assert.Equal(["salary", "rent"], stmt.Tags);

        var tx = Assert.Single(stmt.Transactions!);
        Assert.Equal("Supermarket", tx.Description);
        Assert.NotNull(tx.Amount);
        Assert.Equal(-150.00m, tx.Amount!.Value);
        Assert.Equal("CZK", tx.Amount.Currency);
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

    // ── Normalize helper ──────────────────────────────────────────────────────

    [Fact]
    public void Normalize_ExtractsCurrencyFromOpeningBalance()
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
                    DocumentId = "d1",
                    StatementFromDate = "2024-01-01",
                    StatementToDate = "2024-01-31",
                    OpeningBalance = new AmountValue { Value = 500m, Currency = "CZK" },
                    ClosingBalance = new AmountValue { Value = 600m, Currency = "CZK" },
                    TotalCredits = new AmountValue { Value = 200m, Currency = "CZK" },
                    TotalDebits = new AmountValue { Value = 100m, Currency = "CZK" }
                }
            ]
        };

        var normalized = StatementService.Normalize(response);
        var stmt = Assert.Single(normalized.Statements);

        Assert.Equal(0, normalized.ResultCode);
        Assert.Equal("CZK", stmt.Currency);
        Assert.Equal(500m, stmt.OpeningBalance);
        Assert.Equal(600m, stmt.ClosingBalance);
        Assert.Equal(200m, stmt.TotalCredits);
        Assert.Equal(100m, stmt.TotalDebits);
        Assert.Equal("2024-01-01 – 2024-01-31", stmt.Period);
    }
}
