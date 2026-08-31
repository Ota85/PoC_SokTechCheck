namespace FinspectorPoC.Models;

public class AppSettings
{
    public string BaseApiUrl { get; set; } = "https://finspector.goodveri.online";
    public string TokenUrl { get; set; } = "https://finspector.goodveri.online/connect/token";
    public string StatementPath { get; set; } = "/api/Statement";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string OAuthScope { get; set; } = "finspector";
    public string CountryCode { get; set; } = "CZE";
    public string ReferenceNumber { get; set; } = "";

    // Persisted token
    public string? SavedToken { get; set; }
    public DateTimeOffset? TokenExpiry { get; set; }

    // ── PSD2 / Open Banking settings ─────────────────────────────────────────
    public string ClientCode { get; set; } = "";
    public string Psd2CallbackUrl { get; set; } = "http://localhost:5042/psd2/callback";
    public string Psd2BanksPath { get; set; } = "/api/OpenBanking/banks";
    public string Psd2AuthInitPath { get; set; } = "/api/OpenBanking/auth/init";
    public string Psd2AccountsPath { get; set; } = "/api/OpenBanking/accounts";
    public string Psd2AccountInfoPath { get; set; } = "/api/OpenBanking/accounts/{accountId}";
    public string Psd2TransactionsPath { get; set; } = "/api/OpenBanking/accounts/{accountId}/transactions";

    // Persisted PSD2 tokens / long-lived values returned by the provider
    public string? Psd2ConsentId { get; set; }
    public string? Psd2State { get; set; }
    public string? Psd2AccessToken { get; set; }
    public DateTimeOffset? Psd2AccessTokenExpiry { get; set; }
    public string? Psd2RefreshToken { get; set; }
    public DateTimeOffset? Psd2RefreshTokenExpiry { get; set; }
}

