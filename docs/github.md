# Réglages GitHub

## Tableau de suivi (GitHub Projects)

Les tickets sont des Issues du dépôt, rangées par jalons (0. Fondations → 5. Qualité). Pour les voir en colonnes :

1. Dépôt → onglet **Projects** → **New project** → modèle **Board**, nom « Carnet de salle ».
2. Dans le projet : **⋯ → Workflows → Auto-add to project** → activer, filtre `is:issue,pr`, dépôt `carnet-de-salle`.
3. Ajouter les tickets existants : **+ Add item** → taper `#` → sélectionner le dépôt → tout ajouter (ou depuis la liste des Issues : tout sélectionner → **Projects**).
4. Colonnes conseillées : À faire, En cours, En revue, Terminé. Ajouter une vue **Table** groupée par **Milestone** pour suivre les phases.

## Protection de la branche `main`

Dépôt → **Settings → Rules → Rulesets → New branch ruleset** :

- Cible : `main`
- Cocher **Restrict deletions** et **Block force pushes**
- **Require status checks to pass** : `Build et tests` (API), `Build et tests` (Web), `Schéma SQL Server`
- Facultatif quand tu travailles seul : **Require a pull request before merging** (0 approbation requise)

## Variables et environnement

Voir [deploiement-nas.md](deploiement-nas.md#5-les-variables-github) : `DEPLOY_DIR`, `APP_DOMAIN`, `DEPLOY_ENABLED` et l'environnement `production`.

Aucun secret n'est stocké dans GitHub : les mots de passe et clés restent dans le `.env` du NAS. Le workflow publie les images avec le jeton automatique `GITHUB_TOKEN`.

## Images Docker (GitHub Packages)

Après le premier déploiement, les images apparaissent dans le profil → **Packages** : `carnet-de-salle-api` et `carnet-de-salle-web`. Elles sont privées et liées au dépôt. Chaque version a un tag `sha-xxxxxxx` (le commit) et la dernière a aussi le tag `latest`.

## Dependabot

Activé par `.github/dependabot.yml`. Pense aussi à activer **Settings → Code security → Dependabot alerts** pour être prévenu des failles connues.
