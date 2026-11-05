# syntax=docker/dockerfile:1
# Image de l'API : build avec le SDK .NET 10, exécution sur l'image ASP.NET Alpine (utilisateur non root).
#
# Alpine (musl) et non Ubuntu : sur Docker 20.10.3 (paquet Docker de DSM 7.1), les images glibc récentes
# ne peuvent pas créer de threads (appel système clone3 refusé), et .NET ne démarre pas.
# La CI le vérifie : .github/workflows/docker.yml.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj src/CarnetDeSalle.Api/
RUN dotnet restore src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj
COPY src/ src/
RUN dotnet publish src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
# ICU : cultures complètes (dates et nombres en français) et client SQL Server, qui refuse le mode
# « globalisation invariante » activé par défaut sur Alpine. tzdata : fuseau Europe/Paris.
RUN apk add --no-cache icu-data-full icu-libs tzdata
ARG APP_VERSION=dev
ENV App__Version=${APP_VERSION} \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CarnetDeSalle.Api.dll"]
