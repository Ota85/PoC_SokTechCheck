namespace FinspectorPoC.Models;

public class AppSettings
{
    public string BaseApiUrl { get; set; } = "https://finspector.goodveri.online";
    public string TokenUrl { get; set; } = "https://auth.goodveri.online/connect/token";
    public string StatementPath { get; set; } = "/api/v1/statement";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string OAuthScope { get; set; } = "finspector";
    public string CountryCode { get; set; } = "ZA";
    public string ReferenceNumber { get; set; } = "";

    // Persisted token
    public string? SavedToken { get; set; }
    public DateTimeOffset? TokenExpiry { get; set; }
}
