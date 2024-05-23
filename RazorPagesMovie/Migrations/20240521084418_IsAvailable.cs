using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RazorPagesMovie.Migrations
{
    /// <inheritdoc />
    public partial class IsAvailable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsAvailable",
                table: "Room",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Room");
        }
    }
}
