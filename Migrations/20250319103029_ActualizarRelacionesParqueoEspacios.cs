using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParqueoApp3.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarRelacionesParqueoEspacios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Parqueoid_parqueo",
                table: "Espacio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Espacio_Parqueoid_parqueo",
                table: "Espacio",
                column: "Parqueoid_parqueo");

            migrationBuilder.AddForeignKey(
                name: "FK_Espacio_Parqueo_Parqueoid_parqueo",
                table: "Espacio",
                column: "Parqueoid_parqueo",
                principalTable: "Parqueo",
                principalColumn: "id_parqueo",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Espacio_Parqueo_Parqueoid_parqueo",
                table: "Espacio");

            migrationBuilder.DropIndex(
                name: "IX_Espacio_Parqueoid_parqueo",
                table: "Espacio");

            migrationBuilder.DropColumn(
                name: "Parqueoid_parqueo",
                table: "Espacio");
        }
    }
}
