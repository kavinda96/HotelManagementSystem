using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RazorPagesMovie.Migrations
{
    /// <inheritdoc />
    public partial class NewColsfoodbev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FoodCode",
                table: "Food",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BeverageCode",
                table: "Beverage",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodCode",
                table: "Food");

            migrationBuilder.DropColumn(
                name: "BeverageCode",
                table: "Beverage");
        }
    }
}
