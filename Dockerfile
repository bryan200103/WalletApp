FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Forzar build limpio
COPY WalletApi/WalletApi.csproj ./WalletApi/
WORKDIR /app/WalletApi
RUN dotnet restore

WORKDIR /app
COPY WalletApi/. ./WalletApi/
WORKDIR /app/WalletApi

# Limpiar cualquier cache anterior
RUN rm -rf obj bin
RUN dotnet publish -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WalletApi.dll"]
