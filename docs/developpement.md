# Développer en local (Windows)

## Outils à installer

| Outil | Pour | Installation |
|---|---|---|
| Visual Studio 2026 (charge « Développement ASP.NET et web ») ou Rider | l'API C# | [visualstudio.microsoft.com](https://visualstudio.microsoft.com/) |
| SDK .NET 10 | compiler et tester l'API (installé avec VS) | `winget install Microsoft.DotNet.SDK.10` |
| Node.js LTS (26) | le front Angular | `winget install OpenJS.NodeJS.LTS` |
| Angular CLI | commandes `ng` | `npm install -g @angular/cli` |
| Docker Desktop | SQL Server en local, tester les images | `winget install Docker.DockerDesktop` |
| Outils EF Core | migrations | `dotnet tool install --global dotnet-ef` |
| Bruno (ou le fichier `.http` de VS) | tester l'API à la main | `winget install Bruno.Bruno` |
| VS Code (facultatif) | le front | `winget install Microsoft.VisualStudioCode` |
| SQL Server Management Studio ou l'extension « SQL Server (mssql) » de VS Code | explorer la base | `winget install Microsoft.SQLServerManagementStudio` |

## Lancer l'appli

```powershell
git clone https://github.com/JeanLaurain/carnet-de-salle.git
cd carnet-de-salle

# 1. SQL Server 2019 dans Docker (même version que le NAS)
docker compose -f docker-compose.dev.yml up -d
# Si la base locale a été créée avec SQL Server 2022 ou 2025, la 2019 ne sait pas l'ouvrir :
# docker compose -f docker-compose.dev.yml down -v   (efface la base locale), puis up -d

# 2. API (http://localhost:5113)
cd api/src/CarnetDeSalle.Api
dotnet watch

# 3. Front (http://localhost:4200), dans un autre terminal
cd web
npm install
npm start
```

Le front appelle `/api/...` : en local, `ng serve` relaie ces appels vers `http://localhost:5113` (fichier `web/proxy.conf.json`). C'est la même organisation qu'en production, où nginx fait ce relais.

## Tests

```powershell
cd api;  dotnet test
cd web;  npm test
```

## Migrations EF Core

```powershell
cd api
dotnet ef migrations add NomDeLaMigration --project src/CarnetDeSalle.Api --output-dir Data/Migrations
dotnet ef database update --project src/CarnetDeSalle.Api
# Script SQL pour vérifier ce que la migration va faire :
dotnet ef migrations script --idempotent --project src/CarnetDeSalle.Api
```

En production, les migrations sont appliquées par le service `migrate` (l'image de l'API lancée avec `--migrate`) à chaque déploiement.

## Tester sur l'iPhone avant de déployer

- Le plus simple : déployer sur le NAS (chaque push sur `main`) et ouvrir `https://fitness.jeanlaura.in`.
- Les passkeys et l'installation de la PWA exigent HTTPS : elles ne marchent pas sur `http://192.168.x.x:4200`.

## Façon de travailler

1. Prendre un ticket sur le tableau GitHub, le passer en « En cours ».
2. Créer une branche : `git switch -c 12-endpoints-pesees` (numéro du ticket en tête).
3. Committer petit et souvent (`feat: ...`, `fix: ...`, `chore: ...`, `docs: ...`).
4. Ouvrir une PR avec « Ferme #12 » dans la description : la CI tourne, le ticket se ferme au merge.
5. Merger sur `main` : le déploiement part tout seul.
