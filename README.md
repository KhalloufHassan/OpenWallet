# OpenWallet

A self-hosted personal finance tracker built with .NET 10 Blazor WebAssembly and PostgreSQL.

## Features

- **Accounts** — track multiple accounts with individual currencies and balances
- **Records** — log income, expenses, and transfers with categories, tags, stores, and geo-location
- **Categories** — hierarchical category tree (parent + sub-categories) with icons and colors
- **Tags** — label records with free-form tags for flexible grouping
- **Stores** — track spending by store/merchant
- **Templates** — save frequently used records as reusable templates
- **Debts** — track money lent or borrowed; link repayment records to debts
- **Attachments** — attach receipts and files to records
- **Dashboard** — account balances, expense charts (by category / tag), balance trend, recent records
- **Filters** — filter records and dashboard by date range, account, category, store, type, and search text
- **Privacy mode** — blur sensitive amounts with a single toggle (persisted across sessions)
- **Mobile-friendly** — responsive layout with collapsible sidebar drawer

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10) |
| Backend | ASP.NET Core 10 |
| Database | PostgreSQL 18 |
| ORM | Entity Framework Core 9 (Npgsql) |
| UI | Bootstrap 5 + Bootstrap Icons |
| Charts | Chart.js |
| Maps | Leaflet.js |

## Running with Docker

```bash
docker compose up -d
```

The app will be available at `http://localhost:8090`.

On first load you will be prompted to complete initial setup — create a username, password, and first account. Default categories are seeded automatically.

Data is persisted in two named Docker volumes:

- `pgdata` — PostgreSQL data
- `uploads` — record attachments

## Running Locally (Development)

**Prerequisites:** .NET 10 SDK, PostgreSQL instance

1. Update the connection string in `OpenWallet/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=openwallet;Username=postgres;Password=yourpassword"
  }
}
```

2. Run the server (migrations are applied automatically on startup):

```bash
dotnet run --project OpenWallet
```

The Blazor WASM client is served by the same host. Open `https://localhost:5001` (or the port shown in the terminal).

## Project Structure

```
OpenWallet.sln
├── OpenWallet/           # ASP.NET Core host — API controllers, EF Core, managers
│   ├── Controllers/
│   ├── Database/
│   │   ├── Models/
│   │   └── Migrations/
│   └── Managers/
├── OpenWallet.Client/    # Blazor WebAssembly app
│   ├── Components/       # Reusable Blazor components
│   ├── Layout/
│   ├── Pages/
│   └── Services/
└── OpenWallet.Shared/    # DTOs and enums shared between server and client
    ├── DTOs/
    └── Models/
```

## Default Categories

Categories are seeded on first setup: Food & Drinks, Shopping, Housing, Transportation, Vehicle, Life, Communication & PC, Income, Financial Expenses, Investments, Others — each with sub-categories, icons, and colors.
