using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEmprendimiento.Migrations
{
    /// <inheritdoc />
    public partial class AddEmprendimientoInventarioRelationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmprendimientoId",
                table: "Inventarios",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios",
                column: "EmprendimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Emprendimientos_EmprendimientoId",
                table: "Inventarios",
                column: "EmprendimientoId",
                principalTable: "Emprendimientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Emprendimientos_EmprendimientoId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_EmprendimientoId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "EmprendimientoId",
                table: "Inventarios");
        }
    }
}
