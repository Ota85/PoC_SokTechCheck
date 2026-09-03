# PoC_SokTechCheck

## Finspector PoC — Blazor Server Bank Statement Parser

A small, self-contained Blazor Server application that lets a **local developer** upload PDF bank statements, authenticate with Finspector (SokordiaTech), submit the statements for parsing, and view or download the results.

> ⚠️ **WARNING — LOCAL POC ONLY.**  
> This application displays credentials and bearer tokens in plain text and stores them in a local configuration file.  
> **Do not deploy this application publicly.**  
> **Do not commit `appsettings.local.json` to source control.**  
> It is already listed in `.gitignore`.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Finspector API credentials (Client ID + Client Secret)

---

## Setup

1. **Clone the repo** and navigate into the project:

   ```bash
   cd FinspectorPoC
   ```

2. **Copy the example settings** and fill in your credentials:

   ```bash
   cp appsettings.example.json appsettings.local.json
   ```

   Edit `appsettings.local.json` with your sandbox Client ID and Client Secret. The example defaults use the
   verified sandbox API and OAuth endpoints. Keep the values supplied with your sandbox account if they differ.

   > The local settings file is read from and written to the application's **output directory** (e.g. `bin/Debug/net10.0/appsettings.local.json`). The UI shows the full path.

3. **(Optional)** You can also edit all settings live in the browser UI and click **Save Settings**.

---

## Run

```bash
cd FinspectorPoC
dotnet run
```

Open your browser to `http://localhost:5000` (or the URL shown in the console).

### PSD2 callback test

The default PSD2 return URL is `https://sign.revolving.dev.linksoft.cz/psd2/callback`.
The existing Revolving test relay redirects it to `http://localhost:5000/psd2/callback`, preserving query
parameters such as `clientId`. This is intended only for a local test where the PoC and bank authorization run
in the same browser on the same computer. Do not run the DigiSign relay or DigiSign PoC on port 5000 at the same time.

For the first PSD2 authorization, leave **Client Code** blank so the PoC creates it and use a reference such as
`PSD2-POC-001`. The PoC detects the browser User-Agent. Use **Detect** next to Public IPv4 to query
`api.ipify.org` for the public IPv4 of the computer running this local PoC.

## Local Settings File

| Location | `<AppOutputDir>/appsettings.local.json` |
|---|---|
| Contents | Base API URL, Token URL, Statement Path, Client ID, Client Secret, OAuth Scope, Country Code, Reference Number, and the saved bearer token + expiry |
| Security | Gitignored. Contains credentials. **Never commit.** |

---

## Features

| Section | What it does |
|---|---|
| **Connection Settings** | Edit and persist all API connection settings including credentials |
| *****  Panel** | Acquire a new OAuth client-credentials token, reuse an unexpired saved token, or clear it |
| **PDF Upload & Parse** | Upload one or more PDFs (with optional per-file password), submit to Finspector |
| **Results** | View result code, account summary, transactions with tags; download raw or normalized JSON |

---

## Security Reminder

- `appsettings.local.json` contains your Client Secret and ****** — treat it like a password.
- Never share or commit this file.
- This app has no authentication, no HTTPS enforcement, and no access controls — it is a local tool only.
