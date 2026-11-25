using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class TambahModulRekrutmenDanPenilaianKinerja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndikatorKPI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "char(36)", nullable: true),
                    JabatanId = table.Column<Guid>(type: "char(36)", nullable: true),
                    KodeIndikator = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaIndikator = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Deskripsi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Jenis = table.Column<int>(type: "int", nullable: false),
                    TargetKuantitatif = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Satuan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    BobotBawaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
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
                    table.PrimaryKey("PK_IndikatorKPI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndikatorKPI_Departemen_DepartemenId",
                        column: x => x.DepartemenId,
                        principalTable: "Departemen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IndikatorKPI_Jabatan_JabatanId",
                        column: x => x.JabatanId,
                        principalTable: "Jabatan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IndikatorKPI_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LowonganKerja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KodeLowongan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    JudulLowongan = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    DepartemenId = table.Column<Guid>(type: "char(36)", nullable: true),
                    JabatanId = table.Column<Guid>(type: "char(36)", nullable: true),
                    DeskripsiPekerjaan = table.Column<string>(type: "longtext", nullable: false),
                    Kualifikasi = table.Column<string>(type: "longtext", nullable: false),
                    TipePekerjaan = table.Column<int>(type: "int", nullable: false),
                    LokasiKerja = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    JumlahDibutuhkan = table.Column<int>(type: "int", nullable: false),
                    GajiMin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GajiMax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TanggalDibuka = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalDitutup = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_LowonganKerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LowonganKerja_Departemen_DepartemenId",
                        column: x => x.DepartemenId,
                        principalTable: "Departemen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LowonganKerja_Jabatan_JabatanId",
                        column: x => x.JabatanId,
                        principalTable: "Jabatan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LowonganKerja_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeriodePenilaian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaPeriode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Tahun = table.Column<int>(type: "int", nullable: false),
                    SemesterAtauKuartal = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    TanggalMulai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PeriodePenilaian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodePenilaian_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pelamar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LowonganKerjaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorRegistrasi = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaLengkap = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NomorTelepon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    PendidikanTerakhir = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    NamaInstitusi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Jurusan = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PengalamanKerjaTahun = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    GajiDiharapkan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NamaFileCV = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    JalurFileCV = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    CatatanAdministrasi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_Pelamar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pelamar_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pelamar_LowonganKerja_LowonganKerjaId",
                        column: x => x.LowonganKerjaId,
                        principalTable: "LowonganKerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pelamar_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TahapanSeleksi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LowonganKerjaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaTahapan = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    JenisTahapan = table.Column<int>(type: "int", nullable: false),
                    Urutan = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TahapanSeleksi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TahapanSeleksi_LowonganKerja_LowonganKerjaId",
                        column: x => x.LowonganKerjaId,
                        principalTable: "LowonganKerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TahapanSeleksi_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PenilaianKinerja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PeriodePenilaianId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PenilaiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaPenilai = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TanggalPenilaian = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalSkorAkhir = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PredikatKinerja = table.Column<int>(type: "int", nullable: false),
                    KomentarEvaluasi = table.Column<string>(type: "longtext", nullable: false),
                    RekomendasiPengembangan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PenilaianKinerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PenilaianKinerja_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenilaianKinerja_Pengguna_PenilaiId",
                        column: x => x.PenilaiId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenilaianKinerja_PeriodePenilaian_PeriodePenilaianId",
                        column: x => x.PeriodePenilaianId,
                        principalTable: "PeriodePenilaian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PenilaianKinerja_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JadwalWawancara",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PelamarId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TahapanSeleksiId = table.Column<Guid>(type: "char(36)", nullable: true),
                    PewawancaraId = table.Column<Guid>(type: "char(36)", nullable: true),
                    NamaPewawancara = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TanggalWawancara = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JamMulai = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    JamSelesai = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Metode = table.Column<int>(type: "int", nullable: false),
                    LokasiAtauTautan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CatatanUndangan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_JadwalWawancara", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JadwalWawancara_Pelamar_PelamarId",
                        column: x => x.PelamarId,
                        principalTable: "Pelamar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JadwalWawancara_Pengguna_PewawancaraId",
                        column: x => x.PewawancaraId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JadwalWawancara_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JadwalWawancara_TahapanSeleksi_TahapanSeleksiId",
                        column: x => x.TahapanSeleksiId,
                        principalTable: "TahapanSeleksi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetailPenilaian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PenilaianKinerjaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IndikatorKPIId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaIndikatorSnapshot = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TargetSnapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Realisasi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    BobotPersen = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SkorIndikator = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SkorTertimbang = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CatatanIndikator = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_DetailPenilaian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailPenilaian_IndikatorKPI_IndikatorKPIId",
                        column: x => x.IndikatorKPIId,
                        principalTable: "IndikatorKPI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetailPenilaian_PenilaianKinerja_PenilaianKinerjaId",
                        column: x => x.PenilaianKinerjaId,
                        principalTable: "PenilaianKinerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailPenilaian_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HasilSeleksi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PelamarId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TahapanSeleksiId = table.Column<Guid>(type: "char(36)", nullable: true),
                    JadwalWawancaraId = table.Column<Guid>(type: "char(36)", nullable: true),
                    PenilaiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaPenilai = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TanggalPenilaian = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Skor = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CatatanEvaluasi = table.Column<string>(type: "longtext", nullable: false),
                    Kelebihan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Kekurangan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Keputusan = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_HasilSeleksi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HasilSeleksi_JadwalWawancara_JadwalWawancaraId",
                        column: x => x.JadwalWawancaraId,
                        principalTable: "JadwalWawancara",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HasilSeleksi_Pelamar_PelamarId",
                        column: x => x.PelamarId,
                        principalTable: "Pelamar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HasilSeleksi_Pengguna_PenilaiId",
                        column: x => x.PenilaiId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HasilSeleksi_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HasilSeleksi_TahapanSeleksi_TahapanSeleksiId",
                        column: x => x.TahapanSeleksiId,
                        principalTable: "TahapanSeleksi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenilaian_IndikatorKPIId",
                table: "DetailPenilaian",
                column: "IndikatorKPIId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenilaian_PenilaianKinerjaId_IndikatorKPIId",
                table: "DetailPenilaian",
                columns: new[] { "PenilaianKinerjaId", "IndikatorKPIId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenilaian_PerusahaanId",
                table: "DetailPenilaian",
                column: "PerusahaanId");

            migrationBuilder.CreateIndex(
                name: "IX_HasilSeleksi_JadwalWawancaraId",
                table: "HasilSeleksi",
                column: "JadwalWawancaraId");

            migrationBuilder.CreateIndex(
                name: "IX_HasilSeleksi_PelamarId",
                table: "HasilSeleksi",
                column: "PelamarId");

            migrationBuilder.CreateIndex(
                name: "IX_HasilSeleksi_PenilaiId",
                table: "HasilSeleksi",
                column: "PenilaiId");

            migrationBuilder.CreateIndex(
                name: "IX_HasilSeleksi_PerusahaanId_PelamarId",
                table: "HasilSeleksi",
                columns: new[] { "PerusahaanId", "PelamarId" });

            migrationBuilder.CreateIndex(
                name: "IX_HasilSeleksi_TahapanSeleksiId",
                table: "HasilSeleksi",
                column: "TahapanSeleksiId");

            migrationBuilder.CreateIndex(
                name: "IX_IndikatorKPI_DepartemenId",
                table: "IndikatorKPI",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_IndikatorKPI_JabatanId",
                table: "IndikatorKPI",
                column: "JabatanId");

            migrationBuilder.CreateIndex(
                name: "IX_IndikatorKPI_PerusahaanId_DepartemenId",
                table: "IndikatorKPI",
                columns: new[] { "PerusahaanId", "DepartemenId" });

            migrationBuilder.CreateIndex(
                name: "IX_IndikatorKPI_PerusahaanId_KodeIndikator",
                table: "IndikatorKPI",
                columns: new[] { "PerusahaanId", "KodeIndikator" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JadwalWawancara_PelamarId",
                table: "JadwalWawancara",
                column: "PelamarId");

            migrationBuilder.CreateIndex(
                name: "IX_JadwalWawancara_PerusahaanId_PelamarId",
                table: "JadwalWawancara",
                columns: new[] { "PerusahaanId", "PelamarId" });

            migrationBuilder.CreateIndex(
                name: "IX_JadwalWawancara_PerusahaanId_TanggalWawancara",
                table: "JadwalWawancara",
                columns: new[] { "PerusahaanId", "TanggalWawancara" });

            migrationBuilder.CreateIndex(
                name: "IX_JadwalWawancara_PewawancaraId",
                table: "JadwalWawancara",
                column: "PewawancaraId");

            migrationBuilder.CreateIndex(
                name: "IX_JadwalWawancara_TahapanSeleksiId",
                table: "JadwalWawancara",
                column: "TahapanSeleksiId");

            migrationBuilder.CreateIndex(
                name: "IX_LowonganKerja_DepartemenId",
                table: "LowonganKerja",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_LowonganKerja_JabatanId",
                table: "LowonganKerja",
                column: "JabatanId");

            migrationBuilder.CreateIndex(
                name: "IX_LowonganKerja_PerusahaanId_KodeLowongan",
                table: "LowonganKerja",
                columns: new[] { "PerusahaanId", "KodeLowongan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LowonganKerja_PerusahaanId_Status",
                table: "LowonganKerja",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Pelamar_KaryawanId",
                table: "Pelamar",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_Pelamar_LowonganKerjaId",
                table: "Pelamar",
                column: "LowonganKerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pelamar_PerusahaanId_LowonganKerjaId",
                table: "Pelamar",
                columns: new[] { "PerusahaanId", "LowonganKerjaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pelamar_PerusahaanId_NomorRegistrasi",
                table: "Pelamar",
                columns: new[] { "PerusahaanId", "NomorRegistrasi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pelamar_PerusahaanId_Status",
                table: "Pelamar",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PenilaianKinerja_KaryawanId",
                table: "PenilaianKinerja",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_PenilaianKinerja_PenilaiId",
                table: "PenilaianKinerja",
                column: "PenilaiId");

            migrationBuilder.CreateIndex(
                name: "IX_PenilaianKinerja_PeriodePenilaianId_KaryawanId",
                table: "PenilaianKinerja",
                columns: new[] { "PeriodePenilaianId", "KaryawanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PenilaianKinerja_PerusahaanId_KaryawanId",
                table: "PenilaianKinerja",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PenilaianKinerja_PerusahaanId_PeriodePenilaianId",
                table: "PenilaianKinerja",
                columns: new[] { "PerusahaanId", "PeriodePenilaianId" });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodePenilaian_PerusahaanId_Status",
                table: "PeriodePenilaian",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodePenilaian_PerusahaanId_Tahun_NamaPeriode",
                table: "PeriodePenilaian",
                columns: new[] { "PerusahaanId", "Tahun", "NamaPeriode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TahapanSeleksi_LowonganKerjaId",
                table: "TahapanSeleksi",
                column: "LowonganKerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_TahapanSeleksi_PerusahaanId_LowonganKerjaId_Urutan",
                table: "TahapanSeleksi",
                columns: new[] { "PerusahaanId", "LowonganKerjaId", "Urutan" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailPenilaian");

            migrationBuilder.DropTable(
                name: "HasilSeleksi");

            migrationBuilder.DropTable(
                name: "IndikatorKPI");

            migrationBuilder.DropTable(
                name: "PenilaianKinerja");

            migrationBuilder.DropTable(
                name: "JadwalWawancara");

            migrationBuilder.DropTable(
                name: "PeriodePenilaian");

            migrationBuilder.DropTable(
                name: "Pelamar");

            migrationBuilder.DropTable(
                name: "TahapanSeleksi");

            migrationBuilder.DropTable(
                name: "LowonganKerja");
        }
    }
}
