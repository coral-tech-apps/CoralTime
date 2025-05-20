using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_JiraWorklogId_to_TimeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JiraWorklogId",
                table: "TimeEntries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_JiraWorklogId",
                table: "TimeEntries",
                column: "JiraWorklogId",
                unique: true,
                filter: "JiraWorklogId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_JiraWorklogId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "JiraWorklogId",
                table: "TimeEntries");
        }
    }
}
