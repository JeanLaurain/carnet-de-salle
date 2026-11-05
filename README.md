# Carnet de salle

Appli perso de suivi de musculation, de repas et de poids, installable sur l'iPhone. Elle reprend l'artefact Claude « Carnet de salle » ([copie de référence](docs/reference/carnet-de-salle-artefact.html)) sous forme de vraie appli :

- **API** : ASP.NET Core 10 (C#), EF Core, ASP.NET Core Identity
- **Base** : SQL Server 2025 Express
- **Front** : Angular, en PWA (installable depuis Safari)
- **Hébergement** : NAS Synology, Docker, derrière le reverse proxy DSM, sur `fitness.jeanlaura.in`
- **CI/CD** : GitHub Actions, images sur GitHub Container Registry, mise à jour automatique tirée par le NAS (aucun port entrant)

## Par où commencer

1. Lire le [plan de route](#plan-de-route) ci-dessous, puis prendre les tickets dans l'ordre des jalons ([Issues](https://github.com/JeanLaurain/carnet-de-salle/issues), [jalons](https://github.com/JeanLaurain/carnet-de-salle/milestones)).
2. Installer les outils et lancer l'appli en local : [docs/developpement.md](docs/developpement.md).
3. Préparer le NAS et faire le premier déploiement : [docs/deploiement-nas.md](docs/deploiement-nas.md).

## Documentation

| Document | Contenu |
|---|---|
| [architecture.md](docs/architecture.md) | schémas, CI/CD, organisation du dépôt, contrat d'API |
| [base-de-donnees.md](docs/base-de-donnees.md) | structure de la base, diagramme, correspondance avec l'artefact ([schema.sql](db/schema.sql)) |
| [regles-metier.md](docs/regles-metier.md) | toutes les formules : charges conseillées, kcal des séances, dépense, objectifs, bilan |
| [authentification.md](docs/authentification.md) | Google, Apple, passkeys (Face ID, empreinte), session |
| [claude.md](docs/claude.md) | intégrer Claude : estimation des repas, analyse des séances, serveur MCP |
| [prompts/](docs/prompts/) | les prompts de l'artefact et le nettoyage des réponses |
| [deploiement-nas.md](docs/deploiement-nas.md) | Synology : SQL Server, mise à jour automatique, reverse proxy, sauvegardes |
| [developpement.md](docs/developpement.md) | outils, lancer en local, migrations, façon de travailler |
| [github.md](docs/github.md) | tableau de suivi, protection de `main`, variables |

## Plan de route

| Jalon | Objectif |
|---|---|
| **0. Fondations** | NAS prêt, domaine choisi, mise à jour automatique installée, premier déploiement du squelette, sauvegardes |
| **1. Base et API** | entités EF Core, migrations, programme, séances, pesées, repas, bilans, import des données de l'artefact |
| **2. Authentification** | Google, Apple, passkeys, données par utilisateur |
| **3. Front PWA** | les 5 onglets de l'artefact, installation sur l'iPhone, mode hors ligne pendant la séance |
| **4. Claude** | estimation des repas (texte et photo), analyse des séances, serveur MCP pour Claude |
| **5. Qualité** | tests de bout en bout, supervision, restauration testée |
| **6. Portfolio** | dépôt ouvert au public, compte de démo, README vitrine |

## État actuel

- Squelette API : `GET /api/health`, `GET /api/version`, tests d'intégration.
- Squelette front : page d'accueil qui interroge l'API, PWA (manifeste, service worker), thème de l'artefact.
- CI : API, Web et Base de données verts ; le workflow Images publie les images, que le NAS installe tout seul via `deploy/update.sh`.
