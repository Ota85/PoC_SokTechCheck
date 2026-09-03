using System.Text.Json.Serialization;

namespace FinspectorPoC.Models;

// DTOs follow the public Sokordia/Finspector Swagger document.
public sealed class ProviderListRequest
{
    [JsonPropertyName("requestId")] public string RequestId { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("countryCode")] public string CountryCode { get; set; } = "CZE";
}

public sealed class ProviderListResponse
{
    [JsonPropertyName("providers")] public List<Psd2Provider>? Providers { get; set; }
}

public sealed class Psd2Provider
{
    [JsonPropertyName("countryCode")] public string? CountryCode { get; set; }
    [JsonPropertyName("providerCode")] public string? ProviderCode { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("logo")] public string? Logo { get; set; }
}

public sealed class AccountAuthRequest
{
    [JsonPropertyName("clientCode")] public string ClientCode { get; set; } = "";
    [JsonPropertyName("providerCode")] public string ProviderCode { get; set; } = "";
    [JsonPropertyName("returnUrl")] public string ReturnUrl { get; set; } = "";
    [JsonPropertyName("requestId")] public string RequestId { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("refNo")] public string RefNo { get; set; } = "";
    [JsonPropertyName("countryCode")] public string CountryCode { get; set; } = "CZE";
    [JsonPropertyName("userIp")] public string UserIp { get; set; } = "";
    [JsonPropertyName("userBrowserAgent")] public string UserBrowserAgent { get; set; } = "";
    [JsonPropertyName("scope")] public int Scope { get; set; } = 1;
}

public sealed class AccountAuthResponse
{
    [JsonPropertyName("authenticated")] public bool Authenticated { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("authenticationUrl")] public string? AuthenticationUrl { get; set; }
}

public sealed class AccountListRequest : ProviderRequestBase { }

public sealed class AccountInfoRequest : ProviderRequestBase
{
    [JsonPropertyName("accountId")] public string? AccountId { get; set; }
    [JsonPropertyName("days")] public int Days { get; set; } = 180;
    [JsonPropertyName("calcLogic")] public int CalcLogic { get; set; } = 0;
}

public abstract class ProviderRequestBase
{
    [JsonPropertyName("clientCode")] public string ClientCode { get; set; } = "";
    [JsonPropertyName("providerCode")] public string ProviderCode { get; set; } = "";
    [JsonPropertyName("requestId")] public string RequestId { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("refNo")] public string RefNo { get; set; } = "";
    [JsonPropertyName("countryCode")] public string CountryCode { get; set; } = "CZE";
}

public sealed class AccountListResponse
{
    [JsonPropertyName("resultCode")] public int? ResultCode { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("accounts")] public List<Psd2Account>? Accounts { get; set; }
}

public sealed class Psd2Account
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("accountName")] public string? AccountName { get; set; }
    [JsonPropertyName("productName")] public string? ProductName { get; set; }
    [JsonPropertyName("ownersNames")] public List<string>? OwnersNames { get; set; }
    [JsonPropertyName("pispSuitable")] public bool PispSuitable { get; set; }
    [JsonPropertyName("ibanCode")] public string? IbanCode { get; set; }
    [JsonPropertyName("accountNumber")] public string? AccountNumber { get; set; }
    [JsonPropertyName("creditDebitIndicator")] public string? CreditDebitIndicator { get; set; }
    [JsonPropertyName("balanceType")] public string? BalanceType { get; set; }
    [JsonPropertyName("balance")] public AmountValue? Balance { get; set; }
}

public sealed class AccountInfoResponse
{
    [JsonPropertyName("resultCode")] public int? ResultCode { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("statements")] public List<StatementResult>? Statements { get; set; }
}

public sealed class RevokeAuthRequest
{
    [JsonPropertyName("requestId")] public string RequestId { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("refNo")] public string? RefNo { get; set; }
    [JsonPropertyName("clientCode")] public string ClientCode { get; set; } = "";
}

public sealed class RevokeAuthResponse
{
    [JsonPropertyName("code")] public int? Code { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}
