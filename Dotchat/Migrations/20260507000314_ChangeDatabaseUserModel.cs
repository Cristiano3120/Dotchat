using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotchatServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDatabaseUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerified",
                table: "Users",
                newName: "EmailConfirmed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "Users",
                newName: "EmailVerified");
        }
    }
}
