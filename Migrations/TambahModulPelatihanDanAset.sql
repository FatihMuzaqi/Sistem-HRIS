START TRANSACTION;
CREATE TABLE `AsetPerusahaan` (
    `Id` char(36) NOT NULL,
    `NomorAset` varchar(50) NOT NULL,
    `NamaAset` varchar(150) NOT NULL,
    `Kategori` int NOT NULL,
    `MerkModel` varchar(100) NULL,
    `NomorSeri` varchar(100) NULL,
    `TanggalPembelian` datetime(6) NOT NULL,
    `NilaiPerolehan` decimal(18,2) NOT NULL,
    `Kondisi` int NOT NULL,
    `Status` int NOT NULL,
    `LokasiPenyimpanan` varchar(100) NOT NULL,
    `DeskripsiSpesifikasi` longtext NULL,
    `PemegangSaatIniId` char(36) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AsetPerusahaan_Karyawan_PemegangSaatIniId` FOREIGN KEY (`PemegangSaatIniId`) REFERENCES `Karyawan` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_AsetPerusahaan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ProgramPelatihan` (
    `Id` char(36) NOT NULL,
    `KodeProgram` varchar(50) NOT NULL,
    `NamaProgram` varchar(150) NOT NULL,
    `Deskripsi` longtext NOT NULL,
    `Penyelenggara` varchar(150) NOT NULL,
    `Instruktur` varchar(150) NULL,
    `Metode` int NOT NULL,
    `LokasiAtauTautan` varchar(255) NOT NULL,
    `TanggalMulai` datetime(6) NOT NULL,
    `TanggalSelesai` datetime(6) NOT NULL,
    `TotalJam` decimal(5,2) NOT NULL,
    `BiayaPerPeserta` decimal(18,2) NOT NULL,
    `MaksimalPeserta` int NOT NULL,
    `Status` int NOT NULL,
    `DepartemenId` char(36) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ProgramPelatihan_Departemen_DepartemenId` FOREIGN KEY (`DepartemenId`) REFERENCES `Departemen` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_ProgramPelatihan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PenyerahanAset` (
    `Id` char(36) NOT NULL,
    `AsetPerusahaanId` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NomorDokumen` varchar(50) NOT NULL,
    `TanggalPenyerahan` datetime(6) NOT NULL,
    `TanggalTargetPengembalian` datetime(6) NULL,
    `TanggalRealisasiPengembalian` datetime(6) NULL,
    `KondisiSaatPenyerahan` int NOT NULL,
    `KondisiSaatPengembalian` int NULL,
    `StatusPeminjaman` varchar(50) NOT NULL,
    `KelengkapanAset` varchar(255) NULL,
    `CatatanPenyerahan` varchar(500) NULL,
    `CatatanPengembalian` varchar(500) NULL,
    `DiserahkanOlehId` char(36) NOT NULL,
    `DiterimaKembaliOlehId` char(36) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PenyerahanAset_AsetPerusahaan_AsetPerusahaanId` FOREIGN KEY (`AsetPerusahaanId`) REFERENCES `AsetPerusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PenyerahanAset_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenyerahanAset_Pengguna_DiserahkanOlehId` FOREIGN KEY (`DiserahkanOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenyerahanAset_Pengguna_DiterimaKembaliOlehId` FOREIGN KEY (`DiterimaKembaliOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_PenyerahanAset_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `RiwayatAset` (
    `Id` char(36) NOT NULL,
    `AsetPerusahaanId` char(36) NOT NULL,
    `TipeAktivitas` int NOT NULL,
    `TanggalAktivitas` datetime(6) NOT NULL,
    `KaryawanTerkaitId` char(36) NULL,
    `NamaPetugas` varchar(150) NOT NULL,
    `KondisiSebelumnya` int NOT NULL,
    `KondisiTerbaru` int NOT NULL,
    `Deskripsi` longtext NOT NULL,
    `BiayaPerbaikan` decimal(18,2) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RiwayatAset_AsetPerusahaan_AsetPerusahaanId` FOREIGN KEY (`AsetPerusahaanId`) REFERENCES `AsetPerusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RiwayatAset_Karyawan_KaryawanTerkaitId` FOREIGN KEY (`KaryawanTerkaitId`) REFERENCES `Karyawan` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_RiwayatAset_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PesertaPelatihan` (
    `Id` char(36) NOT NULL,
    `ProgramPelatihanId` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `StatusKeikutsertaan` int NOT NULL,
    `NilaiAkhir` decimal(5,2) NULL,
    `Catatan` varchar(500) NULL,
    `TanggalDidaftarkan` datetime(6) NOT NULL,
    `TanggalSelesai` datetime(6) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PesertaPelatihan_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PesertaPelatihan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PesertaPelatihan_ProgramPelatihan_ProgramPelatihanId` FOREIGN KEY (`ProgramPelatihanId`) REFERENCES `ProgramPelatihan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SertifikatPelatihan` (
    `Id` char(36) NOT NULL,
    `PesertaPelatihanId` char(36) NOT NULL,
    `NomorSertifikat` varchar(100) NOT NULL,
    `JudulSertifikat` varchar(150) NOT NULL,
    `TanggalTerbit` datetime(6) NOT NULL,
    `TanggalKedaluwarsa` datetime(6) NULL,
    `PenerbitSertifikat` varchar(150) NOT NULL,
    `KodeVerifikasi` varchar(50) NOT NULL,
    `FileSertifikat` varchar(255) NULL,
    `Catatan` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SertifikatPelatihan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SertifikatPelatihan_PesertaPelatihan_PesertaPelatihanId` FOREIGN KEY (`PesertaPelatihanId`) REFERENCES `PesertaPelatihan` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_AsetPerusahaan_PemegangSaatIniId` ON `AsetPerusahaan` (`PemegangSaatIniId`);

CREATE INDEX `IX_AsetPerusahaan_PerusahaanId_Kategori` ON `AsetPerusahaan` (`PerusahaanId`, `Kategori`);

CREATE INDEX `IX_AsetPerusahaan_PerusahaanId_Kondisi` ON `AsetPerusahaan` (`PerusahaanId`, `Kondisi`);

CREATE UNIQUE INDEX `IX_AsetPerusahaan_PerusahaanId_NomorAset` ON `AsetPerusahaan` (`PerusahaanId`, `NomorAset`);

CREATE INDEX `IX_AsetPerusahaan_PerusahaanId_Status` ON `AsetPerusahaan` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_PenyerahanAset_AsetPerusahaanId` ON `PenyerahanAset` (`AsetPerusahaanId`);

CREATE INDEX `IX_PenyerahanAset_DiserahkanOlehId` ON `PenyerahanAset` (`DiserahkanOlehId`);

CREATE INDEX `IX_PenyerahanAset_DiterimaKembaliOlehId` ON `PenyerahanAset` (`DiterimaKembaliOlehId`);

CREATE INDEX `IX_PenyerahanAset_KaryawanId` ON `PenyerahanAset` (`KaryawanId`);

CREATE INDEX `IX_PenyerahanAset_PerusahaanId_AsetPerusahaanId` ON `PenyerahanAset` (`PerusahaanId`, `AsetPerusahaanId`);

CREATE INDEX `IX_PenyerahanAset_PerusahaanId_KaryawanId` ON `PenyerahanAset` (`PerusahaanId`, `KaryawanId`);

CREATE UNIQUE INDEX `IX_PenyerahanAset_PerusahaanId_NomorDokumen` ON `PenyerahanAset` (`PerusahaanId`, `NomorDokumen`);

CREATE INDEX `IX_PesertaPelatihan_KaryawanId` ON `PesertaPelatihan` (`KaryawanId`);

CREATE INDEX `IX_PesertaPelatihan_PerusahaanId_KaryawanId` ON `PesertaPelatihan` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_PesertaPelatihan_PerusahaanId_StatusKeikutsertaan` ON `PesertaPelatihan` (`PerusahaanId`, `StatusKeikutsertaan`);

CREATE UNIQUE INDEX `IX_PesertaPelatihan_ProgramPelatihanId_KaryawanId` ON `PesertaPelatihan` (`ProgramPelatihanId`, `KaryawanId`);

CREATE INDEX `IX_ProgramPelatihan_DepartemenId` ON `ProgramPelatihan` (`DepartemenId`);

CREATE UNIQUE INDEX `IX_ProgramPelatihan_PerusahaanId_KodeProgram` ON `ProgramPelatihan` (`PerusahaanId`, `KodeProgram`);

CREATE INDEX `IX_ProgramPelatihan_PerusahaanId_Status` ON `ProgramPelatihan` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_ProgramPelatihan_PerusahaanId_TanggalMulai` ON `ProgramPelatihan` (`PerusahaanId`, `TanggalMulai`);

CREATE INDEX `IX_RiwayatAset_AsetPerusahaanId` ON `RiwayatAset` (`AsetPerusahaanId`);

CREATE INDEX `IX_RiwayatAset_KaryawanTerkaitId` ON `RiwayatAset` (`KaryawanTerkaitId`);

CREATE INDEX `IX_RiwayatAset_PerusahaanId_AsetPerusahaanId` ON `RiwayatAset` (`PerusahaanId`, `AsetPerusahaanId`);

CREATE INDEX `IX_RiwayatAset_PerusahaanId_TanggalAktivitas` ON `RiwayatAset` (`PerusahaanId`, `TanggalAktivitas`);

CREATE UNIQUE INDEX `IX_SertifikatPelatihan_PerusahaanId_KodeVerifikasi` ON `SertifikatPelatihan` (`PerusahaanId`, `KodeVerifikasi`);

CREATE UNIQUE INDEX `IX_SertifikatPelatihan_PerusahaanId_NomorSertifikat` ON `SertifikatPelatihan` (`PerusahaanId`, `NomorSertifikat`);

CREATE UNIQUE INDEX `IX_SertifikatPelatihan_PesertaPelatihanId` ON `SertifikatPelatihan` (`PesertaPelatihanId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261126145718_TambahModulPelatihanDanAset', '10.0.15');

COMMIT;

