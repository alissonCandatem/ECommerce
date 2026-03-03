using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Pedidos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePedidoItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItens_pedidos_PedidoId",
                table: "PedidoItens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PedidoItens",
                table: "PedidoItens");

            migrationBuilder.RenameTable(
                name: "PedidoItens",
                newName: "pedido_itens");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "pedido_itens",
                newName: "quantidade");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "pedido_itens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProdutoId",
                table: "pedido_itens",
                newName: "produto_id");

            migrationBuilder.RenameColumn(
                name: "PrecoUnitario",
                table: "pedido_itens",
                newName: "preco_unitario");

            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "pedido_itens",
                newName: "pedido_id");

            migrationBuilder.RenameColumn(
                name: "NomeProduto",
                table: "pedido_itens",
                newName: "nome_produto");

            migrationBuilder.RenameIndex(
                name: "IX_PedidoItens_PedidoId",
                table: "pedido_itens",
                newName: "IX_pedido_itens_pedido_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "preco_unitario",
                table: "pedido_itens",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "nome_produto",
                table: "pedido_itens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pedido_itens",
                table: "pedido_itens",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_pedido_itens_pedidos_pedido_id",
                table: "pedido_itens",
                column: "pedido_id",
                principalTable: "pedidos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pedido_itens_pedidos_pedido_id",
                table: "pedido_itens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pedido_itens",
                table: "pedido_itens");

            migrationBuilder.RenameTable(
                name: "pedido_itens",
                newName: "PedidoItens");

            migrationBuilder.RenameColumn(
                name: "quantidade",
                table: "PedidoItens",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PedidoItens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "produto_id",
                table: "PedidoItens",
                newName: "ProdutoId");

            migrationBuilder.RenameColumn(
                name: "preco_unitario",
                table: "PedidoItens",
                newName: "PrecoUnitario");

            migrationBuilder.RenameColumn(
                name: "pedido_id",
                table: "PedidoItens",
                newName: "PedidoId");

            migrationBuilder.RenameColumn(
                name: "nome_produto",
                table: "PedidoItens",
                newName: "NomeProduto");

            migrationBuilder.RenameIndex(
                name: "IX_pedido_itens_pedido_id",
                table: "PedidoItens",
                newName: "IX_PedidoItens_PedidoId");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoUnitario",
                table: "PedidoItens",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "NomeProduto",
                table: "PedidoItens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PedidoItens",
                table: "PedidoItens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItens_pedidos_PedidoId",
                table: "PedidoItens",
                column: "PedidoId",
                principalTable: "pedidos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
