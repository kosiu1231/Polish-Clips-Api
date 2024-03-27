using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polish_Clips.Migrations
{
    /// <inheritdoc />
    public partial class CommentAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentAmount",
                table: "Clips",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentAmount",
                table: "Clips");
        }
    }
}
