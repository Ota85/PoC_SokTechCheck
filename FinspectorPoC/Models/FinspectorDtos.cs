using System.Text.Json.Serialization;

namespace FinspectorPoC.Models;

// OAuth token response
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}

// Statement request — matches Finspector OpenAPI
public class StatementRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = "CZE";

    [JsonPropertyName("calcLogic")]
    public string CalcLogic { get; set; } = "FromEndDate";

    [JsonPropertyName("refNo")]
    public string? RefNo { get; set; }

    [JsonPropertyName("files")]
    public List<StatementFile> Files { get; set; } = [];
}

public class StatementFile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>MIME type, e.g. "application/pdf"</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "application/pdf";

    /// <summary>Base64-encoded file content.</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("password")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Password { get; set; }
}

// Statement response — provider DTOs matching Finspector OpenAPI
public class StatementResponse
{
    /// <summary>Numeric result code (0 = success).</summary>
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("statements")]
    public List<StatementResult>? Statements { get; set; }
}

public class StatementResult
{
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("bankCode")]
    public string? BankCode { get; set; }

    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("isTrusted")]
    public bool IsTrusted { get; set; }

    [JsonPropertyName("startDate")]
    public DateTimeOffset? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTimeOffset? EndDate { get; set; }

    [JsonPropertyName("startBalance")]
    public AmountValue? StartBalance { get; set; }

    [JsonPropertyName("endBalance")]
    public AmountValue? EndBalance { get; set; }

    [JsonPropertyName("transactions")]
    public List<Transaction>? Transactions { get; set; }

    [JsonPropertyName("tags")]
    public List<StatementTag>? Tags { get; set; }
}

public class StatementTag
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("flag")]
    public bool Flag { get; set; }

    [JsonPropertyName("amount")]
    public AmountValue? Amount { get; set; }

    [JsonPropertyName("necessityAmount")]
    public AmountValue? NecessityAmount { get; set; }
}

/// <summary>Amount object used for balances and totals (value + currency).</summary>
public class AmountValue
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    public override string ToString() => $"{Value:N2} {Currency}".Trim();
}

public class Transaction
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public AmountValue? Amount { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("vs")]
    public string? VariableSymbol { get; set; }

    [JsonPropertyName("ks")]
    public string? ConstantSymbol { get; set; }

    [JsonPropertyName("ss")]
    public string? SpecificSymbol { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

// Normalized result for download
public class NormalizedResult
{
    public int? ResultCode { get; set; }
    public string? Message { get; set; }
    public string? RequestId { get; set; }
    public List<NormalizedStatement> Statements { get; set; } = [];
}

public class NormalizedStatement
{
    public string? BankCode { get; set; }
    public string? AccountName { get; set; }
    public string? ProductName { get; set; }
    public string? AccountHolder { get; set; }
    public string? AccountNumber { get; set; }
    public string? Period { get; set; }
    public string? Currency { get; set; }
    public bool IsTrusted { get; set; }
    public decimal? StartBalance { get; set; }
    public decimal? EndBalance { get; set; }
    public List<StatementTag>? Tags { get; set; }
    public List<Transaction>? Transactions { get; set; }
}

