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
         /volume1/docker/carnet-de-salle/data/dataprotection \
         /volume1/docker/carnet-de-salle/runner
# SQL Server tourne avec l'utilisateur 10001, l'API .NET avec l'utilisateur 1654
chown -R 10001:0 /volume1/docker/carnet-de-salle/data/mssql /volume1/docker/carnet-de-salle/data/backups
chown -R 1654:1654 /volume1/docker/carnet-de-salle/data/dataprotection
```

## 3. Les secrets (`.env`)

Copier [`deploy/.env.example`](../deploy/.env.example) dans `/volume1/docker/carnet-de-salle/.env` et remplir les valeurs : domaine, mots de passe SQL, emails autorisés, identifiants Google et Apple quand tu les auras. Ce fichier ne quitte jamais le NAS.

```sh
chmod 600 /volume1/docker/carnet-de-salle/.env
```

## 4. Le runner GitHub Actions

1. Sur GitHub : **Settings (ton profil) → Developer settings → Personal access tokens → Fine-grained tokens → Generate new token**.
   - Repository access : *Only select repositories* → `carnet-de-salle`
   - Permissions → Repository → **Administration : Read and write**
2. Sur le NAS :

```sh
cd /volume1/docker/carnet-de-salle/runner
# copier ici deploy/runner/docker-compose.yml, puis :
echo "GITHUB_PAT=github_pat_..." > .env && chmod 600 .env
docker compose up -d
docker compose logs -f   # attendre « Listening for Jobs »
```

3. Sur GitHub : **dépôt → Settings → Actions → Runners** : `nas-synology` doit apparaître « Idle ».

## 5. Les variables GitHub

Dépôt → **Settings → Secrets and variables → Actions → Variables** :

| Variable | Valeur |
|---|---|
| `DEPLOY_DIR` | `/volume1/docker/carnet-de-salle` |
| `APP_DOMAIN` | `fitness.jeanlaura.in` (ou `salle.jeanlaura.in`) |
| `DEPLOY_ENABLED` | `true` (une fois les étapes 1 à 4 faites) |

Puis **Settings → Environments → New environment** : `production` (tu peux y ajouter une validation manuelle avant chaque déploiement).

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

GitHub → **Actions → Deploy → Run workflow** (sans tag). Le workflow :

1. relance les tests de l'API et du front ;
2. construit les images et les publie sur `ghcr.io/jeanlaurain/carnet-de-salle-api` et `-web` ;
3. sur le NAS : copie `docker-compose.yml`, démarre SQL Server, exécute `init-db.sql` (base + compte `carnet_app`), applique les migrations, démarre l'API et le front ;
4. vérifie que `/api/health` répond.

Ensuite, chaque push sur `main` qui touche `api/`, `web/` ou `deploy/` redéploie tout seul.

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
| Le job Deploy reste « Queued » | le runner est arrêté : `docker compose ps` dans `runner/`, ou `DEPLOY_ENABLED` n'est pas à `true` |
| `db` ne devient jamais « healthy » | droits du dossier `data/mssql` (étape 2), RAM insuffisante, ou mot de passe `sa` trop simple : `docker compose logs db` |
| `pull access denied` sur ghcr.io | le job n'a pas pu se connecter au registre : relancer ; vérifier que les packages sont liés au dépôt |
| Déconnexion à chaque déploiement | le dossier `data/dataprotection` n'est pas monté ou pas inscriptible par l'utilisateur 1654 |
| Passkey refusée | le domaine de l'appli ne correspond pas à `APP_DOMAIN`, ou l'API ne voit pas `https` (en-têtes du reverse proxy, `UseForwardedHeaders`) |
