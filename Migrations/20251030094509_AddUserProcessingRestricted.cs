using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvendeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProcessingRestricted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProcessingRestricted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProcessingRestricted",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingRestricted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProcessingRestricted",
                table: "UserRoles");
        }
    }
}
