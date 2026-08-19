using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class UniqueFeedsUrlAndTwitchEventSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WebhookChannel",
                table: "Subscriptions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TwitchBroadcasterId",
                table: "Subscriptions",
                type: "varchar(32)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitchLogin",
                table: "Subscriptions",
                type: "varchar(25)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitchEventSubIds",
                table: "Subscriptions",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TwitchBroadcasterId",
                table: "Subscriptions",
                column: "TwitchBroadcasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Feeds_Url",
                table: "Feeds",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Feeds_Url",
                table: "Feeds");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_TwitchBroadcasterId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TwitchEventSubIds",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TwitchLogin",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TwitchBroadcasterId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "WebhookChannel",
                table: "Subscriptions");
        }
    }
}
