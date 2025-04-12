using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_Thuc_Tap.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DetailReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DetailReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
