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

// Statement request
public class StatementRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = "ZA";

    [JsonPropertyName("referenceNumber")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("documents")]
    public List<StatementDocument> Documents { get; set; } = [];
}

public class StatementDocument
{
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = "";

    [JsonPropertyName("fileContent")]
    public string FileContent { get; set; } = ""; // base64

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

// Statement response - provider DTOs
public class StatementResponse
{
    [JsonPropertyName("resultCode")]
    public string? ResultCode { get; set; }

    [JsonPropertyName("resultMessage")]
    public string? ResultMessage { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("statements")]
    public List<StatementResult>? Statements { get; set; }
}

public class StatementResult
{
    [JsonPropertyName("documentId")]
    public string? DocumentId { get; set; }

    [JsonPropertyName("accountHolder")]
    public string? AccountHolder { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }

    [JsonPropertyName("statementFromDate")]
    public string? StatementFromDate { get; set; }

    [JsonPropertyName("statementToDate")]
    public string? StatementToDate { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("openingBalance")]
    public decimal? OpeningBalance { get; set; }

    [JsonPropertyName("closingBalance")]
    public decimal? ClosingBalance { get; set; }

    [JsonPropertyName("totalCredits")]
    public decimal? TotalCredits { get; set; }

    [JsonPropertyName("totalDebits")]
    public decimal? TotalDebits { get; set; }

    [JsonPropertyName("transactions")]
    public List<Transaction>? Transactions { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

public class Transaction
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("balance")]
    public decimal? Balance { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}

// Normalized result for download
public class NormalizedResult
{
    public string? ResultCode { get; set; }
    public string? ResultMessage { get; set; }
    public string? RequestId { get; set; }
    public List<NormalizedStatement> Statements { get; set; } = [];
}

public class NormalizedStatement
{
    public string? DocumentId { get; set; }
    public string? AccountHolder { get; set; }
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? Period { get; set; }
    public string? Currency { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? TotalCredits { get; set; }
    public decimal? TotalDebits { get; set; }
    public List<string>? Tags { get; set; }
    public List<Transaction>? Transactions { get; set; }
}
