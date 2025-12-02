using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cone.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentGroupIdToTicketAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketAssignments_TicketId_AssignmentId",
                table: "TicketAssignments");

            migrationBuilder.AddColumn<long>(
                name: "StudentGroupId",
                table: "TicketAssignments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignments_StudentGroupId",
                table: "TicketAssignments",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignments_TicketId_AssignmentId_StudentGroupId",
                table: "TicketAssignments",
                columns: new[] { "TicketId", "AssignmentId", "StudentGroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAssignments_StudentGroups_StudentGroupId",
                table: "TicketAssignments",
                column: "StudentGroupId",
                principalTable: "StudentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAssignments_StudentGroups_StudentGroupId",
                table: "TicketAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TicketAssignments_StudentGroupId",
                table: "TicketAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TicketAssignments_TicketId_AssignmentId_StudentGroupId",
                table: "TicketAssignments");

            migrationBuilder.DropColumn(
                name: "StudentGroupId",
                table: "TicketAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignments_TicketId_AssignmentId",
                table: "TicketAssignments",
                columns: new[] { "TicketId", "AssignmentId" },
                unique: true);
        }
    }
}
