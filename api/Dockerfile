# syntax=docker/dockerfile:1
# Image de l'API : build avec le SDK .NET 10, exécution sur l'image ASP.NET (utilisateur non root).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj src/CarnetDeSalle.Api/
RUN dotnet restore src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj
COPY src/ src/
RUN dotnet publish src/CarnetDeSalle.Api/CarnetDeSalle.Api.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ARG APP_VERSION=dev
ENV App__Version=${APP_VERSION} \
    ASPNETCORE_HTTP_PORTS=8080
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CarnetDeSalle.Api.dll"]
