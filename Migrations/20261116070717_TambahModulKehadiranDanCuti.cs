using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class TambahModulKehadiranDanCuti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JenisCuti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaJenisCuti = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    KodeJenisCuti = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    JatahHariPerTahun = table.Column<int>(type: "int", nullable: false),
                    PotongCutiTahunan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WajibLampiran = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatusAktif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Keterangan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JenisCuti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JenisCuti_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PengajuanLembur",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorPengajuan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TanggalLembur = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JamMulai = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    JamSelesai = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TotalJam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlasanLembur = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisetujuiOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
                    WaktuPersetujuan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CatatanPersetujuan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengajuanLembur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PengajuanLembur_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PengajuanLembur_Pengguna_DisetujuiOlehId",
                        column: x => x.DisetujuiOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PengajuanLembur_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShiftKerja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaShift = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    KodeShift = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    JamMasuk = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    JamPulang = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    ToleransiKeterlambatanMenit = table.Column<int>(type: "int", nullable: false),
                    Keterangan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    StatusAktif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftKerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftKerja_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PengajuanCuti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    JenisCutiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorPengajuan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JumlahHari = table.Column<int>(type: "int", nullable: false),
                    Alasan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    AlamatSelamaCuti = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    NomorTeleponDarurat = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    NamaDokumenPendukung = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    JalurDokumenPendukung = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PengajuanCuti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PengajuanCuti_JenisCuti_JenisCutiId",
                        column: x => x.JenisCutiId,
                        principalTable: "JenisCuti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PengajuanCuti_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PengajuanCuti_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SaldoCuti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    JenisCutiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PeriodeTahun = table.Column<int>(type: "int", nullable: false),
                    JumlahHakCuti = table.Column<int>(type: "int", nullable: false),
                    CutiTerpakai = table.Column<int>(type: "int", nullable: false),
                    SisaCuti = table.Column<int>(type: "int", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaldoCuti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaldoCuti_JenisCuti_JenisCutiId",
                        column: x => x.JenisCutiId,
                        principalTable: "JenisCuti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaldoCuti_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaldoCuti_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JadwalKerja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ShiftKerjaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatusHariLibur = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JadwalKerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JadwalKerja_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JadwalKerja_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JadwalKerja_ShiftKerja_ShiftKerjaId",
                        column: x => x.ShiftKerjaId,
                        principalTable: "ShiftKerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Kehadiran",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ShiftKerjaId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Tanggal = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WaktuMasuk = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WaktuPulang = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MenitKeterlambatan = table.Column<int>(type: "int", nullable: false),
                    MenitPulangCepat = table.Column<int>(type: "int", nullable: false),
                    TotalJamKerja = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CatatanMasuk = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CatatanPulang = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    LokasiMasuk = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    LokasiPulang = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    FotoMasuk = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    FotoPulang = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kehadiran", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kehadiran_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Kehadiran_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Kehadiran_ShiftKerja_ShiftKerjaId",
                        column: x => x.ShiftKerjaId,
                        principalTable: "ShiftKerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PersetujuanCuti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PengajuanCutiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Tahap = table.Column<int>(type: "int", nullable: false),
                    PenyetujuId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Keputusan = table.Column<int>(type: "int", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    WaktuPersetujuan = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersetujuanCuti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersetujuanCuti_PengajuanCuti_PengajuanCutiId",
                        column: x => x.PengajuanCutiId,
                        principalTable: "PengajuanCuti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersetujuanCuti_Pengguna_PenyetujuId",
                        column: x => x.PenyetujuId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersetujuanCuti_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JadwalKerja_KaryawanId",
                table: "JadwalKerja",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_JadwalKerja_PerusahaanId_KaryawanId_Tanggal",
                table: "JadwalKerja",
                columns: new[] { "PerusahaanId", "KaryawanId", "Tanggal" });

            migrationBuilder.CreateIndex(
                name: "IX_JadwalKerja_ShiftKerjaId",
                table: "JadwalKerja",
                column: "ShiftKerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_JenisCuti_PerusahaanId_KodeJenisCuti",
                table: "JenisCuti",
                columns: new[] { "PerusahaanId", "KodeJenisCuti" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kehadiran_KaryawanId",
                table: "Kehadiran",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_Kehadiran_PerusahaanId_KaryawanId_Tanggal",
                table: "Kehadiran",
                columns: new[] { "PerusahaanId", "KaryawanId", "Tanggal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kehadiran_PerusahaanId_Status",
                table: "Kehadiran",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Kehadiran_PerusahaanId_Tanggal",
                table: "Kehadiran",
                columns: new[] { "PerusahaanId", "Tanggal" });

            migrationBuilder.CreateIndex(
                name: "IX_Kehadiran_ShiftKerjaId",
                table: "Kehadiran",
                column: "ShiftKerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanCuti_JenisCutiId",
                table: "PengajuanCuti",
                column: "JenisCutiId");

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanCuti_KaryawanId",
                table: "PengajuanCuti",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanCuti_PerusahaanId_KaryawanId",
                table: "PengajuanCuti",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanCuti_PerusahaanId_Status",
                table: "PengajuanCuti",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanLembur_DisetujuiOlehId",
                table: "PengajuanLembur",
                column: "DisetujuiOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanLembur_KaryawanId",
                table: "PengajuanLembur",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanLembur_PerusahaanId_KaryawanId_TanggalLembur",
                table: "PengajuanLembur",
                columns: new[] { "PerusahaanId", "KaryawanId", "TanggalLembur" });

            migrationBuilder.CreateIndex(
                name: "IX_PengajuanLembur_PerusahaanId_Status",
                table: "PengajuanLembur",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PersetujuanCuti_PengajuanCutiId",
                table: "PersetujuanCuti",
                column: "PengajuanCutiId");

            migrationBuilder.CreateIndex(
                name: "IX_PersetujuanCuti_PenyetujuId",
                table: "PersetujuanCuti",
                column: "PenyetujuId");

            migrationBuilder.CreateIndex(
                name: "IX_PersetujuanCuti_PerusahaanId_PengajuanCutiId",
                table: "PersetujuanCuti",
                columns: new[] { "PerusahaanId", "PengajuanCutiId" });

            migrationBuilder.CreateIndex(
                name: "IX_SaldoCuti_JenisCutiId",
                table: "SaldoCuti",
                column: "JenisCutiId");

            migrationBuilder.CreateIndex(
                name: "IX_SaldoCuti_KaryawanId",
                table: "SaldoCuti",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_SaldoCuti_PerusahaanId_KaryawanId_JenisCutiId_PeriodeTahun",
                table: "SaldoCuti",
                columns: new[] { "PerusahaanId", "KaryawanId", "JenisCutiId", "PeriodeTahun" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftKerja_PerusahaanId_KodeShift",
                table: "ShiftKerja",
                columns: new[] { "PerusahaanId", "KodeShift" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JadwalKerja");

            migrationBuilder.DropTable(
                name: "Kehadiran");

            migrationBuilder.DropTable(
                name: "PengajuanLembur");

            migrationBuilder.DropTable(
                name: "PersetujuanCuti");

            migrationBuilder.DropTable(
                name: "SaldoCuti");

            migrationBuilder.DropTable(
                name: "ShiftKerja");

            migrationBuilder.DropTable(
                name: "PengajuanCuti");

            migrationBuilder.DropTable(
                name: "JenisCuti");
        }
    }
}
