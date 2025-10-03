using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ice.Migrations
{
    /// <inheritdoc />
    public partial class addNavigationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AdminUserId",
                table: "Tickets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdminUserId",
                table: "Tickets",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StudentGroupId",
                table: "Tickets",
                column: "StudentGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AdminUsers_AdminUserId",
                table: "Tickets",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_StudentGroups_StudentGroupId",
                table: "Tickets",
                column: "StudentGroupId",
                principalTable: "StudentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AdminUsers_AdminUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_StudentGroups_StudentGroupId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AdminUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_StudentGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "Tickets");
        }
    }
}
