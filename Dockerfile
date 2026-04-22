FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OpenWallet.sln .
COPY OpenWallet.Shared/OpenWallet.Shared.csproj OpenWallet.Shared/
COPY OpenWallet.Client/OpenWallet.Client.csproj OpenWallet.Client/
COPY OpenWallet/OpenWallet.csproj OpenWallet/

RUN dotnet restore OpenWallet.sln

COPY OpenWallet.Shared/ OpenWallet.Shared/
COPY OpenWallet.Client/ OpenWallet.Client/
COPY OpenWallet/ OpenWallet/

RUN dotnet publish OpenWallet/OpenWallet.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenWallet.dll"]
