using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_onetomany_Relation_Cliet_JiraSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings");

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings");

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings",
                column: "ClientId",
                unique: true);
        }
    }
}
