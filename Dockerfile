FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore in its own layer so dependencies are cached between code changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/OpenWallet.Shared/OpenWallet.Shared.csproj src/OpenWallet.Shared/
COPY src/OpenWallet.UI/OpenWallet.UI.csproj src/OpenWallet.UI/
COPY src/OpenWallet.Client/OpenWallet.Client.csproj src/OpenWallet.Client/
COPY src/OpenWallet.Server/OpenWallet.Server.csproj src/OpenWallet.Server/
RUN dotnet restore src/OpenWallet.Server/OpenWallet.Server.csproj

COPY src/ src/
RUN dotnet publish src/OpenWallet.Server/OpenWallet.Server.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenWallet.Server.dll"]
