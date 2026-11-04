# Règles métier

Toutes les règles de l'artefact, à reprendre dans l'API (services C# testés) et dans le front quand le calcul doit marcher hors ligne. Le code d'origine est dans [`docs/reference/carnet-de-salle-artefact.html`](reference/carnet-de-salle-artefact.html) : les noms de fonctions JavaScript sont indiqués entre parenthèses.

> Conseil : chaque règle de cette page = une classe de service + des tests unitaires avec les exemples chiffrés donnés ici.

## 1. Choix de la séance du jour (`prochaine`)

- Aucune séance : proposer **A**.
- Sinon : l'inverse de la dernière séance réalisée (A → B, B → A). Tri par `Date` décroissante puis `CreatedAt` décroissant.
- L'utilisateur peut choisir l'autre séance à la main.

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
- Poids : la dernière pesée à la date de la séance (ou la première pesée, sinon 79 kg).
- Résultat arrondi à 10 kcal.
- Exemple : séance A du 4 nov. (51 min, 15 min de tapis, 79 → 78,5 kg) ≈ 240 kcal.

Estimation « prévue » affichée sur la séance du jour : même calcul avec les séries prévues, l'échauffement fait et le cardio prévu.

## 4. Dépense du jour, objectif et protéines (`depenseBase`, `bilanJour`)

- Métabolisme de base, formule de **Mifflin-St Jeor** : `10 × poids + 6,25 × taille (cm) − 5 × âge + 5` (homme) ou `− 161` (femme).
- Dépense de base = métabolisme × `ActivityFactor` (1,2 plutôt assis ; 1,375 un peu actif ; 1,55 actif).
- Dépense du jour = dépense de base + somme des kcal des séances du jour.
- Objectif du jour = dépense du jour + `DailyKcalOffset` (0, −300 ou −500), arrondi à 10.
- Protéines visées = poids × `ProteinGPerKg` (1,8 g/kg), arrondi à 5 g.
- Pas de date de naissance renseignée : pas de dépense ni d'objectif (l'UI invite à compléter le profil).

## 5. Bilan sur 14 jours (`htmlBilan`)

- Fenêtre : les 14 derniers jours, aujourd'hui compris.
- Un jour est **complet** s'il a au moins un repas, s'il n'est pas aujourd'hui, et s'il dépasse 1 000 kcal notées. Les jours incomplets s'affichent en barre pâle et sont exclus des moyennes.
- Indicateurs sur les jours complets : moyenne mangée, moyenne dépensée, écart moyen, écart cumulé.
- Équivalent gras : `|écart cumulé| / 7 700` kg.
- Comparaison avec la balance : pesée la plus récente avant le premier jour complet (sinon la première pesée dans la fenêtre), contre la dernière pesée ≤ aujourd'hui.

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

## 8. Claude

Les deux prompts de l'artefact, avec leur format de réponse JSON et les règles de nettoyage de la réponse, sont dans [`docs/prompts/`](prompts/). Voir aussi [claude.md](claude.md) pour les façons d'intégrer Claude à l'appli.
