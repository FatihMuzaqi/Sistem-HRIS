START TRANSACTION;
CREATE TABLE `IndikatorKPI` (
    `Id` char(36) NOT NULL,
    `DepartemenId` char(36) NULL,
    `JabatanId` char(36) NULL,
    `KodeIndikator` varchar(50) NOT NULL,
    `NamaIndikator` varchar(150) NOT NULL,
    `Deskripsi` varchar(500) NULL,
    `Jenis` int NOT NULL,
    `TargetKuantitatif` decimal(10,2) NULL,
    `Satuan` varchar(50) NULL,
    `BobotBawaan` decimal(5,2) NOT NULL,
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
    CONSTRAINT `FK_IndikatorKPI_Departemen_DepartemenId` FOREIGN KEY (`DepartemenId`) REFERENCES `Departemen` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_IndikatorKPI_Jabatan_JabatanId` FOREIGN KEY (`JabatanId`) REFERENCES `Jabatan` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_IndikatorKPI_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `LowonganKerja` (
    `Id` char(36) NOT NULL,
    `KodeLowongan` varchar(50) NOT NULL,
    `JudulLowongan` varchar(150) NOT NULL,
    `DepartemenId` char(36) NULL,
    `JabatanId` char(36) NULL,
    `DeskripsiPekerjaan` longtext NOT NULL,
    `Kualifikasi` longtext NOT NULL,
    `TipePekerjaan` int NOT NULL,
    `LokasiKerja` varchar(100) NOT NULL,
    `JumlahDibutuhkan` int NOT NULL,
    `GajiMin` decimal(18,2) NULL,
    `GajiMax` decimal(18,2) NULL,
    `TanggalDibuka` datetime(6) NOT NULL,
    `TanggalDitutup` datetime(6) NULL,
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
    CONSTRAINT `FK_LowonganKerja_Departemen_DepartemenId` FOREIGN KEY (`DepartemenId`) REFERENCES `Departemen` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_LowonganKerja_Jabatan_JabatanId` FOREIGN KEY (`JabatanId`) REFERENCES `Jabatan` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_LowonganKerja_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PeriodePenilaian` (
    `Id` char(36) NOT NULL,
    `NamaPeriode` varchar(100) NOT NULL,
    `Tahun` int NOT NULL,
    `SemesterAtauKuartal` varchar(50) NULL,
    `TanggalMulai` datetime(6) NOT NULL,
    `TanggalSelesai` datetime(6) NOT NULL,
    `Status` int NOT NULL,
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
    CONSTRAINT `FK_PeriodePenilaian_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Pelamar` (
    `Id` char(36) NOT NULL,
    `LowonganKerjaId` char(36) NOT NULL,
    `NomorRegistrasi` varchar(50) NOT NULL,
    `NamaLengkap` varchar(150) NOT NULL,
    `Email` varchar(100) NOT NULL,
    `NomorTelepon` varchar(30) NOT NULL,
    `PendidikanTerakhir` varchar(50) NULL,
    `NamaInstitusi` varchar(150) NULL,
    `Jurusan` varchar(100) NULL,
    `PengalamanKerjaTahun` decimal(4,1) NOT NULL,
    `GajiDiharapkan` decimal(18,2) NULL,
    `NamaFileCV` varchar(255) NOT NULL,
    `JalurFileCV` varchar(500) NOT NULL,
    `CatatanAdministrasi` varchar(500) NULL,
    `Status` int NOT NULL,
    `KaryawanId` char(36) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pelamar_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Pelamar_LowonganKerja_LowonganKerjaId` FOREIGN KEY (`LowonganKerjaId`) REFERENCES `LowonganKerja` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pelamar_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `TahapanSeleksi` (
    `Id` char(36) NOT NULL,
    `LowonganKerjaId` char(36) NOT NULL,
    `NamaTahapan` varchar(100) NOT NULL,
    `JenisTahapan` int NOT NULL,
    `Urutan` int NOT NULL,
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
    CONSTRAINT `FK_TahapanSeleksi_LowonganKerja_LowonganKerjaId` FOREIGN KEY (`LowonganKerjaId`) REFERENCES `LowonganKerja` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_TahapanSeleksi_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `PenilaianKinerja` (
    `Id` char(36) NOT NULL,
    `PeriodePenilaianId` char(36) NOT NULL,
    `KaryawanId` char(36) NOT NULL,
    `PenilaiId` char(36) NOT NULL,
    `NamaPenilai` varchar(150) NOT NULL,
    `TanggalPenilaian` datetime(6) NOT NULL,
    `TotalSkorAkhir` decimal(5,2) NOT NULL,
    `PredikatKinerja` int NOT NULL,
    `KomentarEvaluasi` longtext NOT NULL,
    `RekomendasiPengembangan` varchar(500) NULL,
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
    CONSTRAINT `FK_PenilaianKinerja_Karyawan_KaryawanId` FOREIGN KEY (`KaryawanId`) REFERENCES `Karyawan` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenilaianKinerja_Pengguna_PenilaiId` FOREIGN KEY (`PenilaiId`) REFERENCES `Pengguna` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PenilaianKinerja_PeriodePenilaian_PeriodePenilaianId` FOREIGN KEY (`PeriodePenilaianId`) REFERENCES `PeriodePenilaian` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PenilaianKinerja_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `JadwalWawancara` (
    `Id` char(36) NOT NULL,
    `PelamarId` char(36) NOT NULL,
    `TahapanSeleksiId` char(36) NULL,
    `PewawancaraId` char(36) NULL,
    `NamaPewawancara` varchar(150) NOT NULL,
    `TanggalWawancara` datetime(6) NOT NULL,
    `JamMulai` time(6) NOT NULL,
    `JamSelesai` time(6) NOT NULL,
    `Metode` int NOT NULL,
    `LokasiAtauTautan` varchar(255) NOT NULL,
    `Status` int NOT NULL,
    `CatatanUndangan` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_JadwalWawancara_Pelamar_PelamarId` FOREIGN KEY (`PelamarId`) REFERENCES `Pelamar` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_JadwalWawancara_Pengguna_PewawancaraId` FOREIGN KEY (`PewawancaraId`) REFERENCES `Pengguna` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_JadwalWawancara_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_JadwalWawancara_TahapanSeleksi_TahapanSeleksiId` FOREIGN KEY (`TahapanSeleksiId`) REFERENCES `TahapanSeleksi` (`Id`) ON DELETE SET NULL
);

CREATE TABLE `DetailPenilaian` (
    `Id` char(36) NOT NULL,
    `PenilaianKinerjaId` char(36) NOT NULL,
    `IndikatorKPIId` char(36) NOT NULL,
    `NamaIndikatorSnapshot` varchar(150) NOT NULL,
    `TargetSnapshot` varchar(100) NULL,
    `Realisasi` varchar(100) NULL,
    `BobotPersen` decimal(5,2) NOT NULL,
    `SkorIndikator` decimal(5,2) NOT NULL,
    `SkorTertimbang` decimal(5,2) NOT NULL,
    `CatatanIndikator` varchar(500) NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DetailPenilaian_IndikatorKPI_IndikatorKPIId` FOREIGN KEY (`IndikatorKPIId`) REFERENCES `IndikatorKPI` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_DetailPenilaian_PenilaianKinerja_PenilaianKinerjaId` FOREIGN KEY (`PenilaianKinerjaId`) REFERENCES `PenilaianKinerja` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_DetailPenilaian_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `HasilSeleksi` (
    `Id` char(36) NOT NULL,
    `PelamarId` char(36) NOT NULL,
    `TahapanSeleksiId` char(36) NULL,
    `JadwalWawancaraId` char(36) NULL,
    `PenilaiId` char(36) NOT NULL,
    `NamaPenilai` varchar(150) NOT NULL,
    `TanggalPenilaian` datetime(6) NOT NULL,
    `Skor` decimal(5,2) NOT NULL,
    `CatatanEvaluasi` longtext NOT NULL,
    `Kelebihan` varchar(500) NULL,
    `Kekurangan` varchar(500) NULL,
    `Keputusan` int NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `CreatedBy` varchar(100) NULL,
    `UpdatedAt` datetime NULL,
    `UpdatedBy` varchar(100) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    `DeletedAt` datetime NULL,
    `DeletedBy` varchar(100) NULL,
    `PerusahaanId` char(36) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_HasilSeleksi_JadwalWawancara_JadwalWawancaraId` FOREIGN KEY (`JadwalWawancaraId`) REFERENCES `JadwalWawancara` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_HasilSeleksi_Pelamar_PelamarId` FOREIGN KEY (`PelamarId`) REFERENCES `Pelamar` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_HasilSeleksi_Pengguna_PenilaiId` FOREIGN KEY (`PenilaiId`) REFERENCES `Pengguna` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_HasilSeleksi_Perusahaan_PerusahaanId` FOREIGN KEY (`PerusahaanId`) REFERENCES `Perusahaan` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_HasilSeleksi_TahapanSeleksi_TahapanSeleksiId` FOREIGN KEY (`TahapanSeleksiId`) REFERENCES `TahapanSeleksi` (`Id`) ON DELETE SET NULL
);

CREATE INDEX `IX_DetailPenilaian_IndikatorKPIId` ON `DetailPenilaian` (`IndikatorKPIId`);

CREATE UNIQUE INDEX `IX_DetailPenilaian_PenilaianKinerjaId_IndikatorKPIId` ON `DetailPenilaian` (`PenilaianKinerjaId`, `IndikatorKPIId`);

CREATE INDEX `IX_DetailPenilaian_PerusahaanId` ON `DetailPenilaian` (`PerusahaanId`);

CREATE INDEX `IX_HasilSeleksi_JadwalWawancaraId` ON `HasilSeleksi` (`JadwalWawancaraId`);

CREATE INDEX `IX_HasilSeleksi_PelamarId` ON `HasilSeleksi` (`PelamarId`);

CREATE INDEX `IX_HasilSeleksi_PenilaiId` ON `HasilSeleksi` (`PenilaiId`);

CREATE INDEX `IX_HasilSeleksi_PerusahaanId_PelamarId` ON `HasilSeleksi` (`PerusahaanId`, `PelamarId`);

CREATE INDEX `IX_HasilSeleksi_TahapanSeleksiId` ON `HasilSeleksi` (`TahapanSeleksiId`);

CREATE INDEX `IX_IndikatorKPI_DepartemenId` ON `IndikatorKPI` (`DepartemenId`);

CREATE INDEX `IX_IndikatorKPI_JabatanId` ON `IndikatorKPI` (`JabatanId`);

CREATE INDEX `IX_IndikatorKPI_PerusahaanId_DepartemenId` ON `IndikatorKPI` (`PerusahaanId`, `DepartemenId`);

CREATE UNIQUE INDEX `IX_IndikatorKPI_PerusahaanId_KodeIndikator` ON `IndikatorKPI` (`PerusahaanId`, `KodeIndikator`);

CREATE INDEX `IX_JadwalWawancara_PelamarId` ON `JadwalWawancara` (`PelamarId`);

CREATE INDEX `IX_JadwalWawancara_PerusahaanId_PelamarId` ON `JadwalWawancara` (`PerusahaanId`, `PelamarId`);

CREATE INDEX `IX_JadwalWawancara_PerusahaanId_TanggalWawancara` ON `JadwalWawancara` (`PerusahaanId`, `TanggalWawancara`);

CREATE INDEX `IX_JadwalWawancara_PewawancaraId` ON `JadwalWawancara` (`PewawancaraId`);

CREATE INDEX `IX_JadwalWawancara_TahapanSeleksiId` ON `JadwalWawancara` (`TahapanSeleksiId`);

CREATE INDEX `IX_LowonganKerja_DepartemenId` ON `LowonganKerja` (`DepartemenId`);

CREATE INDEX `IX_LowonganKerja_JabatanId` ON `LowonganKerja` (`JabatanId`);

CREATE UNIQUE INDEX `IX_LowonganKerja_PerusahaanId_KodeLowongan` ON `LowonganKerja` (`PerusahaanId`, `KodeLowongan`);

CREATE INDEX `IX_LowonganKerja_PerusahaanId_Status` ON `LowonganKerja` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_Pelamar_KaryawanId` ON `Pelamar` (`KaryawanId`);

CREATE INDEX `IX_Pelamar_LowonganKerjaId` ON `Pelamar` (`LowonganKerjaId`);

CREATE INDEX `IX_Pelamar_PerusahaanId_LowonganKerjaId` ON `Pelamar` (`PerusahaanId`, `LowonganKerjaId`);

CREATE UNIQUE INDEX `IX_Pelamar_PerusahaanId_NomorRegistrasi` ON `Pelamar` (`PerusahaanId`, `NomorRegistrasi`);

CREATE INDEX `IX_Pelamar_PerusahaanId_Status` ON `Pelamar` (`PerusahaanId`, `Status`);

CREATE INDEX `IX_PenilaianKinerja_KaryawanId` ON `PenilaianKinerja` (`KaryawanId`);

CREATE INDEX `IX_PenilaianKinerja_PenilaiId` ON `PenilaianKinerja` (`PenilaiId`);

CREATE UNIQUE INDEX `IX_PenilaianKinerja_PeriodePenilaianId_KaryawanId` ON `PenilaianKinerja` (`PeriodePenilaianId`, `KaryawanId`);

CREATE INDEX `IX_PenilaianKinerja_PerusahaanId_KaryawanId` ON `PenilaianKinerja` (`PerusahaanId`, `KaryawanId`);

CREATE INDEX `IX_PenilaianKinerja_PerusahaanId_PeriodePenilaianId` ON `PenilaianKinerja` (`PerusahaanId`, `PeriodePenilaianId`);

CREATE INDEX `IX_PeriodePenilaian_PerusahaanId_Status` ON `PeriodePenilaian` (`PerusahaanId`, `Status`);

CREATE UNIQUE INDEX `IX_PeriodePenilaian_PerusahaanId_Tahun_NamaPeriode` ON `PeriodePenilaian` (`PerusahaanId`, `Tahun`, `NamaPeriode`);

CREATE INDEX `IX_TahapanSeleksi_LowonganKerjaId` ON `TahapanSeleksi` (`LowonganKerjaId`);

CREATE INDEX `IX_TahapanSeleksi_PerusahaanId_LowonganKerjaId_Urutan` ON `TahapanSeleksi` (`PerusahaanId`, `LowonganKerjaId`, `Urutan`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261125155354_TambahModulRekrutmenDanPenilaianKinerja', '10.0.15');

COMMIT;

