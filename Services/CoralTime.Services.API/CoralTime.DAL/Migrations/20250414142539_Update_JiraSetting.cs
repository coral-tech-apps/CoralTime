using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Update_JiraSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "JiraSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JiraSettings_Clients_ClientId",
                table: "JiraSettings",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JiraSettings_Clients_ClientId",
                table: "JiraSettings");

            migrationBuilder.DropIndex(
                name: "IX_JiraSettings_ClientId",
                table: "JiraSettings");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "JiraSettings");
        }
    }
}
