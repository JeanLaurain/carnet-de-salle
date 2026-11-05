/* Exécuté à chaque déploiement (idempotent), avec le compte sa :
     sqlcmd -v DbName="CarnetDeSalle" AppUser="carnet_app" AppPassword="..." -i init-db.sql
   Crée la base si besoin, et le compte SQL à droits limités utilisé par l'API.
   Les tables sont créées ensuite par les migrations EF Core (service « migrate »). */
SET NOCOUNT ON;

IF DB_ID(N'$(DbName)') IS NULL
BEGIN
    PRINT 'Création de la base $(DbName)';
    EXEC (N'CREATE DATABASE [$(DbName)]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppUser)')
BEGIN
    PRINT 'Création du login $(AppUser)';
    EXEC (N'CREATE LOGIN [$(AppUser)] WITH PASSWORD = N''$(AppPassword)'', CHECK_POLICY = ON, DEFAULT_DATABASE = [$(DbName)]');
END
ELSE
BEGIN
    /* Garde le mot de passe aligné sur .env */
    EXEC (N'ALTER LOGIN [$(AppUser)] WITH PASSWORD = N''$(AppPassword)''');
END
GO

USE [$(DbName)];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppUser)')
    EXEC (N'CREATE USER [$(AppUser)] FOR LOGIN [$(AppUser)]');
ALTER ROLE db_datareader ADD MEMBER [$(AppUser)];
ALTER ROLE db_datawriter ADD MEMBER [$(AppUser)];
GRANT EXECUTE TO [$(AppUser)];
GO
