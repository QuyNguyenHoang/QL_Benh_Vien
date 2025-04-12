using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_Thuc_Tap.Data.Migrations
{
    /// <inheritdoc />
    public partial class DB4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DetailReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailReports_UserId",
                table: "DetailReports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailReports_Users_UserId",
                table: "DetailReports");

            migrationBuilder.DropIndex(
                name: "IX_DetailReports_UserId",
                table: "DetailReports");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "DetailReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
