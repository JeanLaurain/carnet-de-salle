# Réglages GitHub

## Tableau de suivi (GitHub Projects)

Les tickets sont des Issues du dépôt, rangées par jalons (0. Fondations → 6. Portfolio). Pour les voir en colonnes :

1. Dépôt → onglet **Projects** → **New project** → modèle **Board**, nom « Carnet de salle ».
2. Dans le projet : **⋯ → Workflows → Auto-add to project** → activer, filtre `is:issue,pr`, dépôt `carnet-de-salle`.
3. Ajouter les tickets existants : **+ Add item** → taper `#` → sélectionner le dépôt → tout ajouter (ou depuis la liste des Issues : tout sélectionner → **Projects**).
4. Colonnes conseillées : À faire, En cours, En revue, Terminé. Ajouter une vue **Table** groupée par **Milestone** pour suivre les phases.

## Protection de la branche `main`

Avec un compte GitHub gratuit, les règles de protection (rulesets) **ne sont pas appliquées sur un dépôt privé** : GitHub les enregistre mais affiche « Your rulesets won't be enforced on this private repository ». Il faut GitHub Pro (compte perso, payant) pour qu'elles le soient.

Ce n'est pas bloquant pour ce projet :

- la production est déjà protégée : le workflow Images relance les tests de l'API et du front et ne publie rien si l'un d'eux échoue, et le NAS n'installe que des versions publiées ;
- chaque push et chaque PR passent quand même par la CI, visible dans l'onglet Actions ;
- seul le propriétaire peut pousser sur ce dépôt.

Un dépôt **public** applique les rulesets gratuitement : voir « Rendre le dépôt public » plus bas.

Si tu passes un jour à GitHub Pro : **Settings → Rules → Rulesets → New branch ruleset** sur `main`, avec **Restrict deletions**, **Block force pushes** et **Require status checks to pass** (`Build et tests` pour l'API et le Web, `Schéma SQL Server`).

## Secrets

Aucun secret n'est stocké dans GitHub ni dans le dépôt : les mots de passe et clés restent dans le `.env` du NAS. Le workflow publie les images avec le jeton automatique `GITHUB_TOKEN`. Aucune variable de dépôt n'est nécessaire.

## Images Docker (GitHub Packages)

Les images sont dans le profil → **Packages** : `carnet-de-salle-api` et `carnet-de-salle-web`, liées au dépôt. Chaque version a un tag `sha-xxxxxxx` (le commit) et la dernière a aussi le tag `latest`.

- **Dépôt privé** : les images sont privées ; le NAS a besoin de `GITHUB_TOKEN` dans son `.env` (voir [deploiement-nas.md](deploiement-nas.md)).
- **Dépôt public** : passer chaque package en public (**Package settings → Danger Zone → Change visibility → Public**) ; le NAS n'a plus besoin de jeton. Les images ne contiennent aucun secret.

## Rendre le dépôt public

Le ticket « Ouvrir le dépôt au public » reprend cette liste :

1. Vérifier qu'aucune donnée perso ni aucun secret n'est dans le code, les tickets ou l'historique.
2. **Settings → General → Danger Zone → Change visibility → Public**.
3. **Settings → Actions → General → Fork pull request workflows from outside collaborators** : *Require approval for all external contributors*.
4. Passer les deux packages Docker en public, puis vider `GITHUB_TOKEN` dans le `.env` du NAS.
5. Activer **Settings → Code security** : secret scanning, push protection et Dependabot alerts (gratuits sur un dépôt public).
6. Créer le ruleset de `main` (appliqué gratuitement sur un dépôt public).

## Dependabot

Activé par `.github/dependabot.yml`. Pense aussi à activer **Settings → Code security → Dependabot alerts** pour être prévenu des failles connues.
