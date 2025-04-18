using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Update_Uniq_Index_At_JiraProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JiraProjects_JiraProjectId",
                table: "JiraProjects");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_JiraProjectId_JiraSettingId",
                table: "JiraProjects",
                columns: new[] { "JiraProjectId", "JiraSettingId" },
                unique: true,
                filter: "[JiraProjectId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JiraProjects_JiraProjectId_JiraSettingId",
                table: "JiraProjects");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_JiraProjectId",
                table: "JiraProjects",
                column: "JiraProjectId",
                unique: true,
                filter: "[JiraProjectId] IS NOT NULL");
        }
    }
}
