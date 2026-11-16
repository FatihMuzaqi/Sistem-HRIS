using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class TambahModulPenggajianLengkap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NamaBank",
                table: "Karyawan",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorBpjsKesehatan",
                table: "Karyawan",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorBpjsKetenagakerjaan",
                table: "Karyawan",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorNPWP",
                table: "Karyawan",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomorRekeningBank",
                table: "Karyawan",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusPTKP",
                table: "Karyawan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KebijakanPenggajianPerusahaan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    HitungLemburOtomatis = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TarifLemburPerJamFlat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TerapkanPotonganKeterlambatan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ToleransiKeterlambatanGratisMenit = table.Column<int>(type: "int", nullable: false),
                    DendaKeterlambatanPerMenit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TerapkanBpjsKesehatan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersenBpjsKesehatanPerusahaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PersenBpjsKesehatanPekerja = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BatasMaksimalUpahBpjsKesehatan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TerapkanBpjsKetenagakerjaan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersenBpjsJhtPerusahaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PersenBpjsJhtPekerja = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PersenBpjsJpPerusahaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PersenBpjsJpPekerja = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BatasMaksimalUpahBpjsJp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersenBpjsJkkPerusahaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PersenBpjsJkmPerusahaan = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TerapkanPph21 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MetodePph21 = table.Column<int>(type: "int", nullable: false),
                    TanggalCutoffAwal = table.Column<int>(type: "int", nullable: false),
                    TanggalCutoffAkhir = table.Column<int>(type: "int", nullable: false),
                    TanggalPembayaranGaji = table.Column<int>(type: "int", nullable: false),
                    CatatanSlipGajiDefault = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_KebijakanPenggajianPerusahaan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KebijakanPenggajianPerusahaan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KomponenGaji",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KodeKomponen = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaKomponen = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Jenis = table.Column<int>(type: "int", nullable: false),
                    Sifat = table.Column<int>(type: "int", nullable: false),
                    Kategori = table.Column<int>(type: "int", nullable: false),
                    NilaiBawaan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KenaPajak = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MasukKalkulasiBpjs = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DihitungOtomatisOlehSistem = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_KomponenGaji", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KomponenGaji_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeriodePenggajian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaPeriode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Bulan = table.Column<int>(type: "int", nullable: false),
                    Tahun = table.Column<int>(type: "int", nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalPembayaran = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalKaryawan = table.Column<int>(type: "int", nullable: false),
                    TotalPengeluaranGaji = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DiajukanOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
                    WaktuDiajukan = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    DisetujuiOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
                    WaktuDisetujui = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
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
                    table.PrimaryKey("PK_PeriodePenggajian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodePenggajian_Pengguna_DiajukanOlehId",
                        column: x => x.DiajukanOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PeriodePenggajian_Pengguna_DisetujuiOlehId",
                        column: x => x.DisetujuiOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PeriodePenggajian_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GajiKaryawan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KomponenGajiId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Nominal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_GajiKaryawan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GajiKaryawan_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GajiKaryawan_KomponenGaji_KomponenGajiId",
                        column: x => x.KomponenGajiId,
                        principalTable: "KomponenGaji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GajiKaryawan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Penggajian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PeriodePenggajianId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorSlipGaji = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    StatusPTKP = table.Column<int>(type: "int", nullable: false),
                    MetodePajak = table.Column<int>(type: "int", nullable: false),
                    HariKerjaEfektif = table.Column<int>(type: "int", nullable: false),
                    JumlahHadir = table.Column<int>(type: "int", nullable: false),
                    JumlahTerlambat = table.Column<int>(type: "int", nullable: false),
                    TotalMenitTerlambat = table.Column<int>(type: "int", nullable: false),
                    JumlahJamLembur = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    GajiPokok = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTunjanganTetap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTunjanganTidakTetap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUangLembur = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPendapatanKotor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPotonganBpjs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPotonganPph21 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPotonganKeterlambatan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPotonganLainnya = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPotongan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GajiBersih = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalBpjsDitanggungPerusahaan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApakahTerkunci = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Catatan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisetujuiOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
                    WaktuDisetujui = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
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
                    table.PrimaryKey("PK_Penggajian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penggajian_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Penggajian_Pengguna_DisetujuiOlehId",
                        column: x => x.DisetujuiOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Penggajian_PeriodePenggajian_PeriodePenggajianId",
                        column: x => x.PeriodePenggajianId,
                        principalTable: "PeriodePenggajian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Penggajian_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetailPenggajian",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PenggajianId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KomponenGajiId = table.Column<Guid>(type: "char(36)", nullable: true),
                    KodeKomponen = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaKomponen = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Jenis = table.Column<int>(type: "int", nullable: false),
                    Sifat = table.Column<int>(type: "int", nullable: false),
                    Kategori = table.Column<int>(type: "int", nullable: false),
                    Nominal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NominalDitanggungPerusahaan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Keterangan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Urutan = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DetailPenggajian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailPenggajian_KomponenGaji_KomponenGajiId",
                        column: x => x.KomponenGajiId,
                        principalTable: "KomponenGaji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DetailPenggajian_Penggajian_PenggajianId",
                        column: x => x.PenggajianId,
                        principalTable: "Penggajian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailPenggajian_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SlipGaji",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PenggajianId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorSlipGaji = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    KodeVerifikasi = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    WaktuDiterbitkan = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    DiterbitkanOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
                    SudahDilihatKaryawan = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WaktuDilihatKaryawan = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    CatatanKaki = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SlipGaji", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlipGaji_Penggajian_PenggajianId",
                        column: x => x.PenggajianId,
                        principalTable: "Penggajian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SlipGaji_Pengguna_DiterbitkanOlehId",
                        column: x => x.DiterbitkanOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SlipGaji_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenggajian_KomponenGajiId",
                table: "DetailPenggajian",
                column: "KomponenGajiId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenggajian_PenggajianId_Jenis",
                table: "DetailPenggajian",
                columns: new[] { "PenggajianId", "Jenis" });

            migrationBuilder.CreateIndex(
                name: "IX_DetailPenggajian_PerusahaanId",
                table: "DetailPenggajian",
                column: "PerusahaanId");

            migrationBuilder.CreateIndex(
                name: "IX_GajiKaryawan_KaryawanId",
                table: "GajiKaryawan",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_GajiKaryawan_KomponenGajiId",
                table: "GajiKaryawan",
                column: "KomponenGajiId");

            migrationBuilder.CreateIndex(
                name: "IX_GajiKaryawan_PerusahaanId_KaryawanId_KomponenGajiId",
                table: "GajiKaryawan",
                columns: new[] { "PerusahaanId", "KaryawanId", "KomponenGajiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KebijakanPenggajianPerusahaan_PerusahaanId",
                table: "KebijakanPenggajianPerusahaan",
                column: "PerusahaanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KomponenGaji_PerusahaanId_Jenis",
                table: "KomponenGaji",
                columns: new[] { "PerusahaanId", "Jenis" });

            migrationBuilder.CreateIndex(
                name: "IX_KomponenGaji_PerusahaanId_KodeKomponen",
                table: "KomponenGaji",
                columns: new[] { "PerusahaanId", "KodeKomponen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Penggajian_DisetujuiOlehId",
                table: "Penggajian",
                column: "DisetujuiOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_Penggajian_KaryawanId",
                table: "Penggajian",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_Penggajian_PeriodePenggajianId_KaryawanId",
                table: "Penggajian",
                columns: new[] { "PeriodePenggajianId", "KaryawanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Penggajian_PerusahaanId_KaryawanId",
                table: "Penggajian",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Penggajian_PerusahaanId_NomorSlipGaji",
                table: "Penggajian",
                columns: new[] { "PerusahaanId", "NomorSlipGaji" });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodePenggajian_DiajukanOlehId",
                table: "PeriodePenggajian",
                column: "DiajukanOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodePenggajian_DisetujuiOlehId",
                table: "PeriodePenggajian",
                column: "DisetujuiOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodePenggajian_PerusahaanId_Bulan_Tahun",
                table: "PeriodePenggajian",
                columns: new[] { "PerusahaanId", "Bulan", "Tahun" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlipGaji_DiterbitkanOlehId",
                table: "SlipGaji",
                column: "DiterbitkanOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_SlipGaji_PenggajianId",
                table: "SlipGaji",
                column: "PenggajianId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlipGaji_PerusahaanId_NomorSlipGaji",
                table: "SlipGaji",
                columns: new[] { "PerusahaanId", "NomorSlipGaji" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailPenggajian");

            migrationBuilder.DropTable(
                name: "GajiKaryawan");

            migrationBuilder.DropTable(
                name: "KebijakanPenggajianPerusahaan");

            migrationBuilder.DropTable(
                name: "SlipGaji");

            migrationBuilder.DropTable(
                name: "KomponenGaji");

            migrationBuilder.DropTable(
                name: "Penggajian");

            migrationBuilder.DropTable(
                name: "PeriodePenggajian");

            migrationBuilder.DropColumn(
                name: "NamaBank",
                table: "Karyawan");

            migrationBuilder.DropColumn(
                name: "NomorBpjsKesehatan",
                table: "Karyawan");

            migrationBuilder.DropColumn(
                name: "NomorBpjsKetenagakerjaan",
                table: "Karyawan");

            migrationBuilder.DropColumn(
                name: "NomorNPWP",
                table: "Karyawan");

            migrationBuilder.DropColumn(
                name: "NomorRekeningBank",
                table: "Karyawan");

            migrationBuilder.DropColumn(
                name: "StatusPTKP",
                table: "Karyawan");
        }
    }
}
