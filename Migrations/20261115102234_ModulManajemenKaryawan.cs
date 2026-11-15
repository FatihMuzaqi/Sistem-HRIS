using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class ModulManajemenKaryawan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departemen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KodeDepartemen = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    NamaDepartemen = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Deskripsi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    KepalaDepartemenId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_Departemen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departemen_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jabatan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaJabatan = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DepartemenId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Deskripsi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    LevelJabatan = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Jabatan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jabatan_Departemen_DepartemenId",
                        column: x => x.DepartemenId,
                        principalTable: "Departemen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jabatan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Karyawan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorIndukKaryawan = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    NamaLengkap = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    TempatLahir = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JenisKelamin = table.Column<int>(type: "int", nullable: false),
                    Alamat = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    NomorTelepon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    TanggalMasuk = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepartemenId = table.Column<Guid>(type: "char(36)", nullable: false),
                    JabatanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StatusKaryawan = table.Column<int>(type: "int", nullable: false),
                    FotoProfil = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    PenggunaId = table.Column<Guid>(type: "char(36)", nullable: true),
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
                    table.PrimaryKey("PK_Karyawan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Karyawan_Departemen_DepartemenId",
                        column: x => x.DepartemenId,
                        principalTable: "Departemen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Karyawan_Jabatan_JabatanId",
                        column: x => x.JabatanId,
                        principalTable: "Jabatan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Karyawan_Pengguna_PenggunaId",
                        column: x => x.PenggunaId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Karyawan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DokumenKaryawan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaDokumen = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    JenisDokumen = table.Column<int>(type: "int", nullable: false),
                    NamaFile = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    UkuranFile = table.Column<long>(type: "bigint", nullable: false),
                    TipeFile = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PathFile = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_DokumenKaryawan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DokumenKaryawan_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DokumenKaryawan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KontakDaruratKaryawan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaLengkap = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Hubungan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NomorTelepon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Alamat = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_KontakDaruratKaryawan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KontakDaruratKaryawan_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KontakDaruratKaryawan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KontrakKerja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NomorKontrak = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    JenisKontrak = table.Column<int>(type: "int", nullable: false),
                    TanggalMulai = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TanggalBerakhir = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StatusKontrak = table.Column<int>(type: "int", nullable: false),
                    Keterangan = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    DokumenUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_KontrakKerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KontrakKerja_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KontrakKerja_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RiwayatPekerjaan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    KaryawanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    NamaPerusahaanSebelumnya = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    JabatanTerakhir = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    TahunMulai = table.Column<int>(type: "int", nullable: false),
                    TahunSelesai = table.Column<int>(type: "int", nullable: false),
                    DeskripsiTugas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RiwayatPekerjaan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiwayatPekerjaan_Karyawan_KaryawanId",
                        column: x => x.KaryawanId,
                        principalTable: "Karyawan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiwayatPekerjaan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Departemen_KepalaDepartemenId",
                table: "Departemen",
                column: "KepalaDepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_Departemen_PerusahaanId_KodeDepartemen",
                table: "Departemen",
                columns: new[] { "PerusahaanId", "KodeDepartemen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DokumenKaryawan_KaryawanId",
                table: "DokumenKaryawan",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_DokumenKaryawan_PerusahaanId_KaryawanId",
                table: "DokumenKaryawan",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Jabatan_DepartemenId",
                table: "Jabatan",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_Jabatan_PerusahaanId_DepartemenId_NamaJabatan",
                table: "Jabatan",
                columns: new[] { "PerusahaanId", "DepartemenId", "NamaJabatan" });

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_DepartemenId",
                table: "Karyawan",
                column: "DepartemenId");

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_JabatanId",
                table: "Karyawan",
                column: "JabatanId");

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_PenggunaId",
                table: "Karyawan",
                column: "PenggunaId");

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_PerusahaanId_DepartemenId",
                table: "Karyawan",
                columns: new[] { "PerusahaanId", "DepartemenId" });

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_PerusahaanId_JabatanId",
                table: "Karyawan",
                columns: new[] { "PerusahaanId", "JabatanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_PerusahaanId_NomorIndukKaryawan",
                table: "Karyawan",
                columns: new[] { "PerusahaanId", "NomorIndukKaryawan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Karyawan_PerusahaanId_StatusKaryawan",
                table: "Karyawan",
                columns: new[] { "PerusahaanId", "StatusKaryawan" });

            migrationBuilder.CreateIndex(
                name: "IX_KontakDaruratKaryawan_KaryawanId",
                table: "KontakDaruratKaryawan",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_KontakDaruratKaryawan_PerusahaanId_KaryawanId",
                table: "KontakDaruratKaryawan",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_KontrakKerja_KaryawanId",
                table: "KontrakKerja",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_KontrakKerja_PerusahaanId_KaryawanId",
                table: "KontrakKerja",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatPekerjaan_KaryawanId",
                table: "RiwayatPekerjaan",
                column: "KaryawanId");

            migrationBuilder.CreateIndex(
                name: "IX_RiwayatPekerjaan_PerusahaanId_KaryawanId",
                table: "RiwayatPekerjaan",
                columns: new[] { "PerusahaanId", "KaryawanId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Departemen_Karyawan_KepalaDepartemenId",
                table: "Departemen",
                column: "KepalaDepartemenId",
                principalTable: "Karyawan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departemen_Karyawan_KepalaDepartemenId",
                table: "Departemen");

            migrationBuilder.DropTable(
                name: "DokumenKaryawan");

            migrationBuilder.DropTable(
                name: "KontakDaruratKaryawan");

            migrationBuilder.DropTable(
                name: "KontrakKerja");

            migrationBuilder.DropTable(
                name: "RiwayatPekerjaan");

            migrationBuilder.DropTable(
                name: "Karyawan");

            migrationBuilder.DropTable(
                name: "Jabatan");

            migrationBuilder.DropTable(
                name: "Departemen");
        }
    }
}
