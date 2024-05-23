using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RazorPagesMovie.Migrations
{
    /// <inheritdoc />
    public partial class billid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterbillId",
                table: "Reservations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterbillId",
                table: "Reservations");
        }
    }
}
