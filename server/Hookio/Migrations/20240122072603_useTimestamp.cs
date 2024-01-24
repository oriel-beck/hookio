using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class useTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_announcements",
                table: "announcements");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "announcements",
                newName: "Announcements");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                table: "Users",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "expire_at",
                table: "Users",
                newName: "ExpireAt");

            migrationBuilder.RenameColumn(
                name: "access_token",
                table: "Users",
                newName: "AccessToken");

            migrationBuilder.RenameColumn(
                name: "origin",
                table: "Announcements",
                newName: "Origin");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Announcements",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Announcements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "Announcements",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "announcement_type",
                table: "Announcements",
                newName: "AnnouncementType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Announcements",
                newName: "announcements");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "users",
                newName: "refresh_token");

            migrationBuilder.RenameColumn(
                name: "ExpireAt",
                table: "users",
                newName: "expire_at");

            migrationBuilder.RenameColumn(
                name: "AccessToken",
                table: "users",
                newName: "access_token");

            migrationBuilder.RenameColumn(
                name: "Origin",
                table: "announcements",
                newName: "origin");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "announcements",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "announcements",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "announcements",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "AnnouncementType",
                table: "announcements",
                newName: "announcement_type");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_announcements",
                table: "announcements",
                column: "id");
        }
    }
}
