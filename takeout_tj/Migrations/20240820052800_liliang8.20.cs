using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class liliang820 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_address",
                table: "user_address");

            migrationBuilder.RenameColumn(
                name: "PaymentState",
                table: "coupon_purchases",
                newName: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "user_address",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0)
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "user_address",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "user_address",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "user_address",
                type: "NVARCHAR2(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IsOnShelves",
                table: "coupons",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_address",
                table: "user_address",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_user_address_UserId",
                table: "user_address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_purchases_UserId",
                table: "coupon_purchases",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_coupon_purchases_users_UserId",
                table: "coupon_purchases",
                column: "UserId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coupon_purchases_users_UserId",
                table: "coupon_purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_address",
                table: "user_address");

            migrationBuilder.DropIndex(
                name: "IX_user_address_UserId",
                table: "user_address");

            migrationBuilder.DropIndex(
                name: "IX_coupon_purchases_UserId",
                table: "coupon_purchases");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "IsOnShelves",
                table: "coupons");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "coupon_purchases",
                newName: "PaymentState");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_address",
                table: "user_address",
                columns: new[] { "UserId", "UserAddress" });
        }
    }
}
