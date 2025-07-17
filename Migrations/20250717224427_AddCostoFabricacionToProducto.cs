using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class AddCostoFabricacionToProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "Productos",
                newName: "PrecioVenta");

            migrationBuilder.AddColumn<decimal>(
                name: "CostoFabricacion",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoFabricacion",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "PrecioVenta",
                table: "Productos",
                newName: "Precio");
        }
    }
}
