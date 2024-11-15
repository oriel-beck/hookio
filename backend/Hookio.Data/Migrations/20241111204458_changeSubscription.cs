using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Embed",
                table: "Messages",
                newName: "Embeds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Embeds",
                table: "Messages",
                newName: "Embed");
        }
    }
}
