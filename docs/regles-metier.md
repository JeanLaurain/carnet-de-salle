# Règles métier

Toutes les règles de l'artefact, à reprendre dans l'API (services C# testés) et dans le front quand le calcul doit marcher hors ligne. Le code d'origine est dans [`docs/reference/carnet-de-salle-artefact.html`](reference/carnet-de-salle-artefact.html) : les noms de fonctions JavaScript sont indiqués entre parenthèses.

> Conseil : chaque règle de cette page = une classe de service + des tests unitaires avec les exemples chiffrés donnés ici.

## 1. Choix de la séance du jour (`prochaine`)

- Les séances types du programme actif tournent dans l'ordre de `Position` : A → B → A avec deux séances, A → B → C → A avec trois.
- Aucune séance : proposer la première (A).
- Sinon : la séance qui suit la dernière réalisée (tri par `Date` décroissante puis `CreatedAt` décroissant). Si la dernière séance a été faite avec un code qui n'existe plus dans le programme actif, repartir de la première.
- L'utilisateur peut choisir une autre séance à la main.
- Le nombre de séances par semaine ne change rien au calcul : 3 séances avec un programme A/B donnent A, B, A puis B, A, B la semaine suivante.

## 2. Charge conseillée sur un exercice (`suggestionAuto`, `suggestion`)

On regarde la **dernière fois** que l'exercice a été fait (toutes séances confondues).

### Exercice à charge (`load`)
| Situation | Conseil |
|---|---|
| Jamais fait | « Prends une charge que tu pourrais pousser 15 fois, et note-la. » Pas de charge proposée. |
| `top` = charge max de la dernière fois, `bestReps` = meilleur nombre de reps à `top`. Si `bestReps ≥ repsMax + 3` | Trop léger. Charge visée = `top × (1 + bestReps/30) / (1 + repsMax/30)` (formule d'Epley), arrondie au palier (`LoadIncrementKg`), au minimum `top + palier`. |
| Toutes les séries à `top`, nombre de séries ≥ prévu, et toutes à `repsMax` ou plus | Monter d'un palier : `top + palier`. |
| Toutes les séries à `top` | Garder `top`, gagner une répétition. |
| Charges différentes entre les séries | Faire toutes les séries à `top`. |

Exemple : presse, palier 5 kg, repsMax 12, dernière fois 70×12, 70×12, 80×16 → `top` 80, `bestReps` 16 ≥ 15 → 80 × (1 + 16/30) / (1 + 12/30) = 87,6 → arrondi 90 kg (et ≥ 85).

### Exercice en secondes (`time`)
- Jamais fait : viser 20 s, jusqu'à `SecondsMax`.
- Sinon : `min` = série la plus courte la dernière fois ; viser `min + 5` s.

### Exercice en répétitions seules (`reps`)
- Jamais fait : `RepsMin`. Sinon `min(minReps + 1, RepsMax)` ; si `minReps ≥ RepsMax`, garder `RepsMax` et ralentir (3 s par mouvement).

### Priorité aux consignes de Claude (`notesCoach`)
Si une analyse (ou une consigne manuelle) existe pour l'exercice **et** que l'exercice n'a pas été refait depuis sa date, on affiche sa consigne et sa charge (ou durée) à la place du conseil automatique.

## 3. Estimation des kcal brûlées pendant une séance (`kcalSeance`)

Valeurs MET (Compendium of Physical Activities), comptées **nettes du repos** : `kcal = (MET − 1) × poids (kg) × durée (h)`, car la dépense de base du jour couvre déjà ce temps au repos.

| Bloc | MET | Durée retenue |
|---|---|---|
| Musculation | 3,5 | somme des séries : 40 s d'effort (ou la durée de la série en secondes) + le repos de l'exercice (90 s par défaut, 60 s abdos) |
| Échauffement (s'il est coché) | 4,0 | 8 min (le programme dit 5 à 10) |
| Cardio | `CardioMet` de la séance type : tapis incliné 7,0 ; vélo 6,0 | minutes saisies |

- Si la durée totale de la séance est connue (≥ 20 min), la durée de musculation est plafonnée à `durée − échauffement − cardio` (au moins 10 min).
- Poids : la dernière pesée à la date de la séance (ou la première pesée, sinon 80 kg par défaut).
- Résultat arrondi à 10 kcal.
- Exemple : 80 kg, 12 séries à charge et 3 séries de gainage (52, 53, 54 s), échauffement fait, 15 min de tapis incliné, 51 min au total → musculation plafonnée à 28 min → 93 + 32 + 120 ≈ 250 kcal.

Estimation « prévue » affichée sur la séance du jour : même calcul avec les séries prévues, l'échauffement fait et le cardio prévu.

## 4. Dépense du jour, objectif et protéines (`depenseBase`, `bilanJour`)

- Métabolisme de base, formule de **Mifflin-St Jeor** : `10 × poids + 6,25 × taille (cm) − 5 × âge + 5` (homme) ou `− 161` (femme).
- Dépense de base = métabolisme × `ActivityFactor` (1,2 plutôt assis ; 1,375 un peu actif ; 1,55 actif).
- Dépense du jour = dépense de base + somme des kcal des séances du jour.
- Objectif du jour = dépense du jour + `DailyKcalOffset` (0, −300 ou −500), arrondi à 10.
- Pas de date de naissance renseignée : pas de dépense ni d'objectif (l'UI invite à compléter le profil).

### Objectifs de macronutriments

Les kcal disent si on perd du gras ; la répartition dit **ce qu'on perd** (du gras plutôt que du muscle) et si on tient sur la durée.

| Nutriment | Objectif | Type | Pourquoi |
|---|---|---|---|
| Protéines | poids × `ProteinGPerKg` (1,8 g/kg), arrondi à 5 g | **au moins** | garder et construire le muscle en déficit (l'ISSN recommande 1,4 à 2,0 g/kg chez les sportifs) |
| Lipides | objectif kcal × `FatShareOfKcal` (30 %) / 9, arrondi à 5 g | **environ** (alerte au-delà de +10 %) | hormones et vitamines : ne pas descendre trop bas en déficit. L'ANSES vise 35 à 40 % pour la population générale ; 30 % laisse de la place aux protéines |
| Glucides | (objectif − protéines × 4 − lipides × 9) / 4, arrondi à 5 g | **environ** (alerte au-delà de +10 %) | l'énergie des séances : c'est le reste du budget |
| Fibres | `FiberTargetG` (30 g) | **au moins** | satiété (aide à tenir le déficit), digestion ; repère ANSES 25 à 30 g |

- Les objectifs de lipides et de glucides n'existent que si l'objectif kcal existe (profil complet). Protéines et fibres sont toujours affichées.
- Exemple (homme, 30 ans, 80 kg, 175 cm, un peu actif, −500) : métabolisme 1 749 kcal, dépense de base 2 405 kcal, objectif 1 910 kcal un jour sans séance → protéines 145 g, lipides 65 g, glucides 185 g, fibres 30 g.
- Fibres non estimées (`FiberG` NULL) : le total affiché est un minimum, et l'UI le signale (« fibres non estimées pour 1 repas »).

## 5. Bilan sur 14 jours (`htmlBilan`)

- Fenêtre : les 14 derniers jours, aujourd'hui compris.
- Un jour est **complet** s'il a au moins un repas, s'il n'est pas aujourd'hui, et s'il dépasse 1 000 kcal notées. Les jours incomplets s'affichent en barre pâle et sont exclus des moyennes.
- Indicateurs sur les jours complets : moyenne mangée, moyenne dépensée, écart moyen, écart cumulé.
- Équivalent gras : `|écart cumulé| / 7 700` kg.
- Comparaison avec la balance : pesée la plus récente avant le premier jour complet (sinon la première pesée dans la fenêtre), contre la dernière pesée ≤ aujourd'hui.
- Protéines : moyenne par jour sur les jours complets, comparée à l'objectif, et nombre de jours où l'objectif est atteint.
- Fibres : moyenne sur les jours complets dont tous les repas ont une valeur de fibres.
- Tour de taille : première et dernière mesure (page Poids). Balance stable + tour de taille en baisse = recomposition réussie.

## 6. Repas

- Moment par défaut selon l'heure : avant 10 h 30 petit-déj, avant 15 h déjeuner, avant 18 h 30 en-cas, sinon dîner.
- Portions : de 0,5 à 12, par pas de 0,5. Totaux = valeurs pour 1 portion × portions.
- « Déjà mangé récemment » : les 6 derniers noms de repas distincts (insensible à la casse) ; les reprendre recopie les valeurs pour 1 portion et le nombre de portions.
- Saisie manuelle possible (nom, kcal, protéines facultatives), même sans Claude.

## 7. Séance en cours (brouillon)

- Le brouillon (séries cochées, charges, cardio, ressenti) est gardé **sur l'appareil** (IndexedDB ou localStorage), une clé par séance type.
- Il expire après 3 jours. Si le programme change de version, on garde les séries des exercices inchangés.
- À la fin : seules les séries renseignées sont envoyées, avec la date du début de séance, la durée (début → fin) et l'Id généré par le front.
- Minuteur de repos : démarre quand on valide une série (repos de l'exercice), bouton +30 s, bip à la fin. Chrono plein écran pour les exercices en secondes, avec 5 s de mise en place.

## 8. Changer le programme

Toute modification crée une **nouvelle version** du programme ; les anciennes restent consultables et les séances passées gardent leur version.

| Modification | Effet |
|---|---|
| Remplacer un exercice (nouvelle machine) | même place, mêmes séries et répétitions par défaut ; la charge conseillée repart de la règle « jamais fait » puisque la machine est différente |
| Ajouter, retirer, réordonner un exercice | dans une séance type |
| Changer séries, répétitions, palier, repos | par exercice |
| Ajouter ou retirer une séance type (C, D...) | la rotation du §1 en tient compte |
| Changer le cardio, l'échauffement, la fréquence | textes et minutes |
| Durée du cycle | `PlannedWeeks` (par exemple 8 semaines) |

- Exercices proposés pour un remplacement : d'abord ceux qui travaillent les mêmes muscles, puis tout le catalogue, puis « Créer un exercice » (nom, muscles, type de mesure, conseils, recherche vidéo).
- Les notes de version (`ChangeNotes`) sont générées à partir des différences : « Séance B : hack squat remplacé par goblet squat », « Nouvelle séance C ». L'utilisateur peut les modifier avant d'enregistrer.
- Le brouillon d'une séance en cours garde les séries des exercices inchangés (§7).
- **Rappel de cycle** : à partir de `StartsOn + PlannedWeeks`, l'onglet Programme affiche « Semaine 8 sur 8 : c'est le moment de changer quelques machines », avec un bouton vers l'éditeur (et, plus tard, « Demander à Claude de proposer la suite »).
- Retour arrière : réactiver une ancienne version crée une nouvelle version identique à celle-ci.

## 9. Claude

Les deux prompts de l'artefact, avec leur format de réponse JSON et les règles de nettoyage de la réponse, sont dans [`docs/prompts/`](prompts/). Voir aussi [claude.md](claude.md) pour les façons d'intégrer Claude à l'appli.
