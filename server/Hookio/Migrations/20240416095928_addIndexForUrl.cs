using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class addIndexForUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Url",
                table: "Subscriptions",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_Url",
                table: "Subscriptions");
        }
    }
}
