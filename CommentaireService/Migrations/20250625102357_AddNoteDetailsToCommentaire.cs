using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentaireService.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteDetailsToCommentaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Note",
                table: "commentaires",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "FaculteUtilisation",
                table: "commentaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QualiteProduit",
                table: "commentaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RapportQualitePrix",
                table: "commentaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaculteUtilisation",
                table: "commentaires");

            migrationBuilder.DropColumn(
                name: "QualiteProduit",
                table: "commentaires");

            migrationBuilder.DropColumn(
                name: "RapportQualitePrix",
                table: "commentaires");

            migrationBuilder.AlterColumn<int>(
                name: "Note",
                table: "commentaires",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
