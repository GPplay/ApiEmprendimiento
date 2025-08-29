using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class AddReporteFinancieroMensualTableAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "ReportesFinancierosMensuales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmprendimientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    TotalGastosFabricacionMes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalGananciasVentasMes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesFinancierosMensuales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesFinancierosMensuales_Emprendimientos_EmprendimientoId",
                        column: x => x.EmprendimientoId,
                        principalTable: "Emprendimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportesFinancierosMensuales_EmprendimientoId",
                table: "ReportesFinancierosMensuales",
                column: "EmprendimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios",
                column: "EmprendimientoId",
                principalTable: "Emprendimientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "ReportesFinancierosMensuales");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios",
                column: "EmprendimientoId",
                principalTable: "Emprendimientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
