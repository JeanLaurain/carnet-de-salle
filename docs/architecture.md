# Architecture

## Vue d'ensemble

```mermaid
flowchart LR
    subgraph Appareils
      I[iPhone<br/>appli installée PWA]
      A[Android / PC<br/>navigateur]
    end
    subgraph Internet
      DNS[DNS<br/>fitness.jeanlaura.in]
    end
    subgraph Maison
      BOX[Box internet<br/>port 443 vers le NAS]
      subgraph NAS[NAS Synology]
        RP[Reverse proxy DSM<br/>HTTPS Let's Encrypt]
        subgraph Docker[Container Manager : projet carnet-de-salle]
          WEB[web : nginx<br/>Angular + /api]
          API[api : ASP.NET Core 10]
          DB[(db : SQL Server 2025 Express)]
        end
        RUN[runner GitHub Actions]
      end
    end
    I & A -->|HTTPS| DNS --> BOX --> RP -->|http://localhost:8380| WEB
    WEB -->|/api/*| API --> DB
    RUN -.->|docker compose| Docker
```

- **Un seul domaine** : le front est servi sur `/` et l'API sur `/api`, par le même conteneur nginx. Pas de CORS, et la session tient dans un cookie (voir [authentification.md](authentification.md)).
- **Seul le conteneur `web` est publié** (port 8380 du NAS), et seulement pour le reverse proxy DSM. L'API et SQL Server ne sont joignables que sur le réseau Docker interne.
- **SQL Server** tourne en conteneur, avec ses fichiers dans `/volume1/docker/carnet-de-salle/data/mssql`.

## CI/CD

```mermaid
flowchart LR
    DEV[Toi : push ou PR] --> GH[GitHub]
    GH --> CI[GitHub Actions cloud<br/>API, Web, Base de données]
    CI -->|main uniquement| IMG[Images Docker<br/>ghcr.io sha-xxxxxxx + latest]
    IMG --> DEP[Job Deploy<br/>sur le runner du NAS]
    DEP --> NAS[docker compose pull<br/>init-db, migrations, up -d<br/>test /api/health]
```

| Workflow | Quand | Ce qu'il fait |
|---|---|---|
| `api.yml` | PR et push touchant `api/` | restore, format, build, tests xUnit, couverture |
| `web.yml` | PR et push touchant `web/` | `npm ci`, lint, tests, build de production |
| `db.yml` | PR et push touchant `db/` ou `deploy/sql/` | exécute le schéma de référence et `init-db.sql` sur un vrai SQL Server |
| `deploy.yml` | push sur `main` touchant `api/`, `web/` ou `deploy/` ; ou à la main | relance les tests, construit et publie les images, puis déploie sur le NAS |
| Dependabot | chaque semaine | PR de mise à jour NuGet, npm, Actions, images Docker |

Le **runner auto-hébergé** tourne dans un conteneur sur le NAS. Il ouvre lui-même une connexion vers GitHub : aucun port entrant n'est nécessaire, ce qui convient à un serveur derrière une box. Les PR n'utilisent jamais ce runner, seuls les déploiements de `main` le font.

**Retour arrière** : Actions → Deploy → Run workflow → indiquer un tag déjà publié (`sha-1a2b3c4`, visible dans Packages).

## Organisation du dépôt

```text
carnet-de-salle/
├── api/                      API ASP.NET Core 10 (solution CarnetDeSalle)
│   ├── src/CarnetDeSalle.Api/
│   ├── tests/CarnetDeSalle.Api.Tests/
│   └── Dockerfile
├── web/                      Angular (PWA), servi par nginx
│   ├── nginx.conf
│   └── Dockerfile
├── db/
│   ├── schema.sql            schéma de référence (validé en CI)
│   ├── seed/programme.json   programme actuel (exercices, séances A et B)
│   └── ci/                   scripts de test de la CI
├── deploy/
│   ├── docker-compose.yml    production sur le NAS
│   ├── .env.example          modèle des secrets (le vrai .env reste sur le NAS)
│   ├── sql/init-db.sql       base + compte SQL de l'API
│   ├── backup.sh             sauvegarde quotidienne
│   └── runner/               runner GitHub Actions du NAS
├── docs/                     cette documentation
└── docker-compose.dev.yml    SQL Server pour développer en local
```

## Choix techniques

| Sujet | Choix | Pourquoi |
|---|---|---|
| API | ASP.NET Core 10 (LTS), minimal API, EF Core 10 | ton choix ; LTS jusqu'en novembre 2028 |
| Base | SQL Server 2025 Express en conteneur | gratuit en production, limite de 50 Go largement suffisante |
| Front | Angular récent (composants standalone, signals), PWA | ton choix ; installable sur l'iPhone |
| Auth | Identity + cookie, Google, Apple, passkeys | voir [authentification.md](authentification.md) |
| Images | GitHub Container Registry (privé) | gratuit, lié au dépôt |
| Déploiement | runner auto-hébergé + docker compose | NAS derrière une box, pas de port entrant |
| Doc de l'API | OpenAPI (`/openapi/v1.json`) + Scalar en dev | à tester avec Bruno ou le fichier `.http` |

## Contrat d'API (proposition)

Toutes les routes sont sous `/api`, au format JSON, et toutes (sauf santé et connexion) exigent d'être connecté. Les données sont toujours filtrées par l'utilisateur connecté.

| Méthode et route | Rôle |
|---|---|
| `GET /health` | santé (utilisé par le déploiement) |
| `GET /auth/me`, `POST /auth/logout` | session |
| `POST /auth/google`, `POST /auth/apple` | connexion par ID token |
| `POST /auth/passkey/...` | passkeys (voir authentification.md) |
| `GET/PUT /profile` | profil (date de naissance, taille, activité, objectif) |
| `GET /program` | programme actif : séances types, exercices, conseils |
| `GET /program/today` | séance proposée (A ou B) avec la charge conseillée par exercice |
| `GET /workouts?from&to`, `GET /workouts/{id}` | historique |
| `PUT /workouts/{id}` | enregistrer une séance terminée (Id fourni par le front, idempotent) |
| `DELETE /workouts/{id}` | supprimer |
| `POST /workouts/{id}/analysis` | analyse par Claude (phase 4) |
| `GET /stats/lifts` | charges max par exercice dans le temps (onglet Suivi) |
| `GET /weigh-ins`, `PUT /weigh-ins/{date}`, `DELETE /weigh-ins/{date}` | pesées |
| `GET /meals?date`, `PUT /meals/{id}`, `DELETE /meals/{id}` | repas |
| `GET /meals/recent` | les 6 derniers repas distincts |
| `POST /meals/estimate` | estimation par Claude (phase 4) |
| `GET /days/{date}` | bilan du jour : mangé, protéines, séances, dépense, objectif |
| `GET /days?from&to` | bilan sur plusieurs jours (graphique 14 jours) |
| `POST /import/artifact` | import unique des données de l'artefact |
