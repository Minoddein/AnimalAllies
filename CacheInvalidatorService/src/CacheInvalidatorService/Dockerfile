FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    dotnet nuget add source "https://gitlab.com/api/v4/projects/63188031/packages/nuget/index.json" \
    --name "gitlab-mcconder" \
    --username "$(cat /run/secrets/nuget_username)" \
    --password "$(cat /run/secrets/nuget_password)" \
    --store-password-in-clear-text

COPY ["src/CacheInvalidatorService/CacheInvalidatorService.csproj", "src/CacheInvalidatorService/"]
RUN dotnet restore "src/CacheInvalidatorService/CacheInvalidatorService.csproj"
COPY . .
WORKDIR "/src/src/CacheInvalidatorService"
RUN dotnet build "./CacheInvalidatorService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CacheInvalidatorService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CacheInvalidatorService.dll"]
