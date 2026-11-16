DELETE FROM PersetujuanCuti;
DELETE FROM PengajuanCuti;
DELETE FROM Kehadiran;
DELETE FROM PengajuanLembur;
DELETE FROM ShiftKerja WHERE KodeShift = 'SFT-MALAM';
UPDATE SaldoCuti SET CutiTerpakai = 0, SisaCuti = 12 WHERE PeriodeTahun = 2026;
