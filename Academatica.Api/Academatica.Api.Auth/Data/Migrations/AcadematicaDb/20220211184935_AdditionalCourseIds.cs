using Microsoft.EntityFrameworkCore.Migrations;

namespace Academatica.Api.Auth.Data.Migrations.AcadematicaDb
{
    public partial class AdditionalCourseIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TierId",
                table: "Topics",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TierId",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopicId",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_TierId",
                table: "Topics",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TierId",
                table: "Classes",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TopicId",
                table: "Classes",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Tiers_TierId",
                table: "Classes",
                column: "TierId",
                principalTable: "Tiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Topics_TopicId",
                table: "Classes",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Tiers_TierId",
                table: "Topics",
                column: "TierId",
                principalTable: "Tiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Tiers_TierId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Topics_TopicId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Tiers_TierId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_TierId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Classes_TierId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_TopicId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TierId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "TierId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Classes");
        }
    }
}
