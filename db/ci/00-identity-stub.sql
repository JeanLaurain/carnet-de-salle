/* CI uniquement : table d'identité minimale pour valider db/schema.sql.
   En vrai, dbo.AspNetUsers est créée par la migration ASP.NET Core Identity. */
CREATE TABLE dbo.AspNetUsers (
    Id       uniqueidentifier NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    UserName nvarchar(256)    NULL
);
GO
