using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_Tables_For_JiraProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SettingName",
                table: "JiraSettings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "JiraProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JiraProjectId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JiraSettingId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JiraProjects_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraProjects_AspNetUsers_LastEditorUserId",
                        column: x => x.LastEditorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraProjects_JiraSettings_JiraSettingId",
                        column: x => x.JiraSettingId,
                        principalTable: "JiraSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinkedJiraProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    JiraProjectId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedJiraProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkedJiraProjects_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LinkedJiraProjects_AspNetUsers_LastEditorUserId",
                        column: x => x.LastEditorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LinkedJiraProjects_JiraProjects_JiraProjectId",
                        column: x => x.JiraProjectId,
                        principalTable: "JiraProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LinkedJiraProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_SettingName",
                table: "JiraSettings",
                column: "SettingName",
                unique: true,
                filter: "[SettingName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_CreatorId",
                table: "JiraProjects",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_JiraProjectId",
                table: "JiraProjects",
                column: "JiraProjectId",
                unique: true,
                filter: "[JiraProjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_JiraSettingId",
                table: "JiraProjects",
                column: "JiraSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraProjects_LastEditorUserId",
                table: "JiraProjects",
                column: "LastEditorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedJiraProjects_CreatorId",
                table: "LinkedJiraProjects",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedJiraProjects_JiraProjectId",
                table: "LinkedJiraProjects",
                column: "JiraProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedJiraProjects_LastEditorUserId",
                table: "LinkedJiraProjects",
                column: "LastEditorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedJiraProjects_ProjectId",
                table: "LinkedJiraProjects",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkedJiraProjects");

            migrationBuilder.DropTable(
                name: "JiraProjects");

            migrationBuilder.DropIndex(
                name: "IX_JiraSettings_SettingName",
                table: "JiraSettings");

            migrationBuilder.AlterColumn<string>(
                name: "SettingName",
                table: "JiraSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
