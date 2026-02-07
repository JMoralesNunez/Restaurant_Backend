using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "OrderItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "OrderItems");
        }
    }
}
