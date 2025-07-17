using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInventarioProductoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Productos_ProductoId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Inventarios");

            migrationBuilder.AddColumn<Guid>(
                name: "InventarioId",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Productos_InventarioId",
                table: "Productos",
                column: "InventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Inventarios_InventarioId",
                table: "Productos",
                column: "InventarioId",
                principalTable: "Inventarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Inventarios_InventarioId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_InventarioId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "InventarioId",
                table: "Productos");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoId",
                table: "Inventarios",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Productos_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
