using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RazorPagesMovie.Migrations
{
    /// <inheritdoc />
    public partial class selectedRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedRooms",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedRooms",
                table: "Reservations");
        }
    }
}
