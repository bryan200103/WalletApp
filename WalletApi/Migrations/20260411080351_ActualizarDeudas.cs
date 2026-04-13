using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarDeudas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeudasMensuales_Usuarios_UsuarioId",
                table: "DeudasMensuales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeudasMensuales",
                table: "DeudasMensuales");

            migrationBuilder.DropColumn(
                name: "AnioActual",
                table: "DeudasMensuales");

            migrationBuilder.DropColumn(
                name: "EsActiva",
                table: "DeudasMensuales");

            migrationBuilder.DropColumn(
                name: "EstaPagado",
                table: "DeudasMensuales");

            migrationBuilder.DropColumn(
                name: "Monto",
                table: "DeudasMensuales");

            migrationBuilder.RenameTable(
                name: "DeudasMensuales",
                newName: "Deudas");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Deudas",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "MesActual",
                table: "Deudas",
                newName: "CuotasPagadas");

            migrationBuilder.RenameColumn(
                name: "DiaVencimiento",
                table: "Deudas",
                newName: "CantidadCuotas");

            migrationBuilder.RenameIndex(
                name: "IX_DeudasMensuales_UsuarioId",
                table: "Deudas",
                newName: "IX_Deudas_UsuarioId");

            migrationBuilder.AddColumn<string>(
                name: "Acreedor",
                table: "Deudas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "Deudas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "MontoCuota",
                table: "Deudas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "Deudas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximoVencimiento",
                table: "Deudas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deudas",
                table: "Deudas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deudas_Usuarios_UsuarioId",
                table: "Deudas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deudas_Usuarios_UsuarioId",
                table: "Deudas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deudas",
                table: "Deudas");

            migrationBuilder.DropColumn(
                name: "Acreedor",
                table: "Deudas");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Deudas");

            migrationBuilder.DropColumn(
                name: "MontoCuota",
                table: "Deudas");

            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "Deudas");

            migrationBuilder.DropColumn(
                name: "ProximoVencimiento",
                table: "Deudas");

            migrationBuilder.RenameTable(
                name: "Deudas",
                newName: "DeudasMensuales");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "DeudasMensuales",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "CuotasPagadas",
                table: "DeudasMensuales",
                newName: "MesActual");

            migrationBuilder.RenameColumn(
                name: "CantidadCuotas",
                table: "DeudasMensuales",
                newName: "DiaVencimiento");

            migrationBuilder.RenameIndex(
                name: "IX_Deudas_UsuarioId",
                table: "DeudasMensuales",
                newName: "IX_DeudasMensuales_UsuarioId");

            migrationBuilder.AddColumn<int>(
                name: "AnioActual",
                table: "DeudasMensuales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsActiva",
                table: "DeudasMensuales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EstaPagado",
                table: "DeudasMensuales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Monto",
                table: "DeudasMensuales",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeudasMensuales",
                table: "DeudasMensuales",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeudasMensuales_Usuarios_UsuarioId",
                table: "DeudasMensuales",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
