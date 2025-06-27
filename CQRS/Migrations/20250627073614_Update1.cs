using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQRS.Migrations
{
    /// <inheritdoc />
    public partial class Update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commentaires_Produits_ProduitId1",
                table: "Commentaires");

            migrationBuilder.DropIndex(
                name: "IX_Commentaires_ProduitId1",
                table: "Commentaires");

            migrationBuilder.DropColumn(
                name: "ProduitId1",
                table: "Commentaires");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProduitId1",
                table: "Commentaires",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commentaires_ProduitId1",
                table: "Commentaires",
                column: "ProduitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Commentaires_Produits_ProduitId1",
                table: "Commentaires",
                column: "ProduitId1",
                principalTable: "Produits",
                principalColumn: "Id");
        }
    }
}
