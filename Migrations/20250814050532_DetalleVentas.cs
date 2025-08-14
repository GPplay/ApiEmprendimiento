using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class DetalleVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_Emprendimientos_EmprendimientoId",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios");

            migrationBuilder.RenameTable(
                name: "usuarios",
                newName: "Usuarios");

            migrationBuilder.RenameIndex(
                name: "IX_usuarios_EmprendimientoId",
                table: "Usuarios",
                newName: "IX_Usuarios_EmprendimientoId");

            migrationBuilder.RenameColumn(
                name: "VentaId",
                table: "DetallesVenta",
                newName: "UsuarioId");

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaVenta",
                table: "DetallesVenta",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Usuarios_UsuarioId",
                table: "DetallesVenta",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios",
                column: "EmprendimientoId",
                principalTable: "Emprendimientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Usuarios_UsuarioId",
                table: "DetallesVenta");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Emprendimientos_EmprendimientoId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmprendimientoId",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "FechaVenta",
                table: "DetallesVenta");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuarios");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_EmprendimientoId",
                table: "usuarios",
                newName: "IX_usuarios_EmprendimientoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "DetallesVenta",
                newName: "VentaId");

            migrationBuilder.RenameIndex(
                name: "IX_DetallesVenta_UsuarioId",
                table: "DetallesVenta",
                newName: "IX_DetallesVenta_VentaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios",
                table: "usuarios",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaVenta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId",
                table: "Ventas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_Emprendimientos_EmprendimientoId",
                table: "usuarios",
                column: "EmprendimientoId",
                principalTable: "Emprendimientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
