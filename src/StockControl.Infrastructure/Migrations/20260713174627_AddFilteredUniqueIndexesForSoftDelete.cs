using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexesForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_Email",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_produtos_Codigo",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores");

            migrationBuilder.DropIndex(
                name: "IX_entregadores_cpf",
                table: "entregadores");

            migrationBuilder.DropIndex(
                name: "IX_clientes_cpf",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "IX_categorias_Nome",
                table: "categorias");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_Codigo",
                table: "produtos",
                column: "Codigo",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores",
                column: "cnpj",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_entregadores_cpf",
                table: "entregadores",
                column: "cpf",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_cpf",
                table: "clientes",
                column: "cpf",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_Nome",
                table: "categorias",
                column: "Nome",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_Email",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_produtos_Codigo",
                table: "produtos");

            migrationBuilder.DropIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores");

            migrationBuilder.DropIndex(
                name: "IX_entregadores_cpf",
                table: "entregadores");

            migrationBuilder.DropIndex(
                name: "IX_clientes_cpf",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "IX_categorias_Nome",
                table: "categorias");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_produtos_Codigo",
                table: "produtos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entregadores_cpf",
                table: "entregadores",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientes_cpf",
                table: "clientes",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categorias_Nome",
                table: "categorias",
                column: "Nome",
                unique: true);
        }
    }
}
