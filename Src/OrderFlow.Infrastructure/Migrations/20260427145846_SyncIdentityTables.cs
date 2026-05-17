using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "OrderItems");
        }
    }
}
