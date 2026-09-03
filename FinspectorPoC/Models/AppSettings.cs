namespace FinspectorPoC.Models;

public class AppSettings
{
    public string BaseApiUrl { get; set; } = "https://sandbox-finspector.goodveri.online";
    public string TokenUrl { get; set; } = "https://identity.goodveri.online/connect/token";
    public string StatementPath { get; set; } = "/api/Statement";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string OAuthScope { get; set; } = "sc:demo:finspector";
    public string CountryCode { get; set; } = "CZE";
    public string ReferenceNumber { get; set; } = "";

    // Persisted token
    public string? SavedToken { get; set; }
    public DateTimeOffset? TokenExpiry { get; set; }

    // ── PSD2 / Open Banking settings ─────────────────────────────────────────
    public string ClientCode { get; set; } = "";
    public string Psd2CallbackUrl { get; set; } = "http://localhost:5042/psd2/callback";
    public string Psd2ProvidersPath { get; set; } = "/api/ProviderList";
    public string Psd2AccountAuthPath { get; set; } = "/api/AccountAuth";
    public string Psd2AccountsPath { get; set; } = "/api/AccountList";
    public string Psd2AccountInfoPath { get; set; } = "/api/AccountInfo";
    public string Psd2RevokeAuthPath { get; set; } = "/api/RevokeAuth";
    public string Psd2ReferenceNumber { get; set; } = "PSD2-POC";
    public string Psd2UserIp { get; set; } = "127.0.0.1";
    public string Psd2UserBrowserAgent { get; set; } = "FinspectorPoC/1.0";
    public int Psd2Scope { get; set; } = 1;
}

