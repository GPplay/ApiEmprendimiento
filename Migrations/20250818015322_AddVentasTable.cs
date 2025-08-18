using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Usuarios_UsuarioId",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "EmprendimientoId",
                table: "DetallesVenta");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "DetallesVenta",
                newName: "VentaId");

            migrationBuilder.RenameColumn(
                name: "FechaVenta",
                table: "DetallesVenta",
                newName: "FechaCreacion");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_UsuarioId",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_VentaId");

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaVenta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmprendimientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Emprendimientos_EmprendimientoId",
                        column: x => x.EmprendimientoId,
                        principalTable: "Emprendimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmprendimientoId",
                table: "Ventas",
                column: "EmprendimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.RenameColumn(
                name: "VentaId",
                table: "DetallesVenta",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "DetallesVenta",
                newName: "FechaVenta");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_UsuarioId");

            migrationBuilder.AddColumn<Guid>(
                name: "EmprendimientoId",
                table: "DetallesVenta",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Usuarios_UsuarioId",
                table: "DetallesVenta",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
