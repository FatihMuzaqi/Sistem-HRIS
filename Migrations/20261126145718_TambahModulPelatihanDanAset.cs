using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class TambahModulPelatihanDanAset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsetPerusahaan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorAset = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaAset = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Kategori = table.Column<int>(type: "int", nullable: false),
                    MerkModel = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    NomorSeri = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TanggalPembelian = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NilaiPerolehan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kondisi = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LokasiPenyimpanan = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DeskripsiSpesifikasi = table.Column<string>(type: "longtext", nullable: true),
                    PemegangSaatIniId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_AsetPerusahaan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsetPerusahaan_Karyawan_PemegangSaatIniId",
                        column: x => x.PemegangSaatIniId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AsetPerusahaan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProgramPelatihan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KodeProgram = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaProgram = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Deskripsi = table.Column<string>(type: "longtext", nullable: false),
                    Penyelenggara = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Instruktur = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Metode = table.Column<int>(type: "int", nullable: false),
                    LokasiAtauTautan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalJam = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BiayaPerPeserta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaksimalPeserta = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_ProgramPelatihan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramPelatihan_Departemen_DepartemenId",
                        column: x => x.DepartemenId,
                        principalTable: "Departemen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProgramPelatihan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PenyerahanAset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AsetPerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorDokumen = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TanggalPenyerahan = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalTargetPengembalian = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TanggalRealisasiPengembalian = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    KondisiSaatPenyerahan = table.Column<int>(type: "int", nullable: false),
                    KondisiSaatPengembalian = table.Column<int>(type: "int", nullable: true),
                    StatusPeminjaman = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    KelengkapanAset = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CatatanPenyerahan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CatatanPengembalian = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DiserahkanOlehId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DiterimaKembaliOlehId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_PenyerahanAset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PenyerahanAset_AsetPerusahaan_AsetPerusahaanId",
                        column: x => x.AsetPerusahaanId,
                        principalTable: "AsetPerusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PenyerahanAset_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenyerahanAset_Pengguna_DiserahkanOlehId",
                        column: x => x.DiserahkanOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PenyerahanAset_Pengguna_DiterimaKembaliOlehId",
                        column: x => x.DiterimaKembaliOlehId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PenyerahanAset_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RiwayatAset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AsetPerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TipeAktivitas = table.Column<int>(type: "int", nullable: false),
                    TanggalAktivitas = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    KaryawanTerkaitId = table.Column<Guid>(type: "char(36)", nullable: true),
                    NamaPetugas = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    KondisiSebelumnya = table.Column<int>(type: "int", nullable: false),
                    KondisiTerbaru = table.Column<int>(type: "int", nullable: false),
                    Deskripsi = table.Column<string>(type: "longtext", nullable: false),
                    BiayaPerbaikan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_RiwayatAset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiwayatAset_AsetPerusahaan_AsetPerusahaanId",
                        column: x => x.AsetPerusahaanId,
                        principalTable: "AsetPerusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiwayatAset_Karyawan_KaryawanTerkaitId",
                        column: x => x.KaryawanTerkaitId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RiwayatAset_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PesertaPelatihan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProgramPelatihanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StatusKeikutsertaan = table.Column<int>(type: "int", nullable: false),
                    NilaiAkhir = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Catatan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    TanggalDidaftarkan = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalSelesai = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PesertaPelatihan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesertaPelatihan_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PesertaPelatihan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PesertaPelatihan_ProgramPelatihan_ProgramPelatihanId",
                        column: x => x.ProgramPelatihanId,
                        principalTable: "ProgramPelatihan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SertifikatPelatihan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PesertaPelatihanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorSertifikat = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    JudulSertifikat = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TanggalTerbit = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalKedaluwarsa = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PenerbitSertifikat = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    KodeVerifikasi = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FileSertifikat = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_SertifikatPelatihan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SertifikatPelatihan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SertifikatPelatihan_PesertaPelatihan_PesertaPelatihanId",
                        column: x => x.PesertaPelatihanId,
                        principalTable: "PesertaPelatihan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AsetPerusahaan_PemegangSaatIniId",
                table: "AsetPerusahaan",
                column: "PemegangSaatIniId");

            migrationBuilder.CreateIndex(
                name: "IX_AsetPerusahaan_PerusahaanId_Kategori",
                table: "AsetPerusahaan",
                columns: new[] { "PerusahaanId", "Kategori" });

            migrationBuilder.CreateIndex(
                name: "IX_AsetPerusahaan_PerusahaanId_Kondisi",
                table: "AsetPerusahaan",
                columns: new[] { "PerusahaanId", "Kondisi" });

            migrationBuilder.CreateIndex(
                name: "IX_AsetPerusahaan_PerusahaanId_NomorAset",
                table: "AsetPerusahaan",
                columns: new[] { "PerusahaanId", "NomorAset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsetPerusahaan_PerusahaanId_Status",
                table: "AsetPerusahaan",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_AsetPerusahaanId",
                table: "PenyerahanAset",
                column: "AsetPerusahaanId");

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_DiserahkanOlehId",
                table: "PenyerahanAset",
                column: "DiserahkanOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_DiterimaKembaliOlehId",
                table: "PenyerahanAset",
                column: "DiterimaKembaliOlehId");

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_KaryawanId",
                table: "PenyerahanAset",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_PerusahaanId_AsetPerusahaanId",
                table: "PenyerahanAset",
                columns: new[] { "PerusahaanId", "AsetPerusahaanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_PerusahaanId_KaryawanId",
                table: "PenyerahanAset",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PenyerahanAset_PerusahaanId_NomorDokumen",
                table: "PenyerahanAset",
                columns: new[] { "PerusahaanId", "NomorDokumen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PesertaPelatihan_KaryawanId",
                table: "PesertaPelatihan",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_PesertaPelatihan_PerusahaanId_KaryawanId",
                table: "PesertaPelatihan",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PesertaPelatihan_PerusahaanId_StatusKeikutsertaan",
                table: "PesertaPelatihan",
                columns: new[] { "PerusahaanId", "StatusKeikutsertaan" });

            migrationBuilder.CreateIndex(
                name: "IX_PesertaPelatihan_ProgramPelatihanId_KaryawanId",
                table: "PesertaPelatihan",
                columns: new[] { "ProgramPelatihanId", "KaryawanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPelatihan_DepartemenId",
                table: "ProgramPelatihan",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPelatihan_PerusahaanId_KodeProgram",
                table: "ProgramPelatihan",
                columns: new[] { "PerusahaanId", "KodeProgram" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPelatihan_PerusahaanId_Status",
                table: "ProgramPelatihan",
                columns: new[] { "PerusahaanId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPelatihan_PerusahaanId_TanggalMulai",
                table: "ProgramPelatihan",
                columns: new[] { "PerusahaanId", "TanggalMulai" });

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatAset_AsetPerusahaanId",
                table: "RiwayatAset",
                column: "AsetPerusahaanId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatAset_KaryawanTerkaitId",
                table: "RiwayatAset",
                column: "KaryawanTerkaitId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatAset_PerusahaanId_AsetPerusahaanId",
                table: "RiwayatAset",
                columns: new[] { "PerusahaanId", "AsetPerusahaanId" });

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatAset_PerusahaanId_TanggalAktivitas",
                table: "RiwayatAset",
                columns: new[] { "PerusahaanId", "TanggalAktivitas" });

            migrationBuilder.CreateIndex(
                name: "IX_SertifikatPelatihan_PerusahaanId_KodeVerifikasi",
                table: "SertifikatPelatihan",
                columns: new[] { "PerusahaanId", "KodeVerifikasi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SertifikatPelatihan_PerusahaanId_NomorSertifikat",
                table: "SertifikatPelatihan",
                columns: new[] { "PerusahaanId", "NomorSertifikat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SertifikatPelatihan_PesertaPelatihanId",
                table: "SertifikatPelatihan",
                column: "PesertaPelatihanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PenyerahanAset");

            migrationBuilder.DropTable(
                name: "RiwayatAset");

            migrationBuilder.DropTable(
                name: "SertifikatPelatihan");

            migrationBuilder.DropTable(
                name: "AsetPerusahaan");

            migrationBuilder.DropTable(
                name: "PesertaPelatihan");

            migrationBuilder.DropTable(
                name: "ProgramPelatihan");
        }
    }
}
