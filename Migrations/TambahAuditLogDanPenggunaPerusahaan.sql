START TRANSACTION;
CREATE TABLE `AuditLog` (
    `Id` char(36) NOT NULL,
    `PerusahaanId` char(36) NULL,
    `PenggunaId` char(36) NULL,
    `NamaPengguna` varchar(150) NULL,
    `Aksi` varchar(50) NOT NULL,
    `NamaEntitas` varchar(100) NULL,
    `KunciEntitas` varchar(100) NULL,
    `DataLama` longtext NULL,
    `DataBaru` longtext NULL,
    `AlamatIP` varchar(50) NULL,
    `UserAgent` varchar(255) NULL,
    `Waktu` datetime NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AuditLog_Pengguna_PenggunaId` FOREIGN KEY (`PenggunaId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_AuditLog_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE SET NULL
);

CREATE TABLE `PenggunaPerusahaan` (
    `Id` char(36) NOT NULL,
    `PenggunaId` char(36) NOT NULL,
    `PerusahaanId` char(36) NOT NULL,
    `PeranDiPerusahaan` varchar(50) NULL,
    `IsDefault` tinyint(1) NOT NULL,
    `StatusAktif` tinyint(1) NOT NULL,
    `JoinedAt` datetime NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PenggunaPerusahaan_Pengguna_PenggunaId` FOREIGN KEY (`PenggunaId`) REFERENCES `Pengguna` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PenggunaPerusahaan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_AuditLog_Aksi` ON `AuditLog` (`Aksi`);

CREATE INDEX `IX_AuditLog_PenggunaId` ON `AuditLog` (`PenggunaId`);

CREATE INDEX `IX_AuditLog_PerusahaanId` ON `AuditLog` (`PerusahaanId`);

CREATE INDEX `IX_AuditLog_Waktu` ON `AuditLog` (`Waktu`);

CREATE UNIQUE INDEX `IX_PenggunaPerusahaan_PenggunaId_PerusahaanId` ON `PenggunaPerusahaan` (`PenggunaId`, `PerusahaanId`);

CREATE INDEX `IX_PenggunaPerusahaan_PerusahaanId` ON `PenggunaPerusahaan` (`PerusahaanId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261115083704_TambahAuditLogDanPenggunaPerusahaan', '10.0.15');

COMMIT;

