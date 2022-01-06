using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartMath.Api.Auth.Migrations
{
    public partial class TopicClassificationAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAlgebraTopic",
                table: "Topics",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlgebraTopic",
                table: "Topics");
        }
    }
}
