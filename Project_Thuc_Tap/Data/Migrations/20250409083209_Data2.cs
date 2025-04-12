using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_Thuc_Tap.Data.Migrations
{
    /// <inheritdoc />
    public partial class Data2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOverTime",
                table: "TimeKeeping",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOverTime",
                table: "TimeKeeping");
        }
    }
}
