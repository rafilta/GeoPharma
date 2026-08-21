using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoPharma.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaEstruturaCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estabelecimentos_Regioes_RegiaoId",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "LatitudeCentro",
                table: "Regioes");

            migrationBuilder.DropColumn(
                name: "LongitudeCentro",
                table: "Regioes");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Estabelecimentos");

            migrationBuilder.RenameColumn(
                name: "NomeRegiao",
                table: "Regioes",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "CNPJ",
                table: "Estabelecimentos",
                newName: "Cnpj");

            migrationBuilder.RenameColumn(
                name: "CEP",
                table: "Estabelecimentos",
                newName: "Cep");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Regioes",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "RegiaoId",
                table: "Estabelecimentos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Estabelecimentos",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(11,8)");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Estabelecimentos",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,8)");

            migrationBuilder.AlterColumn<string>(
                name: "Cidade",
                table: "Estabelecimentos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(80)",
                oldMaxLength: 80)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Estabelecimentos",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(18)",
                oldMaxLength: 18,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Cep",
                table: "Estabelecimentos",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "Estabelecimentos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(80)",
                oldMaxLength: 80)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Estabelecimentos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Estabelecimentos",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "Estabelecimentos",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Estabelecimentos",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Estabelecimentos_Regioes_RegiaoId",
                table: "Estabelecimentos",
                column: "RegiaoId",
                principalTable: "Regioes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estabelecimentos_Regioes_RegiaoId",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Regioes");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "Estabelecimentos");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Estabelecimentos");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Regioes",
                newName: "NomeRegiao");

            migrationBuilder.RenameColumn(
                name: "Cnpj",
                table: "Estabelecimentos",
                newName: "CNPJ");

            migrationBuilder.RenameColumn(
                name: "Cep",
                table: "Estabelecimentos",
                newName: "CEP");

            migrationBuilder.AddColumn<decimal>(
                name: "LatitudeCentro",
                table: "Regioes",
                type: "decimal(10,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LongitudeCentro",
                table: "Regioes",
                type: "decimal(11,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "RegiaoId",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Estabelecimentos",
                type: "decimal(11,8)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Estabelecimentos",
                type: "decimal(10,8)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CNPJ",
                table: "Estabelecimentos",
                type: "varchar(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Estabelecimentos",
                keyColumn: "Cidade",
                keyValue: null,
                column: "Cidade",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Cidade",
                table: "Estabelecimentos",
                type: "varchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Estabelecimentos",
                keyColumn: "CEP",
                keyValue: null,
                column: "CEP",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "Estabelecimentos",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Estabelecimentos",
                keyColumn: "Bairro",
                keyValue: null,
                column: "Bairro",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "Estabelecimentos",
                type: "varchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Estabelecimentos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Estabelecimentos",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Estabelecimentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Estabelecimentos_Regioes_RegiaoId",
                table: "Estabelecimentos",
                column: "RegiaoId",
                principalTable: "Regioes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
