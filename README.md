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
| Mobile | .NET MAUI Blazor Hybrid (Android) |
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

### Behind a reverse proxy (HTTPS)

To reach OpenWallet from outside your home network, put it behind a reverse proxy that handles
HTTPS, such as Caddy, Traefik or nginx:

1. Set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` on the app (commented out in
   `docker-compose.yml`), so OpenWallet knows the original request was HTTPS.
2. Make the proxy pass on the original `Host` header and set `X-Forwarded-Proto`. Caddy does both
   by default. With nginx, add `proxy_set_header Host $host;` and
   `proxy_set_header X-Forwarded-Proto $scheme;`.
3. Let attachments through: nginx accepts only 1 MB by default, so add
   `client_max_body_size 50m;` (Caddy and Traefik have no limit by default).

**Passkeys** need HTTPS (browsers only allow them on `localhost` without it). A passkey belongs to
the address you created it on, taken from the `Host` header. So create passkeys through the address
you normally use, and don't change that address later, or the passkeys stop working. Passwords and
authenticator codes keep working either way.

### Automatic updates

Every push to `main` publishes the image as `ghcr.io/khalloufhassan/openwallet:latest` (and
`:sha-<commit>`, to go back to an earlier version). To have a server update itself, run that image
instead of building it, and add [Watchtower](https://github.com/nicholas-fedor/watchtower), which
checks for a new image every few minutes and restarts OpenWallet with it. In `docker-compose.yml`
(or a TrueNAS custom app's YAML):

```yaml
services:
  app:
    image: ghcr.io/khalloufhassan/openwallet:latest   # instead of build:
    labels:
      com.centurylinklabs.watchtower.enable: "true"
    # …the rest as before

  watchtower:
    image: nickfedor/watchtower:latest
    restart: unless-stopped
    environment:
      WATCHTOWER_LABEL_ENABLE: "true"     # only update containers with the label above
      WATCHTOWER_POLL_INTERVAL: "300"     # seconds
      WATCHTOWER_CLEANUP: "true"          # delete the old images
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
```

The database stays on its pinned `postgres:18` image. To go back to an earlier version, replace
`latest` with one of the `sha-…` tags listed on the repository's Packages page. After an update,
hard-refresh the browser to load the new Blazor client.

## Android app

The app shows the same pages as the web app, talking to your own OpenWallet server. It needs the
server to be reachable over HTTPS (see [Behind a reverse proxy](#behind-a-reverse-proxy-https)).

### Install

Download the latest `OpenWallet-x.y.z.apk` from the repository's
[releases](https://github.com/KhalloufHassan/OpenWallet/releases) on the phone and open it (Android asks
you to allow installing apps from the browser or file manager the first time).

When the app first opens, enter your server's address and sign in with your username, password and
authenticator code. **Use a Passkey** opens your server's login page in the browser; sign in there
and confirm to come back to the app signed in. Passkeys are added from **Security** in the app, which
opens the same page in the browser, since a passkey belongs to the server's address.

The app stays signed in for 90 days without use (`Apps__RefreshTokenDays`). Changing your password
signs it out within 30 minutes (`Apps__AccessTokenMinutes`).

### Build and run it yourself

You need the .NET SDK (from [Microsoft](https://dotnet.microsoft.com/download), not a Linux
distribution's package, which can't install workloads), a JDK 17 or 21 and the Android SDK:

```sh
dotnet workload install maui-android
# The Android SDK and JDK, if you don't have them (installs into the default locations):
dotnet build src/OpenWallet.Mobile -t:InstallAndroidDependencies -p:AndroidSdkDirectory=$HOME/Android/Sdk -p:AcceptAndroidSdkLicenses=true

# With the phone connected over USB (developer options → USB debugging), or `adb pair`/`adb connect` over Wi-Fi:
dotnet build src/OpenWallet.Mobile -t:Run -p:AndroidSdkDirectory=$HOME/Android/Sdk
```

`OpenWallet.slnx` leaves the app out, so the server builds and tests without the Android tools; add
`src/OpenWallet.Mobile/OpenWallet.Mobile.csproj` to your IDE's solution to work on it. The app's
logic that doesn't need Android is in `OpenWallet.Mobile.Core`, tested with
`dotnet test --project tests/OpenWallet.Mobile.Core.Tests`.

The app needs a server with a certificate Android trusts (such as Let's Encrypt, as a reverse proxy
provides): it never uses plain HTTP.

### Releases

Pushing a tag like `android-v1.2.0` builds a signed APK and publishes it as a GitHub release
(`.github/workflows/android-release.yml`). Android only installs updates signed with the same key, so
make the key once and keep it safe (a password manager is a good place):

```sh
keytool -genkeypair -v -keystore openwallet.keystore -alias openwallet -keyalg RSA -keysize 4096 -validity 10000
base64 -w0 openwallet.keystore    # the value of the ANDROID_KEYSTORE_BASE64 secret
```

Then add the repository secrets `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`,
`ANDROID_KEY_ALIAS` (`openwallet` above) and `ANDROID_KEY_PASSWORD` (the same as the keystore's
unless you chose another), and release with:

```sh
git tag android-v1.0.0 && git push origin android-v1.0.0
```

A server can require a newer app with `Apps__MinimumVersion` (e.g. `1.2.0`); older apps then ask to
be updated before signing in.

## Running Locally (Development)

**Prerequisites:** .NET 10 SDK, PostgreSQL instance

1. Update the connection string in `src/OpenWallet.Server/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=openwallet;Username=postgres;Password=yourpassword"
  }
}
```

2. Run the server (migrations are applied automatically on startup):

```bash
dotnet run --project src/OpenWallet.Server
```

The Blazor WASM client is served by the same host. Open `https://localhost:5001` (or the port shown in the terminal).

## Project Structure

```
OpenWallet.slnx
├── src/
│   ├── OpenWallet.Server/       # ASP.NET Core host — API controllers, EF Core, managers
│   │   ├── Auth/                # Bearer tokens and passkey codes for the mobile app
│   │   ├── Controllers/
│   │   ├── Database/
│   │   │   ├── Models/
│   │   │   └── Migrations/
│   │   └── Managers/
│   ├── OpenWallet.UI/           # All pages and components, shared by the web app and the mobile app
│   │   ├── Components/          # Reusable Blazor components
│   │   ├── Layout/
│   │   ├── Pages/
│   │   ├── Services/            # ApiClient and the host abstractions (IAppHost, IAccountService…)
│   │   └── wwwroot/             # CSS and JavaScript
│   ├── OpenWallet.Client/       # Blazor WebAssembly entry point for the web app (cookie session)
│   ├── OpenWallet.Mobile.Core/  # The mobile app's logic without Android: server address, tokens
│   ├── OpenWallet.Mobile/       # The Android app (.NET MAUI Blazor Hybrid)
│   └── OpenWallet.Shared/       # DTOs and enums shared between server and clients
│       ├── DTOs/
│       └── Models/
└── tests/
    └── OpenWallet.Mobile.Core.Tests/
```

## Default Categories

Categories are seeded on first setup: Food & Drinks, Shopping, Housing, Transportation, Vehicle, Life, Communication & PC, Income, Financial Expenses, Investments, Others — each with sub-categories, icons, and colors.
