# Déploiement sur le NAS Synology

À faire une fois, dans l'ordre. Compte environ une heure. Les tickets du jalon « 0. Fondations » reprennent ces étapes.

## 1. Vérifier que le NAS peut faire tourner SQL Server

SQL Server n'existe qu'en version **x86_64** (processeur Intel ou AMD) et demande **au moins 2 Go de RAM** pour lui seul.

- DSM → **Panneau de configuration → Centre d'infos → Général** : noter le modèle, le processeur et la mémoire.
- Ou en SSH : `uname -m` doit afficher `x86_64`, et `free -m` la mémoire.

| Résultat | Suite |
|---|---|
| x86_64 et 4 Go ou plus | parfait |
| x86_64 et 2 Go | possible en baissant `MSSQL_MEMORY_LIMIT_MB` à 1024, mais ajouter de la RAM est recommandé |
| ARM (séries « j », « play », « value » récentes) | SQL Server ne tournera pas sur le NAS : il faudra une autre machine pour la base, ou changer de moteur |

## 2. Préparer DSM

1. **Centre de paquets** → installer **Container Manager** (DSM 7.2 ou plus).
2. **Panneau de configuration → Terminal et SNMP** → activer SSH (le temps de l'installation).
3. En SSH (`ssh ton-compte@ip-du-nas`, puis `sudo -i`) :

```sh
mkdir -p /volume1/docker/carnet-de-salle/data/mssql \
         /volume1/docker/carnet-de-salle/data/backups \
         /volume1/docker/carnet-de-salle/data/dataprotection
# SQL Server tourne avec l'utilisateur 10001, l'API .NET avec l'utilisateur 1654
chown -R 10001:0 /volume1/docker/carnet-de-salle/data/mssql /volume1/docker/carnet-de-salle/data/backups
chown -R 1654:1654 /volume1/docker/carnet-de-salle/data/dataprotection
```

## 3. Les secrets (`.env`)

Copier [`deploy/.env.example`](../deploy/.env.example) dans `/volume1/docker/carnet-de-salle/.env` et remplir les valeurs : domaine, mots de passe SQL, emails autorisés, identifiants Google et Apple quand tu les auras. Ce fichier ne quitte jamais le NAS.

```sh
chmod 600 /volume1/docker/carnet-de-salle/.env
```

## 4. La mise à jour automatique

C'est le NAS qui va chercher les nouvelles versions : pas de runner GitHub, pas de port entrant, et GitHub ne peut rien exécuter sur le NAS.

1. Récupérer le script une première fois (il se met à jour tout seul ensuite) :

```sh
cd /volume1/docker/carnet-de-salle
curl -fsSL https://raw.githubusercontent.com/JeanLaurain/carnet-de-salle/main/deploy/update.sh -o update.sh
```

   Tant que le dépôt est **privé** : créer un jeton GitHub classique (**Settings → Developer settings → Personal access tokens → Tokens (classic)**) avec les droits `repo` et `read:packages`, ajouter `-H "Authorization: Bearer <jeton>"` à la commande ci-dessus, et mettre le jeton dans `GITHUB_TOKEN` du `.env`. Une fois le dépôt et les images publics, vider `GITHUB_TOKEN` et révoquer le jeton.

2. DSM → **Panneau de configuration → Planificateur de tâches → Créer → Tâche planifiée → Script défini par l'utilisateur** :
   - Général : nom « Carnet de salle : mise à jour », utilisateur **root** ;
   - Programmation : tous les jours, toutes les **5 minutes** ;
   - Paramètres de tâche : `bash /volume1/docker/carnet-de-salle/update.sh`

3. Commandes utiles :

| Besoin | Commande (dans `/volume1/docker/carnet-de-salle`) |
|---|---|
| Voir ce qui s'est passé | `tail -n 50 logs/update.log` |
| Redéployer tout de suite | `bash update.sh --force` |
| Suspendre les mises à jour | `touch .pause` (et `rm .pause` pour reprendre) |
| Revenir à une version | `IMAGE_TAG=sha-1a2b3c4` dans `.env`, puis `bash update.sh` |
| Suivre de nouveau `main` | `IMAGE_TAG=` vide dans `.env` |
| Version en ligne | `cat .deployed` |

Si une nouvelle version ne répond pas sur `/api/health`, le script remet la précédente et note la version dans `.failed` pour ne pas réessayer en boucle. Les migrations déjà appliquées restent en place : écrire des migrations compatibles avec la version précédente (ajouter des colonnes plutôt que d'en supprimer).

## 5. Les images Docker

Les images sont publiées par le workflow **Images** sur `ghcr.io/jeanlaurain/carnet-de-salle-api` et `-web`.

- Dépôt privé : images privées, le NAS utilise `GITHUB_TOKEN` (étape 4).
- Dépôt public : passer les deux packages en public (profil GitHub → **Packages** → package → **Package settings → Change visibility → Public**), puis vider `GITHUB_TOKEN`.

## 6. Domaine, certificat et reverse proxy

1. **DNS** : chez ton registraire, un enregistrement `A` `fitness` → l'IP publique de ta box (ou un `CNAME` vers ton adresse DDNS Synology si ton IP change).
2. **Box** : redirection des ports 80 et 443 vers le NAS (déjà fait si ton reverse proxy sert d'autres sites). Le port 80 sert au renouvellement Let's Encrypt.
3. **Certificat** : DSM → **Panneau de configuration → Sécurité → Certificat → Ajouter → Let's Encrypt**, domaine `fitness.jeanlaura.in`.
4. **Reverse proxy** : DSM → **Panneau de configuration → Portail de connexion → Avancé → Proxy inversé → Créer** :
   - Source : HTTPS, `fitness.jeanlaura.in`, port 443, activer HSTS
   - Destination : HTTP, `localhost`, port `8380`
   - En-têtes personnalisés : `X-Forwarded-Proto` = `https`, `X-Forwarded-For` = `$proxy_add_x_forwarded_for`
5. **Certificat → Paramètres** : associer le certificat à l'entrée du proxy inversé.

## 7. Premier déploiement

1. Vérifier sur GitHub que le dernier passage du workflow **Images** sur `main` est vert (onglet Actions). Sinon : **Actions → Images → Run workflow**.
2. Sur le NAS, lancer le script à la main la première fois pour suivre ce qui se passe :

```sh
cd /volume1/docker/carnet-de-salle
bash update.sh --force
tail -n 20 logs/update.log
```

Le script récupère `docker-compose.yml` et les scripts SQL de la même version, démarre SQL Server, exécute `init-db.sql` (base + compte `carnet_app`), applique les migrations, démarre l'API et le front, puis vérifie `/api/health`. Le premier lancement télécharge SQL Server (environ 1,5 Go) : compter quelques minutes.

Ensuite, chaque push sur `main` qui touche `api/`, `web/` ou `deploy/` est en ligne au plus 5 minutes après la fin du workflow Images.

Vérifier : `https://fitness.jeanlaura.in` affiche l'appli, et `https://fitness.jeanlaura.in/api/health` répond `Healthy`.

## 8. Sauvegardes

1. DSM → **Planificateur de tâches → Créer → Tâche planifiée → Script défini par l'utilisateur** :
   - utilisateur `root`, tous les jours à 3 h ;
   - commande : `sh /volume1/docker/carnet-de-salle/backup.sh`
2. **Hyper Backup** : ajouter `/volume1/docker/carnet-de-salle/data/backups` à une sauvegarde hors du NAS (disque USB ou cloud).
3. **Restaurer** (à tester une fois) :

```sh
cd /volume1/docker/carnet-de-salle
docker compose stop api
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P '...' \
  -Q "RESTORE DATABASE [CarnetDeSalle] FROM DISK = N'/var/opt/mssql/backup/CarnetDeSalle_AAAA-MM-JJ_0300.bak' WITH REPLACE"
docker compose start api
```

## Dépannage

| Symptôme | Piste |
|---|---|
| Rien ne se déploie | `tail logs/update.log` ; présence d'un fichier `.pause` ; tâche planifiée désactivée ; dernier workflow Images en échec ; version listée dans `.failed` |
| `docker: command not found` dans le journal | la tâche ne tourne pas en `root`, ou Container Manager n'est pas installé |
| `db` ne devient jamais « healthy » | droits du dossier `data/mssql` (étape 2), RAM insuffisante, ou mot de passe `sa` trop simple : `docker compose logs db` |
| `pull access denied` sur ghcr.io | images privées sans `GITHUB_TOKEN` (ou jeton sans `read:packages`) : rendre les packages publics ou compléter le jeton |
| Déconnexion à chaque déploiement | le dossier `data/dataprotection` n'est pas monté ou pas inscriptible par l'utilisateur 1654 |
| Passkey refusée | le domaine de l'appli ne correspond pas à `APP_DOMAIN`, ou l'API ne voit pas `https` (en-têtes du reverse proxy, `UseForwardedHeaders`) |
