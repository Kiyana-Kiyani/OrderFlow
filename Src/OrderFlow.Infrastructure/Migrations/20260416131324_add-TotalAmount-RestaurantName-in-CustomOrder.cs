using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addTotalAmountRestaurantNameinCustomOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RestaurantName",
                table: "CustomerOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "CustomerOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestaurantName",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "CustomerOrders");
        }
    }
}
