using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddJiraProjectIdToTimeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JiraProjectId",
                table: "TimeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_JiraProjectId",
                table: "TimeEntries",
                column: "JiraProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_JiraProjects_JiraProjectId",
                table: "TimeEntries",
                column: "JiraProjectId",
                principalTable: "JiraProjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_JiraProjects_JiraProjectId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_JiraProjectId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "JiraProjectId",
                table: "TimeEntries");
        }
    }
}
