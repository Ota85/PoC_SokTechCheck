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

   Edit `appsettings.local.json` with your actual Client ID, Client Secret, and (if needed) custom API/Token URLs.

   > The local settings file is read from and written to the application's **output directory** (e.g. `bin/Debug/net10.0/appsettings.local.json`). The UI shows the full path.

3. **(Optional)** You can also edit all settings live in the browser UI and click **Save Settings**.

---

## Run

```bash
cd FinspectorPoC
dotnet run
```

Open your browser to `http://localhost:5042` (or the URL shown in the console).

### Run against the ECUPK local SokordiaTech mock

The ECUPK repository provides a deterministic mock that exercises the same PDF contract as the
production integration. Start it in a separate terminal:

```powershell
cd C:\Projects\ECUPK_SocordiaTech\mocks\sokordiatech
node src/server.js
```

When it reports that it is listening on port `5108`, open the PoC and select **Use ECUPK local
PDF mock** in Connection Settings. The preset uses only synthetic credentials, clears any saved
provider token, and configures the PDF route as `POST /api/Statement`. PSD2 uses different
provider request contracts and is intentionally not changed by this PDF-only preset.

---

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
