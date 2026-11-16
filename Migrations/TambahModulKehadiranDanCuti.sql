START TRANSACTION;
CREATE TABLE `JenisCuti` (
    `Id` char(36) NOT NULL,
    `NamaJenisCuti` varchar(100) NOT NULL,
    `KodeJenisCuti` varchar(20) NOT NULL,
    `JatahHariPerTahun` int NOT NULL,
    `PotongCutiTahunan` tinyint(1) NOT NULL,
    `WajibLampiran` tinyint(1) NOT NULL,
    `StatusAktif` tinyint(1) NOT NULL,
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
    CONSTRAINT `FK_JenisCuti_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PengajuanLembur` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NomorPengajuan` varchar(50) NOT NULL,
    `TanggalLembur` datetime(6) NOT NULL,
    `JamMulai` time(6) NOT NULL,
    `JamSelesai` time(6) NOT NULL,
    `TotalJam` decimal(18,2) NOT NULL,
    `AlasanLembur` varchar(500) NOT NULL,
    `Status` int NOT NULL,
    `DisetujuiOlehId` char(36) NULL,
    `WaktuPersetujuan` datetime(6) NULL,
    `CatatanPersetujuan` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PengajuanLembur_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PengajuanLembur_Pengguna_DisetujuiOlehId` FOREIGN KEY (`DisetujuiOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_PengajuanLembur_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `ShiftKerja` (
    `Id` char(36) NOT NULL,
    `NamaShift` varchar(50) NOT NULL,
    `KodeShift` varchar(20) NOT NULL,
    `JamMasuk` time(6) NOT NULL,
    `JamPulang` time(6) NOT NULL,
    `ToleransiKeterlambatanMenit` int NOT NULL,
    `Keterangan` varchar(255) NULL,
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
    CONSTRAINT `FK_ShiftKerja_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PengajuanCuti` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `JenisCutiId` char(36) NOT NULL,
    `NomorPengajuan` varchar(50) NOT NULL,
    `TanggalMulai` datetime(6) NOT NULL,
    `TanggalSelesai` datetime(6) NOT NULL,
    `JumlahHari` int NOT NULL,
    `Alasan` varchar(500) NOT NULL,
    `AlamatSelamaCuti` varchar(255) NULL,
    `NomorTeleponDarurat` varchar(50) NULL,
    `NamaDokumenPendukung` varchar(255) NULL,
    `JalurDokumenPendukung` varchar(500) NULL,
    `Status` int NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PengajuanCuti_JenisCuti_JenisCutiId` FOREIGN KEY (`JenisCutiId`) REFERENCES `JenisCuti` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PengajuanCuti_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PengajuanCuti_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SaldoCuti` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `JenisCutiId` char(36) NOT NULL,
    `PeriodeTahun` int NOT NULL,
    `JumlahHakCuti` int NOT NULL,
    `CutiTerpakai` int NOT NULL,
    `SisaCuti` int NOT NULL,
    `Catatan` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SaldoCuti_JenisCuti_JenisCutiId` FOREIGN KEY (`JenisCutiId`) REFERENCES `JenisCuti` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SaldoCuti_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SaldoCuti_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `JadwalKerja` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `ShiftKerjaId` char(36) NOT NULL,
    `Tanggal` datetime(6) NOT NULL,
    `StatusHariLibur` tinyint(1) NOT NULL,
    `Catatan` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_JadwalKerja_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_JadwalKerja_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_JadwalKerja_ShiftKerja_ShiftKerjaId` FOREIGN KEY (`ShiftKerjaId`) REFERENCES `ShiftKerja` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `Kehadiran` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `ShiftKerjaId` char(36) NULL,
    `Tanggal` datetime(6) NOT NULL,
    `WaktuMasuk` datetime(6) NULL,
    `WaktuPulang` datetime(6) NULL,
    `Status` int NOT NULL,
    `MenitKeterlambatan` int NOT NULL,
    `MenitPulangCepat` int NOT NULL,
    `TotalJamKerja` decimal(18,2) NOT NULL,
    `CatatanMasuk` varchar(255) NULL,
    `CatatanPulang` varchar(255) NULL,
    `LokasiMasuk` varchar(255) NULL,
    `LokasiPulang` varchar(255) NULL,
    `FotoMasuk` varchar(255) NULL,
    `FotoPulang` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Kehadiran_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Kehadiran_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Kehadiran_ShiftKerja_ShiftKerjaId` FOREIGN KEY (`ShiftKerjaId`) REFERENCES `ShiftKerja` (`Id`) ON DELETE SET NULL
);

CREATE TABLE `PersetujuanCuti` (
    `Id` char(36) NOT NULL,
    `PengajuanCutiId` char(36) NOT NULL,
    `Tahap` int NOT NULL,
    `PenyetujuId` char(36) NOT NULL,
    `Keputusan` int NOT NULL,
    `Catatan` varchar(500) NULL,
    `WaktuPersetujuan` datetime(6) NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PersetujuanCuti_PengajuanCuti_PengajuanCutiId` FOREIGN KEY (`PengajuanCutiId`) REFERENCES `PengajuanCuti` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PersetujuanCuti_Pengguna_PenyetujuId` FOREIGN KEY (`PenyetujuId`) REFERENCES `Pengguna` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PersetujuanCuti_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_JadwalKerja_KaryawanId` ON `JadwalKerja` (`KaryawanId`);

CREATE INDEX `IX_JadwalKerja_PerusahaanId_KaryawanId_Tanggal` ON `JadwalKerja` (`PerusahaanId`, `KaryawanId`, `Tanggal`);

CREATE INDEX `IX_JadwalKerja_ShiftKerjaId` ON `JadwalKerja` (`ShiftKerjaId`);

CREATE UNIQUE INDEX `IX_JenisCuti_PerusahaanId_KodeJenisCuti` ON `JenisCuti` (`PerusahaanId`, `KodeJenisCuti`);

CREATE INDEX `IX_Kehadiran_KaryawanId` ON `Kehadiran` (`KaryawanId`);

CREATE UNIQUE INDEX `IX_Kehadiran_PerusahaanId_KaryawanId_Tanggal` ON `Kehadiran` (`PerusahaanId`, `KaryawanId`, `Tanggal`);

CREATE INDEX `IX_Kehadiran_PerusahaanId_Status` ON `Kehadiran` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_Kehadiran_PerusahaanId_Tanggal` ON `Kehadiran` (`PerusahaanId`, `Tanggal`);

CREATE INDEX `IX_Kehadiran_ShiftKerjaId` ON `Kehadiran` (`ShiftKerjaId`);

CREATE INDEX `IX_PengajuanCuti_JenisCutiId` ON `PengajuanCuti` (`JenisCutiId`);

CREATE INDEX `IX_PengajuanCuti_KaryawanId` ON `PengajuanCuti` (`KaryawanId`);

CREATE INDEX `IX_PengajuanCuti_PerusahaanId_KaryawanId` ON `PengajuanCuti` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_PengajuanCuti_PerusahaanId_Status` ON `PengajuanCuti` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_PengajuanLembur_DisetujuiOlehId` ON `PengajuanLembur` (`DisetujuiOlehId`);

CREATE INDEX `IX_PengajuanLembur_KaryawanId` ON `PengajuanLembur` (`KaryawanId`);

CREATE INDEX `IX_PengajuanLembur_PerusahaanId_KaryawanId_TanggalLembur` ON `PengajuanLembur` (`PerusahaanId`, `KaryawanId`, `TanggalLembur`);

CREATE INDEX `IX_PengajuanLembur_PerusahaanId_Status` ON `PengajuanLembur` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_PersetujuanCuti_PengajuanCutiId` ON `PersetujuanCuti` (`PengajuanCutiId`);

CREATE INDEX `IX_PersetujuanCuti_PenyetujuId` ON `PersetujuanCuti` (`PenyetujuId`);

CREATE INDEX `IX_PersetujuanCuti_PerusahaanId_PengajuanCutiId` ON `PersetujuanCuti` (`PerusahaanId`, `PengajuanCutiId`);

CREATE INDEX `IX_SaldoCuti_JenisCutiId` ON `SaldoCuti` (`JenisCutiId`);

CREATE INDEX `IX_SaldoCuti_KaryawanId` ON `SaldoCuti` (`KaryawanId`);

CREATE UNIQUE INDEX `IX_SaldoCuti_PerusahaanId_KaryawanId_JenisCutiId_PeriodeTahun` ON `SaldoCuti` (`PerusahaanId`, `KaryawanId`, `JenisCutiId`, `PeriodeTahun`);

CREATE UNIQUE INDEX `IX_ShiftKerja_PerusahaanId_KodeShift` ON `ShiftKerja` (`PerusahaanId`, `KodeShift`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261116070717_TambahModulKehadiranDanCuti', '10.0.15');

COMMIT;

