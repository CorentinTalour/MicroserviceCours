using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CommentaireService.Migrations
{
    /// <inheritdoc />
    public partial class update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commentaires_Produit_ProduitId",
                table: "Commentaires");

            migrationBuilder.DropTable(
                name: "Produit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Commentaires",
                table: "Commentaires");

            migrationBuilder.DropIndex(
                name: "IX_Commentaires_ProduitId",
                table: "Commentaires");

            migrationBuilder.RenameTable(
                name: "Commentaires",
                newName: "commentaires");

            migrationBuilder.AddPrimaryKey(
                name: "PK_commentaires",
                table: "commentaires",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_commentaires",
                table: "commentaires");

            migrationBuilder.RenameTable(
                name: "commentaires",
                newName: "Commentaires");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Commentaires",
                table: "Commentaires",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Produit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Notable = table.Column<bool>(type: "boolean", nullable: false),
                    Prix = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commentaires_ProduitId",
                table: "Commentaires",
                column: "ProduitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commentaires_Produit_ProduitId",
                table: "Commentaires",
                column: "ProduitId",
                principalTable: "Produit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
