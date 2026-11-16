DELETE FROM SlipGaji;
DELETE FROM DetailPenggajian;
DELETE FROM Penggajian;
DELETE FROM PeriodePenggajian;
DELETE FROM GajiKaryawan WHERE KomponenGajiId IN (SELECT Id FROM KomponenGaji WHERE KodeKomponen = 'BONUS-PROYEK');
DELETE FROM KomponenGaji WHERE KodeKomponen = 'BONUS-PROYEK';
