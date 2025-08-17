using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class ProductoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Inventarios");

            migrationBuilder.CreateTable(
                name: "InventarioProductos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FechaActualizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioProductos_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventarioProductos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioProductos_InventarioId",
                table: "InventarioProductos",
                column: "InventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioProductos_ProductoId",
                table: "InventarioProductos",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventarioProductos");

            migrationBuilder.AddColumn<Guid>(
                name: "InventarioId",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "Inventarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
