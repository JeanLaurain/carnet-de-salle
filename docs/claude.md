# Intégrer Claude

Dans l'artefact, Claude tournait « gratuitement » parce que la page utilisait ton compte Claude. Sur ton propre site, ce n'est plus le cas : il faut choisir comment Claude intervient. Il y a deux façons, complémentaires.

| | A. Claude **dans** l'appli | B. L'appli **dans** Claude |
|---|---|---|
| Ce que ça fait | l'API appelle Claude : estimation des kcal d'un repas (texte ou photo), analyse d'une séance | tu discutes avec Claude sur claude.ai ou dans l'appli Claude, et il lit (et peut écrire) tes séances, repas et pesées |
| Exemple | « Pâtes bolognaise, grosse assiette » → 670 kcal, 32 g de protéines | « Analyse ma semaine et ajuste mon programme » |
| Technique | package NuGet officiel `Anthropic` dans l'API | un **serveur MCP** dans l'API, ajouté comme connecteur personnalisé dans Claude |
| Coût | API Anthropic facturée à l'usage, séparément de ton abonnement Claude | inclus dans ton abonnement Claude |
| Effort | faible | moyen (OAuth à mettre en place) |

**Recommandation** : faire **A** d'abord pour les repas, car sans estimation automatique l'onglet Repas demande de saisir les kcal à la main. Puis **B** quand l'appli est stable, pour retrouver le suivi en conversation qu'on a aujourd'hui.

## A. Claude dans l'appli (API Anthropic)

1. Créer une clé sur [platform.claude.com](https://platform.claude.com/) (Console Anthropic), avec une limite de dépense mensuelle.
2. La stocker sur le NAS dans `.env` (`ANTHROPIC_API_KEY`), jamais dans le dépôt.
3. Dans l'API : package NuGet `Anthropic` (SDK C# officiel, voir la [documentation](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/csharp.md)).
4. Endpoints :
   - `POST /api/meals/estimate` (multipart : texte + 0 à 3 images) → estimation, **sans enregistrer** ;
   - `POST /api/workouts/{id}/analysis` → analyse enregistrée dans `app.WorkoutAnalyses`.
5. Prompts, format JSON et nettoyage des réponses : [prompts/estimation-repas.md](prompts/estimation-repas.md) et [prompts/analyse-seance.md](prompts/analyse-seance.md). Utiliser les sorties structurées (JSON schema) du SDK si disponibles, et garder le nettoyage côté serveur dans tous les cas.
6. Journaliser chaque appel dans `app.AiUsage` (modèle, jetons) et afficher le total du mois dans « Mon compte ». Ajouter une limite par jour (par exemple 30 estimations).
7. Coût : quelques centimes par jour pour un usage perso (à vérifier sur la [page des tarifs](https://claude.com/pricing)). Une estimation de repas consomme environ 1 000 à 3 000 jetons, davantage avec une photo.

## B. L'appli dans Claude (serveur MCP)

Un serveur [MCP](https://modelcontextprotocol.io/) expose des « outils » que Claude peut appeler. Ton API en héberge un sur `https://fitness.jeanlaura.in/mcp`, et tu l'ajoutes dans Claude via **Customize → Connectors → Add custom connector** ([documentation](https://claude.com/docs/connectors/custom)).

- Package NuGet : `ModelContextProtocol.AspNetCore` (SDK C# officiel de MCP), transport HTTP « Streamable ».
- Outils proposés :
  - lecture : `get_program`, `list_workouts(from, to)`, `get_workout(id)`, `list_meals(date)`, `get_day_summary(date)`, `get_weight_history(from, to)` ;
  - écriture (avec confirmation dans Claude) : `add_meal`, `add_weigh_in`, `save_workout_analysis`, `propose_program_version`.
- Authentification : Claude se connecte en **OAuth**. L'API doit donc jouer le rôle de serveur d'autorisation (par exemple avec OpenIddict), en réutilisant ta connexion Google, Apple ou passkey. Ne jamais exposer le serveur MCP sans authentification : il donne accès à toutes tes données.
- Le serveur doit être joignable depuis internet (c'est le cas via le reverse proxy du NAS).

Avec B, plus besoin d'artefact : tu demandes à Claude « regarde ma séance de ce matin », il lit tes données, propose une consigne, et peut l'enregistrer dans l'appli.
