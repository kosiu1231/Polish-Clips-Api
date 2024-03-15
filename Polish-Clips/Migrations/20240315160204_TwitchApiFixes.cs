using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polish_Clips.Migrations
{
    /// <inheritdoc />
    public partial class TwitchApiFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Duration",
                table: "Clips",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "TwitchId",
                table: "Clips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwitchId",
                table: "Clips");

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "Clips",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
