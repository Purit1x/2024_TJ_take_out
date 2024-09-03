using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class AlterOrderRiders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_riders_riders_RiderId",
                table: "order_riders");

            migrationBuilder.AlterColumn<int>(
                name: "RiderId",
                table: "order_riders",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AddForeignKey(
                name: "FK_order_riders_riders_RiderId",
                table: "order_riders",
                column: "RiderId",
                principalTable: "riders",
                principalColumn: "RiderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_riders_riders_RiderId",
                table: "order_riders");

            migrationBuilder.AlterColumn<int>(
                name: "RiderId",
                table: "order_riders",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_order_riders_riders_RiderId",
                table: "order_riders",
                column: "RiderId",
                principalTable: "riders",
                principalColumn: "RiderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
