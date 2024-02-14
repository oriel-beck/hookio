using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class connectAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Subscriptions_AnnouncementId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AnnouncementId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "AnnouncementId",
                table: "Messages",
                newName: "SubscriptionId");

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AddTimestamp",
                table: "Embeds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionType",
                table: "Subscriptions",
                column: "SubscriptionType");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SubscriptionId",
                table: "Messages",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Subscriptions_SubscriptionId",
                table: "Messages",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Subscriptions_SubscriptionId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_SubscriptionType",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SubscriptionId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AddTimestamp",
                table: "Embeds");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Messages",
                newName: "AnnouncementId");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AnnouncementId",
                table: "Messages",
                column: "AnnouncementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Subscriptions_AnnouncementId",
                table: "Messages",
                column: "AnnouncementId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
