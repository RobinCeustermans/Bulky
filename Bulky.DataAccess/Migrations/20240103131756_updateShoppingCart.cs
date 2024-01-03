using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyWeb.DataAcces.Migrations
{
    /// <inheritdoc />
    public partial class updateShoppingCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ShoppingCarts",
                newName: "ShoppingCartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "ShoppingCarts",
                newName: "Id");
        }
    }
}
