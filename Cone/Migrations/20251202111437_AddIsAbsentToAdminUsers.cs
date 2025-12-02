using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cone.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAbsentToAdminUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAbsent",
                table: "AdminUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAbsent",
                table: "AdminUsers");
        }
    }
}
