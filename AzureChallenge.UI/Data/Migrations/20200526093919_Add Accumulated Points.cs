using Microsoft.EntityFrameworkCore.Migrations;

namespace AzureChallenge.UI.Data.Migrations
{
    public partial class AddAccumulatedPoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccumulatedPoint",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccumulatedPoint",
                table: "AspNetUsers");
        }
    }
}
