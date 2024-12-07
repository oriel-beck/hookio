using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Data.Migrations
{
    /// <inheritdoc />
    public partial class addWebhookProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookAvatar",
                table: "Subscriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookAvatar",
                table: "Subscriptions");
        }
    }
}
