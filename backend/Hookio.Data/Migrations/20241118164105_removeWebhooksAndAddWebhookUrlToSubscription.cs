using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Data.Migrations
{
    /// <inheritdoc />
    public partial class removeWebhooksAndAddWebhookUrlToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Webhooks_WebhookId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Webhooks");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_WebhookId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "WebhookId",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<string>(
                name: "GuildId",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Subscriptions",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "WebhookId",
                table: "Subscriptions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    ExpireAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_WebhookId",
                table: "Subscriptions",
                column: "WebhookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Webhooks_WebhookId",
                table: "Subscriptions",
                column: "WebhookId",
                principalTable: "Webhooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
