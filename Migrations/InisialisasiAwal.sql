CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `Peran` (
    `Id` char(36) NOT NULL,
    `Deskripsi` varchar(255) NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` longtext NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Perusahaan` (
    `Id` char(36) NOT NULL,
    `KodePerusahaan` varchar(20) NOT NULL,
    `NamaPerusahaan` varchar(150) NOT NULL,
    `Email` varchar(100) NULL,
    `NomorTelepon` varchar(30) NULL,
    `Alamat` varchar(255) NULL,
    `NPWP` varchar(50) NULL,
    `LogoUrl` varchar(255) NULL,
    `Website` varchar(100) NULL,
    `StatusAktif` tinyint(1) NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `PeranKlaim` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` char(36) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PeranKlaim_Peran_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Peran` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Pengguna` (
    `Id` char(36) NOT NULL,
    `NamaLengkap` varchar(150) NOT NULL,
    `PerusahaanId` char(36) NULL,
    `AvatarUrl` varchar(255) NULL,
    `StatusAktif` tinyint(1) NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext NULL,
    `SecurityStamp` longtext NULL,
    `ConcurrencyStamp` longtext NULL,
    `PhoneNumber` longtext NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pengguna_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`)
);

CREATE TABLE `PenggunaKlaim` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` char(36) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PenggunaKlaim_Pengguna_UserId` FOREIGN KEY (`UserId`) REFERENCES `Pengguna` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PenggunaLogin` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext NULL,
    `UserId` char(36) NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_PenggunaLogin_Pengguna_UserId` FOREIGN KEY (`UserId`) REFERENCES `Pengguna` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PenggunaPeran` (
    `UserId` char(36) NOT NULL,
    `RoleId` char(36) NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_PenggunaPeran_Pengguna_UserId` FOREIGN KEY (`UserId`) REFERENCES `Pengguna` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PenggunaPeran_Peran_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Peran` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PenggunaToken` (
    `UserId` char(36) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_PenggunaToken_Pengguna_UserId` FOREIGN KEY (`UserId`) REFERENCES `Pengguna` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `EmailIndex` ON `Pengguna` (`NormalizedEmail`);

CREATE INDEX `IX_Pengguna_Email` ON `Pengguna` (`Email`);

CREATE INDEX `IX_Pengguna_PerusahaanId` ON `Pengguna` (`PerusahaanId`);

CREATE UNIQUE INDEX `UserNameIndex` ON `Pengguna` (`NormalizedUserName`);

CREATE INDEX `IX_PenggunaKlaim_UserId` ON `PenggunaKlaim` (`UserId`);

CREATE INDEX `IX_PenggunaLogin_UserId` ON `PenggunaLogin` (`UserId`);

CREATE INDEX `IX_PenggunaPeran_RoleId` ON `PenggunaPeran` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `Peran` (`NormalizedName`);

CREATE INDEX `IX_PeranKlaim_RoleId` ON `PeranKlaim` (`RoleId`);

CREATE UNIQUE INDEX `IX_Perusahaan_KodePerusahaan` ON `Perusahaan` (`KodePerusahaan`);

CREATE INDEX `IX_Perusahaan_StatusAktif` ON `Perusahaan` (`StatusAktif`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261115082106_InisialisasiAwal', '10.0.15');

COMMIT;

