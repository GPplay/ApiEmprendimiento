using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarRelacionEmprendimientoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios",
                column: "EmprendimientoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios",
                column: "EmprendimientoId");
        }
    }
}
