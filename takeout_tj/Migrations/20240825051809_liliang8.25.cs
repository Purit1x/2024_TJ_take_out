using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class liliang825 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DishNum",
                table: "order_dishes",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "order_coupons",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UserCouponDBCouponId",
                table: "order_coupons",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserCouponDBExpirationDate",
                table: "order_coupons",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UserCouponDBUserId",
                table: "order_coupons",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_order_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserCouponDBUserId", "UserCouponDBCouponId", "UserCouponDBExpirationDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_order_coupons_user_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserCouponDBUserId", "UserCouponDBCouponId", "UserCouponDBExpirationDate" },
                principalTable: "user_coupons",
                principalColumns: new[] { "UserId", "CouponId", "ExpirationDate" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_coupons_user_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropIndex(
                name: "IX_order_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "DishNum",
                table: "order_dishes");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "UserCouponDBCouponId",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "UserCouponDBUserId",
                table: "order_coupons");
        }
    }
}
