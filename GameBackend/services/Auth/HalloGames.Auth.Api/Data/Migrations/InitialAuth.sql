CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" text NOT NULL,
        "Expires" timestamp with time zone NOT NULL,
        "IsRevoked" boolean NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Username" text NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    CREATE TABLE "Passwords" (
        "Id" uuid NOT NULL,
        "PasswordHash" text NOT NULL,
        "AuthType" integer NOT NULL,
        "UserId" uuid NOT NULL,
        "ExternalId" text NOT NULL,
        CONSTRAINT "PK_Passwords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Passwords_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    CREATE UNIQUE INDEX "IX_Passwords_UserId" ON "Passwords" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    CREATE UNIQUE INDEX "UX_Passwords_Email" ON "Passwords" ("ExternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261005220345_InitialAuth') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261005220345_InitialAuth', '10.0.12');
    END IF;
END $EF$;
COMMIT;

