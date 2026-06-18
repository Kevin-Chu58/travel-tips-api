# Travel Tips API

The backend REST API for the [Travel Tips](https://github.com/Kevin-Chu58/travel-tips-ui) travel planning application. Built with ASP.NET Core (C#) and deployed to Azure App Service.

---

## Overview

Travel Tips API provides the server-side data layer for the Travel Tips UI. It handles authentication via Auth0 JWT tokens, exposes a RESTful interface for managing trips and travel content, and is deployed as an Azure Web App with CI/CD via GitHub Actions.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# (.NET) |
| Framework | ASP.NET Core Web API |
| Auth | Auth0 (JWT Bearer tokens) |
| Hosting | Azure App Service (US West) |
| CI/CD | GitHub Actions |
| IDE | Visual Studio 2022 |

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (check `TravelTipsAPI.csproj` for the target framework version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code with the C# extension
- An [Auth0](https://auth0.com) tenant configured with an API audience of `https://travel-tips`
- A database instance (configure the connection string in `appsettings.json`)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Kevin-Chu58/travel-tips-api.git
cd travel-tips-api
```

### 2. Configure app settings

Copy `appsettings.json` and create a local override:

```bash
cp TravelTipsAPI/appsettings.json TravelTipsAPI/appsettings.Development.json
```

Update `appsettings.Development.json` with your local configuration:

```json
{
  "Auth0": {
    "Domain": "your-auth0-domain.us.auth0.com",
    "Audience": "https://travel-tips"
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-database-connection-string"
  }
}
```

> **Note:** Never commit secrets or real connection strings. Use `appsettings.Development.json` (git-ignored) or [.NET Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.

### 3. Restore dependencies and run

**With the .NET CLI:**

```bash
cd TravelTipsAPI
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5252` by default (matching the UI's `VITE_API_URL_LOCAL`).

**With Visual Studio:**

Open `TravelTipsAPI.sln` and press `F5` to run in debug mode.

---

## Project Structure

```
travel-tips-api/
├── .github/workflows/       # CI/CD pipelines (GitHub Actions → Azure)
├── TravelTipsAPI/           # Main ASP.NET Core project
│   ├── Controllers/         # API route handlers
│   ├── Models/              # Data models and DTOs
│   ├── Program.cs           # App entry point and service configuration
│   └── appsettings.json     # Base configuration (no secrets)
└── TravelTipsAPI.sln        # Visual Studio solution file
```

---

## API Endpoints

The API is consumed by the Travel Tips UI at `/api`. Base URLs:

| Environment | URL |
|-------------|-----|
| Local | `http://localhost:5252/api` |
| Production (US West) | `https://travel-tips-api-us-west-aweybudkfpchejam.westus-01.azurewebsites.net/api` |

All endpoints require a valid Auth0 JWT Bearer token in the `Authorization` header.

---

## Authentication

This API uses Auth0 for authentication. Requests must include a Bearer token issued by the configured Auth0 tenant:

```
Authorization: Bearer <your-auth0-access-token>
```

The token audience must be `https://travel-tips`. The UI handles token acquisition automatically via `@auth0/auth0-react`.

---

## Deployment

The API is deployed to Azure App Service via GitHub Actions. Workflows are defined in `.github/workflows/`.

To deploy manually using the Azure CLI:

```bash
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group <rg> --name <app-name> --src-path ./publish
```

---

## Related

- **Frontend:** [travel-tips-ui](https://github.com/Kevin-Chu58/travel-tips-ui) — React + TypeScript + Vite

---

## License

This project is licensed under [CC BY-NC](./LICENSE-CC-BY-NC) — free to use for non-commercial purposes with attribution.
