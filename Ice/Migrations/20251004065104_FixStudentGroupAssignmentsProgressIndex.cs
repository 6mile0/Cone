using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ice.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentGroupAssignmentsProgressIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentGroupAssignmentsProgress_AssignmentId",
                table: "StudentGroupAssignmentsProgress");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroupAssignmentsProgress_AssignmentId",
                table: "StudentGroupAssignmentsProgress",
                column: "AssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentGroupAssignmentsProgress_AssignmentId",
                table: "StudentGroupAssignmentsProgress");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroupAssignmentsProgress_AssignmentId",
                table: "StudentGroupAssignmentsProgress",
                column: "AssignmentId",
                unique: true);
        }
    }
}
