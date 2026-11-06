# Réglages GitHub

## Tableau de suivi (GitHub Projects)

Les tickets sont des Issues du dépôt, rangées par jalons (0. Fondations → 6. Portfolio). Pour les voir en colonnes :

1. Dépôt → onglet **Projects** → **New project** → modèle **Board**, nom « Carnet de salle ».
2. Dans le projet : **⋯ → Workflows → Auto-add to project** → activer, filtre `is:issue,pr`, dépôt `carnet-de-salle`.
3. Ajouter les tickets existants : **+ Add item** → taper `#` → sélectionner le dépôt → tout ajouter (ou depuis la liste des Issues : tout sélectionner → **Projects**).
4. Colonnes conseillées : À faire, En cours, En revue, Terminé. Ajouter une vue **Table** groupée par **Milestone** pour suivre les phases.

## Protection de la branche `main`

GitHub n'applique les règles de protection (rulesets) que sur un dépôt **public** avec un compte gratuit (sur un dépôt privé, il faut GitHub Pro). La règle se crée donc juste après l'ouverture du dépôt (ticket « Ouvrir le dépôt au public »).

Réglages : **Settings → Rules → Rulesets → New ruleset → New branch ruleset**

| Champ | Valeur |
|---|---|
| Ruleset name | `main` |
| Enforcement status | **Active** |
| Target branches | **Include default branch** |
| Restrict deletions | coché |
| Block force pushes | coché |
| Require a pull request before merging | coché, **0** approbation requise (tu travailles seul) |
| Require status checks to pass | coché, ajouter le contrôle **`CI`** |
| Bypass list | vide |

Pour ajouter le contrôle : cocher **Require status checks to pass**, cliquer sur **+ Add checks**, taper `CI` et choisir **CI** (GitHub Actions). Ne pas ajouter « Fichiers modifiés » ni les contrôles « api / ... », « web / ... », « db / ... » : ils sont sautés quand une PR ne les concerne pas et la bloqueraient.

Pourquoi un seul contrôle `CI` : le workflow `ci.yml` ne lance que les contrôles concernés par la PR (API, Web, Base de données, Images Docker) puis publie toujours un statut `CI`. Exiger directement « Build et tests » bloquerait les PR qui ne touchent que la doc, puisque ces contrôles n'y tournent pas.

Avec cette règle :

- tout passe par une PR, y compris tes propres changements et ceux de Dependabot ;
- une PR ne peut être fusionnée que si `CI` est vert ;
- personne ne peut supprimer `main` ni réécrire son historique.

Si la CI est cassée par un élément extérieur (une action GitHub retirée, par exemple) et bloque même la PR de correction : passer la règle en **Disabled** le temps de fusionner le correctif, puis la remettre en **Active**.

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
6. Créer la règle de protection de `main` (section ci-dessus), avec le contrôle `CI`.

## Dependabot

Activé par `.github/dependabot.yml`. Pense aussi à activer **Settings → Code security → Dependabot alerts** pour être prévenu des failles connues.
