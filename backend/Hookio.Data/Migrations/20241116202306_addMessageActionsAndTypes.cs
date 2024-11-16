using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Data.Migrations
{
    /// <inheritdoc />
    public partial class addMessageActionsAndTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Type_SubscriptionId",
                table: "Messages",
                columns: new[] { "Type", "SubscriptionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_Type_SubscriptionId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Messages");
        }
    }
}
