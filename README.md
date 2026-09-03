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

   Edit `appsettings.local.json` with your sandbox Client ID, Client Secret, GUID client code, and registered
   callback URL. The example defaults use the verified sandbox API and OAuth endpoints. Keep the values supplied
   with your sandbox account if they differ.

   > The local settings file is read from and written to the application's **output directory** (e.g. `bin/Debug/net10.0/appsettings.local.json`). The UI shows the full path.

3. **(Optional)** You can also edit all settings live in the browser UI and click **Save Settings**.

---

## Run

```bash
cd FinspectorPoC
dotnet run
```

Open your browser to `http://localhost:5042` (or the URL shown in the console).

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
