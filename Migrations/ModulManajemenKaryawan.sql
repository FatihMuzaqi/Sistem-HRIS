START TRANSACTION;
CREATE TABLE `Departemen` (
    `Id` char(36) NOT NULL,
    `KodeDepartemen` varchar(20) NOT NULL,
    `NamaDepartemen` varchar(100) NOT NULL,
    `Deskripsi` varchar(255) NULL,
    `KepalaDepartemenId` char(36) NULL,
    `StatusAktif` tinyint(1) NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Departemen_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Jabatan` (
    `Id` char(36) NOT NULL,
    `NamaJabatan` varchar(100) NOT NULL,
    `DepartemenId` char(36) NOT NULL,
    `Deskripsi` varchar(255) NULL,
    `LevelJabatan` int NOT NULL,
    `StatusAktif` tinyint(1) NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Jabatan_Departemen_DepartemenId` FOREIGN KEY (`DepartemenId`) REFERENCES `Departemen` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Jabatan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Karyawan` (
    `Id` char(36) NOT NULL,
    `NomorIndukKaryawan` varchar(30) NOT NULL,
    `NamaLengkap` varchar(150) NOT NULL,
    `TempatLahir` varchar(100) NULL,
    `TanggalLahir` datetime(6) NOT NULL,
    `JenisKelamin` int NOT NULL,
    `Alamat` varchar(255) NULL,
    `NomorTelepon` varchar(30) NULL,
    `Email` varchar(100) NOT NULL,
    `TanggalMasuk` datetime(6) NOT NULL,
    `DepartemenId` char(36) NOT NULL,
    `JabatanId` char(36) NOT NULL,
    `StatusKaryawan` int NOT NULL,
    `FotoProfil` varchar(255) NULL,
    `PenggunaId` char(36) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Karyawan_Departemen_DepartemenId` FOREIGN KEY (`DepartemenId`) REFERENCES `Departemen` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Karyawan_Jabatan_JabatanId` FOREIGN KEY (`JabatanId`) REFERENCES `Jabatan` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Karyawan_Pengguna_PenggunaId` FOREIGN KEY (`PenggunaId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Karyawan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `DokumenKaryawan` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NamaDokumen` varchar(150) NOT NULL,
    `JenisDokumen` int NOT NULL,
    `NamaFile` varchar(255) NOT NULL,
    `UkuranFile` bigint NOT NULL,
    `TipeFile` varchar(100) NULL,
    `PathFile` varchar(500) NOT NULL,
    `Keterangan` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DokumenKaryawan_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DokumenKaryawan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `KontakDaruratKaryawan` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NamaLengkap` varchar(150) NOT NULL,
    `Hubungan` varchar(50) NOT NULL,
    `NomorTelepon` varchar(30) NOT NULL,
    `Alamat` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_KontakDaruratKaryawan_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_KontakDaruratKaryawan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `KontrakKerja` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NomorKontrak` varchar(50) NOT NULL,
    `JenisKontrak` int NOT NULL,
    `TanggalMulai` datetime(6) NOT NULL,
    `TanggalBerakhir` datetime(6) NULL,
    `StatusKontrak` int NOT NULL,
    `Keterangan` varchar(255) NULL,
    `DokumenUrl` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_KontrakKerja_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_KontrakKerja_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `RiwayatPekerjaan` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NamaPerusahaanSebelumnya` varchar(150) NOT NULL,
    `JabatanTerakhir` varchar(100) NOT NULL,
    `TahunMulai` int NOT NULL,
    `TahunSelesai` int NOT NULL,
    `DeskripsiTugas` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RiwayatPekerjaan_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RiwayatPekerjaan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_Departemen_KepalaDepartemenId` ON `Departemen` (`KepalaDepartemenId`);

CREATE UNIQUE INDEX `IX_Departemen_PerusahaanId_KodeDepartemen` ON `Departemen` (`PerusahaanId`, `KodeDepartemen`);

CREATE INDEX `IX_DokumenKaryawan_KaryawanId` ON `DokumenKaryawan` (`KaryawanId`);

CREATE INDEX `IX_DokumenKaryawan_PerusahaanId_KaryawanId` ON `DokumenKaryawan` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_Jabatan_DepartemenId` ON `Jabatan` (`DepartemenId`);

CREATE INDEX `IX_Jabatan_PerusahaanId_DepartemenId_NamaJabatan` ON `Jabatan` (`PerusahaanId`, `DepartemenId`, `NamaJabatan`);

CREATE INDEX `IX_Karyawan_DepartemenId` ON `Karyawan` (`DepartemenId`);

CREATE INDEX `IX_Karyawan_JabatanId` ON `Karyawan` (`JabatanId`);

CREATE INDEX `IX_Karyawan_PenggunaId` ON `Karyawan` (`PenggunaId`);

CREATE INDEX `IX_Karyawan_PerusahaanId_DepartemenId` ON `Karyawan` (`PerusahaanId`, `DepartemenId`);

CREATE INDEX `IX_Karyawan_PerusahaanId_JabatanId` ON `Karyawan` (`PerusahaanId`, `JabatanId`);

CREATE UNIQUE INDEX `IX_Karyawan_PerusahaanId_NomorIndukKaryawan` ON `Karyawan` (`PerusahaanId`, `NomorIndukKaryawan`);

CREATE INDEX `IX_Karyawan_PerusahaanId_StatusKaryawan` ON `Karyawan` (`PerusahaanId`, `StatusKaryawan`);

CREATE INDEX `IX_KontakDaruratKaryawan_KaryawanId` ON `KontakDaruratKaryawan` (`KaryawanId`);

CREATE INDEX `IX_KontakDaruratKaryawan_PerusahaanId_KaryawanId` ON `KontakDaruratKaryawan` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_KontrakKerja_KaryawanId` ON `KontrakKerja` (`KaryawanId`);

CREATE INDEX `IX_KontrakKerja_PerusahaanId_KaryawanId` ON `KontrakKerja` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_RiwayatPekerjaan_KaryawanId` ON `RiwayatPekerjaan` (`KaryawanId`);

CREATE INDEX `IX_RiwayatPekerjaan_PerusahaanId_KaryawanId` ON `RiwayatPekerjaan` (`PerusahaanId`, `KaryawanId`);

ALTER TABLE `Departemen` ADD CONSTRAINT `FK_Departemen_Karyawan_KepalaDepartemenId` FOREIGN KEY (`KepalaDepartemenId`) REFERENCES `Karyawan` (`Id`) ON DELETE SET NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261115102234_ModulManajemenKaryawan', '10.0.15');

COMMIT;

