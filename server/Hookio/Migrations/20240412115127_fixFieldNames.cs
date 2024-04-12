using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class fixFieldNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "webhookUsername",
                table: "Messages",
                newName: "WebhookUsername");

            migrationBuilder.RenameColumn(
                name: "webhookAvatar",
                table: "Messages",
                newName: "WebhookAvatar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WebhookUsername",
                table: "Messages",
                newName: "webhookUsername");

            migrationBuilder.RenameColumn(
                name: "WebhookAvatar",
                table: "Messages",
                newName: "webhookAvatar");
        }
    }
}
