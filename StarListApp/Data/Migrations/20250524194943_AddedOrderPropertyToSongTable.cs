using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarListApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderPropertyToSongTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Songs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Songs");
        }
    }
}
