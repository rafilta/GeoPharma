using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoPharma.Migrations
{
    public partial class RenomearEstabelecimentosParaClientes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Renomeia somente se a tabela antiga ainda existir.
             *
             * Isso também permite concluir com segurança a migration
             * no banco atual, onde a tabela já foi renomeada para Clientes.
             */

            migrationBuilder.Sql(@"
                SET @existe_estabelecimentos =
                (
                    SELECT COUNT(*)
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND table_name = 'Estabelecimentos'
                );

                SET @existe_clientes =
                (
                    SELECT COUNT(*)
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND table_name = 'Clientes'
                );

                SET @comando =
                    IF(
                        @existe_estabelecimentos > 0
                        AND @existe_clientes = 0,
                        'RENAME TABLE `Estabelecimentos` TO `Clientes`',
                        'SELECT 1'
                    );

                PREPARE stmt FROM @comando;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @existe_clientes =
                (
                    SELECT COUNT(*)
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND table_name = 'Clientes'
                );

                SET @existe_estabelecimentos =
                (
                    SELECT COUNT(*)
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND table_name = 'Estabelecimentos'
                );

                SET @comando =
                    IF(
                        @existe_clientes > 0
                        AND @existe_estabelecimentos = 0,
                        'RENAME TABLE `Clientes` TO `Estabelecimentos`',
                        'SELECT 1'
                    );

                PREPARE stmt FROM @comando;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}