START TRANSACTION;
ALTER TABLE `Karyawan` ADD `NamaBank` varchar(50) NULL;

ALTER TABLE `Karyawan` ADD `NomorBpjsKesehatan` varchar(50) NULL;

ALTER TABLE `Karyawan` ADD `NomorBpjsKetenagakerjaan` varchar(50) NULL;

ALTER TABLE `Karyawan` ADD `NomorNPWP` varchar(50) NULL;

ALTER TABLE `Karyawan` ADD `NomorRekeningBank` varchar(50) NULL;

ALTER TABLE `Karyawan` ADD `StatusPTKP` int NOT NULL DEFAULT 0;

CREATE TABLE `KebijakanPenggajianPerusahaan` (
    `Id` char(36) NOT NULL,
    `HitungLemburOtomatis` tinyint(1) NOT NULL,
    `TarifLemburPerJamFlat` decimal(18,2) NOT NULL,
    `TerapkanPotonganKeterlambatan` tinyint(1) NOT NULL,
    `ToleransiKeterlambatanGratisMenit` int NOT NULL,
    `DendaKeterlambatanPerMenit` decimal(18,2) NOT NULL,
    `TerapkanBpjsKesehatan` tinyint(1) NOT NULL,
    `PersenBpjsKesehatanPerusahaan` decimal(5,2) NOT NULL,
    `PersenBpjsKesehatanPekerja` decimal(5,2) NOT NULL,
    `BatasMaksimalUpahBpjsKesehatan` decimal(18,2) NOT NULL,
    `TerapkanBpjsKetenagakerjaan` tinyint(1) NOT NULL,
    `PersenBpjsJhtPerusahaan` decimal(5,2) NOT NULL,
    `PersenBpjsJhtPekerja` decimal(5,2) NOT NULL,
    `PersenBpjsJpPerusahaan` decimal(5,2) NOT NULL,
    `PersenBpjsJpPekerja` decimal(5,2) NOT NULL,
    `BatasMaksimalUpahBpjsJp` decimal(18,2) NOT NULL,
    `PersenBpjsJkkPerusahaan` decimal(5,2) NOT NULL,
    `PersenBpjsJkmPerusahaan` decimal(5,2) NOT NULL,
    `TerapkanPph21` tinyint(1) NOT NULL,
    `MetodePph21` int NOT NULL,
    `TanggalCutoffAwal` int NOT NULL,
    `TanggalCutoffAkhir` int NOT NULL,
    `TanggalPembayaranGaji` int NOT NULL,
    `CatatanSlipGajiDefault` varchar(255) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_KebijakanPenggajianPerusahaan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `KomponenGaji` (
    `Id` char(36) NOT NULL,
    `KodeKomponen` varchar(50) NOT NULL,
    `NamaKomponen` varchar(100) NOT NULL,
    `Jenis` int NOT NULL,
    `Sifat` int NOT NULL,
    `Kategori` int NOT NULL,
    `NilaiBawaan` decimal(18,2) NOT NULL,
    `KenaPajak` tinyint(1) NOT NULL,
    `MasukKalkulasiBpjs` tinyint(1) NOT NULL,
    `DihitungOtomatisOlehSistem` tinyint(1) NOT NULL,
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
    CONSTRAINT `FK_KomponenGaji_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PeriodePenggajian` (
    `Id` char(36) NOT NULL,
    `NamaPeriode` varchar(100) NOT NULL,
    `Bulan` int NOT NULL,
    `Tahun` int NOT NULL,
    `TanggalMulai` datetime(6) NOT NULL,
    `TanggalSelesai` datetime(6) NOT NULL,
    `TanggalPembayaran` datetime(6) NOT NULL,
    `Status` int NOT NULL,
    `TotalKaryawan` int NOT NULL,
    `TotalPengeluaranGaji` decimal(18,2) NOT NULL,
    `Catatan` varchar(500) NULL,
    `DiajukanOlehId` char(36) NULL,
    `WaktuDiajukan` datetime NULL,
    `DisetujuiOlehId` char(36) NULL,
    `WaktuDisetujui` datetime NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_PeriodePenggajian_Pengguna_DiajukanOlehId` FOREIGN KEY (`DiajukanOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_PeriodePenggajian_Pengguna_DisetujuiOlehId` FOREIGN KEY (`DisetujuiOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_PeriodePenggajian_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `GajiKaryawan` (
    `Id` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `KomponenGajiId` char(36) NOT NULL,
    `Nominal` decimal(18,2) NOT NULL,
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
    CONSTRAINT `FK_GajiKaryawan_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_GajiKaryawan_KomponenGaji_KomponenGajiId` FOREIGN KEY (`KomponenGajiId`) REFERENCES `KomponenGaji` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_GajiKaryawan_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Penggajian` (
    `Id` char(36) NOT NULL,
    `PeriodePenggajianId` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `NomorSlipGaji` varchar(50) NOT NULL,
    `StatusPTKP` int NOT NULL,
    `MetodePajak` int NOT NULL,
    `HariKerjaEfektif` int NOT NULL,
    `JumlahHadir` int NOT NULL,
    `JumlahTerlambat` int NOT NULL,
    `TotalMenitTerlambat` int NOT NULL,
    `JumlahJamLembur` decimal(4,2) NOT NULL,
    `GajiPokok` decimal(18,2) NOT NULL,
    `TotalTunjanganTetap` decimal(18,2) NOT NULL,
    `TotalTunjanganTidakTetap` decimal(18,2) NOT NULL,
    `TotalUangLembur` decimal(18,2) NOT NULL,
    `TotalPendapatanKotor` decimal(18,2) NOT NULL,
    `TotalPotonganBpjs` decimal(18,2) NOT NULL,
    `TotalPotonganPph21` decimal(18,2) NOT NULL,
    `TotalPotonganKeterlambatan` decimal(18,2) NOT NULL,
    `TotalPotonganLainnya` decimal(18,2) NOT NULL,
    `TotalPotongan` decimal(18,2) NOT NULL,
    `GajiBersih` decimal(18,2) NOT NULL,
    `TotalBpjsDitanggungPerusahaan` decimal(18,2) NOT NULL,
    `Status` int NOT NULL,
    `ApakahTerkunci` tinyint(1) NOT NULL,
    `Catatan` varchar(500) NULL,
    `DisetujuiOlehId` char(36) NULL,
    `WaktuDisetujui` datetime NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Penggajian_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Penggajian_Pengguna_DisetujuiOlehId` FOREIGN KEY (`DisetujuiOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Penggajian_PeriodePenggajian_PeriodePenggajianId` FOREIGN KEY (`PeriodePenggajianId`) REFERENCES `PeriodePenggajian` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Penggajian_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `DetailPenggajian` (
    `Id` char(36) NOT NULL,
    `PenggajianId` char(36) NOT NULL,
    `KomponenGajiId` char(36) NULL,
    `KodeKomponen` varchar(50) NOT NULL,
    `NamaKomponen` varchar(100) NOT NULL,
    `Jenis` int NOT NULL,
    `Sifat` int NOT NULL,
    `Kategori` int NOT NULL,
    `Nominal` decimal(18,2) NOT NULL,
    `NominalDitanggungPerusahaan` decimal(18,2) NOT NULL,
    `Keterangan` varchar(255) NULL,
    `Urutan` int NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DetailPenggajian_KomponenGaji_KomponenGajiId` FOREIGN KEY (`KomponenGajiId`) REFERENCES `KomponenGaji` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_DetailPenggajian_Penggajian_PenggajianId` FOREIGN KEY (`PenggajianId`) REFERENCES `Penggajian` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DetailPenggajian_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `SlipGaji` (
    `Id` char(36) NOT NULL,
    `PenggajianId` char(36) NOT NULL,
    `NomorSlipGaji` varchar(50) NOT NULL,
    `KodeVerifikasi` varchar(64) NOT NULL,
    `WaktuDiterbitkan` datetime NOT NULL,
    `DiterbitkanOlehId` char(36) NULL,
    `SudahDilihatKaryawan` tinyint(1) NOT NULL,
    `WaktuDilihatKaryawan` datetime NULL,
    `CatatanKaki` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_SlipGaji_Penggajian_PenggajianId` FOREIGN KEY (`PenggajianId`) REFERENCES `Penggajian` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SlipGaji_Pengguna_DiterbitkanOlehId` FOREIGN KEY (`DiterbitkanOlehId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_SlipGaji_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE INDEX `IX_DetailPenggajian_KomponenGajiId` ON `DetailPenggajian` (`KomponenGajiId`);

CREATE INDEX `IX_DetailPenggajian_PenggajianId_Jenis` ON `DetailPenggajian` (`PenggajianId`, `Jenis`);

CREATE INDEX `IX_DetailPenggajian_PerusahaanId` ON `DetailPenggajian` (`PerusahaanId`);

CREATE INDEX `IX_GajiKaryawan_KaryawanId` ON `GajiKaryawan` (`KaryawanId`);

CREATE INDEX `IX_GajiKaryawan_KomponenGajiId` ON `GajiKaryawan` (`KomponenGajiId`);

CREATE UNIQUE INDEX `IX_GajiKaryawan_PerusahaanId_KaryawanId_KomponenGajiId` ON `GajiKaryawan` (`PerusahaanId`, `KaryawanId`, `KomponenGajiId`);

CREATE UNIQUE INDEX `IX_KebijakanPenggajianPerusahaan_PerusahaanId` ON `KebijakanPenggajianPerusahaan` (`PerusahaanId`);

CREATE INDEX `IX_KomponenGaji_PerusahaanId_Jenis` ON `KomponenGaji` (`PerusahaanId`, `Jenis`);

CREATE UNIQUE INDEX `IX_KomponenGaji_PerusahaanId_KodeKomponen` ON `KomponenGaji` (`PerusahaanId`, `KodeKomponen`);

CREATE INDEX `IX_Penggajian_DisetujuiOlehId` ON `Penggajian` (`DisetujuiOlehId`);

CREATE INDEX `IX_Penggajian_KaryawanId` ON `Penggajian` (`KaryawanId`);

CREATE UNIQUE INDEX `IX_Penggajian_PeriodePenggajianId_KaryawanId` ON `Penggajian` (`PeriodePenggajianId`, `KaryawanId`);

CREATE INDEX `IX_Penggajian_PerusahaanId_KaryawanId` ON `Penggajian` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_Penggajian_PerusahaanId_NomorSlipGaji` ON `Penggajian` (`PerusahaanId`, `NomorSlipGaji`);

CREATE INDEX `IX_PeriodePenggajian_DiajukanOlehId` ON `PeriodePenggajian` (`DiajukanOlehId`);

CREATE INDEX `IX_PeriodePenggajian_DisetujuiOlehId` ON `PeriodePenggajian` (`DisetujuiOlehId`);

CREATE UNIQUE INDEX `IX_PeriodePenggajian_PerusahaanId_Bulan_Tahun` ON `PeriodePenggajian` (`PerusahaanId`, `Bulan`, `Tahun`);

CREATE INDEX `IX_SlipGaji_DiterbitkanOlehId` ON `SlipGaji` (`DiterbitkanOlehId`);

CREATE UNIQUE INDEX `IX_SlipGaji_PenggajianId` ON `SlipGaji` (`PenggajianId`);

CREATE UNIQUE INDEX `IX_SlipGaji_PerusahaanId_NomorSlipGaji` ON `SlipGaji` (`PerusahaanId`, `NomorSlipGaji`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261116093836_TambahModulPenggajianLengkap', '10.0.15');

COMMIT;

