using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemHRD.Migrations
{
    /// <inheritdoc />
    public partial class TambahAuditLogDanPenggunaPerusahaan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: true),
                    PenggunaId = table.Column<Guid>(type: "char(36)", nullable: true),
                    NamaPengguna = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Aksi = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NamaEntitas = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    KunciEntitas = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DataLama = table.Column<string>(type: "longtext", nullable: true),
                    DataBaru = table.Column<string>(type: "longtext", nullable: true),
                    AlamatIP = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Waktu = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Pengguna_PenggunaId",
                        column: x => x.PenggunaId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLog_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PenggunaPerusahaan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    PenggunaId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PerusahaanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PeranDiPerusahaan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatusAktif = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenggunaPerusahaan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PenggunaPerusahaan_Pengguna_PenggunaId",
                        column: x => x.PenggunaId,
                        principalTable: "Pengguna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PenggunaPerusahaan_Perusahaan_PerusahaanId",
                        column: x => x.PerusahaanId,
                        principalTable: "Perusahaan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Aksi",
                table: "AuditLog",
                column: "Aksi");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_PenggunaId",
                table: "AuditLog",
                column: "PenggunaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_PerusahaanId",
                table: "AuditLog",
                column: "PerusahaanId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Waktu",
                table: "AuditLog",
                column: "Waktu");

            migrationBuilder.CreateIndex(
                name: "IX_PenggunaPerusahaan_PenggunaId_PerusahaanId",
                table: "PenggunaPerusahaan",
                columns: new[] { "PenggunaId", "PerusahaanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PenggunaPerusahaan_PerusahaanId",
                table: "PenggunaPerusahaan",
                column: "PerusahaanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "PenggunaPerusahaan");
        }
    }
}
