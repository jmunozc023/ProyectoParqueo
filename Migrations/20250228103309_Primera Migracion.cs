using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParqueoApp3.Migrations
{
    /// <inheritdoc />
    public partial class PrimeraMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parqueo",
                columns: table => new
                {
                    id_parqueo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_parqueo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ubicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parqueo", x => x.id_parqueo);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "Espacio",
                columns: table => new
                {
                    id_espacio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_espacio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    disponibilidad = table.Column<bool>(type: "bit", nullable: false),
                    id_parqueo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Espacio", x => x.id_espacio);
                    table.ForeignKey(
                        name: "FK_Espacio_Parqueo_id_parqueo",
                        column: x => x.id_parqueo,
                        principalTable: "Parqueo",
                        principalColumn: "id_parqueo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                columns: table => new
                {
                    id_vehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    placa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tipo_vehiculo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculo", x => x.id_vehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculo_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asig_vehiculos",
                columns: table => new
                {
                    id_asig_vehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_vehiculo = table.Column<int>(type: "int", nullable: false),
                    id_espacio = table.Column<int>(type: "int", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_salida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asig_vehiculos", x => x.id_asig_vehiculo);
                    table.ForeignKey(
                        name: "FK_Asig_vehiculos_Espacio_id_espacio",
                        column: x => x.id_espacio,
                        principalTable: "Espacio",
                        principalColumn: "id_espacio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asig_vehiculos_Vehiculo_id_vehiculo",
                        column: x => x.id_vehiculo,
                        principalTable: "Vehiculo",
                        principalColumn: "id_vehiculo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asig_vehiculos_id_espacio",
                table: "Asig_vehiculos",
                column: "id_espacio");

            migrationBuilder.CreateIndex(
                name: "IX_Asig_vehiculos_id_vehiculo",
                table: "Asig_vehiculos",
                column: "id_vehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Espacio_id_parqueo",
                table: "Espacio",
                column: "id_parqueo");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_id_usuario",
                table: "Vehiculo",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asig_vehiculos");

            migrationBuilder.DropTable(
                name: "Espacio");

            migrationBuilder.DropTable(
                name: "Vehiculo");

            migrationBuilder.DropTable(
                name: "Parqueo");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
