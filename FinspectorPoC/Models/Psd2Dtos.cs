using System.Text.Json.Serialization;

namespace FinspectorPoC.Models;

// ── Banks ────────────────────────────────────────────────────────────────────

public class Psd2BanksResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("banks")]
    public List<Psd2Bank>? Banks { get; set; }
}

public class Psd2Bank
{
    [JsonPropertyName("bankId")]
    public string? BankId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("bic")]
    public string? Bic { get; set; }

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("supported")]
    public bool? Supported { get; set; }

    public override string ToString() => $"{Name} ({BankId})";
}

// ── Auth Init ────────────────────────────────────────────────────────────────

public class Psd2AuthInitRequest
{
    [JsonPropertyName("bankId")]
    public string BankId { get; set; } = "";

    [JsonPropertyName("clientCode")]
    public string ClientCode { get; set; } = "";

    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; set; } = "";

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = "CZE";
}

public class Psd2AuthInitResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("authorizationUrl")]
    public string? AuthorizationUrl { get; set; }

    [JsonPropertyName("consentId")]
    public string? ConsentId { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

// ── Accounts ─────────────────────────────────────────────────────────────────

public class Psd2AccountsRequest
{
    [JsonPropertyName("consentId")]
    public string ConsentId { get; set; } = "";

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }
}

public class Psd2AccountsResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("accounts")]
    public List<Psd2Account>? Accounts { get; set; }
}

public class Psd2Account
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    public override string ToString() => $"{Iban ?? AccountId} {Currency}".Trim();
}

// ── Account Info ─────────────────────────────────────────────────────────────

public class Psd2AccountInfoResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("account")]
    public Psd2AccountDetail? Account { get; set; }
}

public class Psd2AccountDetail
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("balances")]
    public List<Psd2Balance>? Balances { get; set; }
}

public class Psd2Balance
{
    [JsonPropertyName("balanceType")]
    public string? BalanceType { get; set; }

    [JsonPropertyName("amount")]
    public AmountValue? Amount { get; set; }

    [JsonPropertyName("referenceDate")]
    public string? ReferenceDate { get; set; }
}

// ── Transactions ─────────────────────────────────────────────────────────────

public class Psd2TransactionsResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("transactions")]
    public List<Psd2Transaction>? Transactions { get; set; }
}

public class Psd2Transaction
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("bookingDate")]
    public string? BookingDate { get; set; }

    [JsonPropertyName("valueDate")]
    public string? ValueDate { get; set; }

    [JsonPropertyName("amount")]
    public AmountValue? Amount { get; set; }

    [JsonPropertyName("creditorName")]
    public string? CreditorName { get; set; }

    [JsonPropertyName("debtorName")]
    public string? DebtorName { get; set; }

    [JsonPropertyName("remittanceInformation")]
    public string? RemittanceInformation { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
