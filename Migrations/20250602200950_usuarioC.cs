using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class usuarioC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contrasena",
                table: "usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contrasena",
                table: "usuarios");
        }
    }
}
