using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ice.Migrations
{
    /// <inheritdoc />
    public partial class fixTicketAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AdminUsers_AdminUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AdminUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAdminUsers_TicketId",
                table: "TicketAdminUsers",
                column: "TicketId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketAdminUsers_TicketId",
                table: "TicketAdminUsers");

            migrationBuilder.AddColumn<long>(
                name: "AdminUserId",
                table: "Tickets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdminUserId",
                table: "Tickets",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AdminUsers_AdminUserId",
                table: "Tickets",
                column: "AdminUserId",
                principalTable: "AdminUsers",
                principalColumn: "Id");
        }
    }
}
