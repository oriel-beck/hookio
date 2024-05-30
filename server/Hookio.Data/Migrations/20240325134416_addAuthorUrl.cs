using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class addAuthorUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Embeds",
                newName: "TitleUrl");

            migrationBuilder.AddColumn<string>(
                name: "AuthorUrl",
                table: "Embeds",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorUrl",
                table: "Embeds");

            migrationBuilder.RenameColumn(
                name: "TitleUrl",
                table: "Embeds",
                newName: "Url");
        }
    }
}
