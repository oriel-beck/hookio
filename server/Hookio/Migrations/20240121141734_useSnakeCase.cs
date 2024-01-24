using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookio.Migrations
{
    /// <inheritdoc />
    public partial class useSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_announcements",
                table: "announcements");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_announcements",
                table: "announcements");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                table: "users",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "expire_at",
                table: "users",
                newName: "ExpireAt");

            migrationBuilder.RenameColumn(
                name: "access_token",
                table: "users",
                newName: "AccessToken");

            migrationBuilder.RenameColumn(
                name: "origin",
                table: "announcements",
                newName: "Origin");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "announcements",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "announcements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "announcements",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "announcement_type",
                table: "announcements",
                newName: "AnnouncementType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_announcements",
                table: "announcements",
                column: "Id");
        }
    }
}
