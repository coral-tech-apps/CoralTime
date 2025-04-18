using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoralTime.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_JiraSettings_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableJira",
                table: "Members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "JiraSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JiraSettings_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraSettings_AspNetUsers_LastEditorUserId",
                        column: x => x.LastEditorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JiraMemberSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JiraUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JiraSettingId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditorUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraMemberSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JiraMemberSettings_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraMemberSettings_AspNetUsers_LastEditorUserId",
                        column: x => x.LastEditorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraMemberSettings_JiraSettings_JiraSettingId",
                        column: x => x.JiraSettingId,
                        principalTable: "JiraSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JiraMemberSettings_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JiraMemberSettings_CreatorId",
                table: "JiraMemberSettings",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraMemberSettings_JiraSettingId",
                table: "JiraMemberSettings",
                column: "JiraSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraMemberSettings_LastEditorUserId",
                table: "JiraMemberSettings",
                column: "LastEditorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraMemberSettings_MemberId",
                table: "JiraMemberSettings",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_CreatorId",
                table: "JiraSettings",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_JiraSettings_LastEditorUserId",
                table: "JiraSettings",
                column: "LastEditorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JiraMemberSettings");

            migrationBuilder.DropTable(
                name: "JiraSettings");

            migrationBuilder.DropColumn(
                name: "EnableJira",
                table: "Members");
        }
    }
}
