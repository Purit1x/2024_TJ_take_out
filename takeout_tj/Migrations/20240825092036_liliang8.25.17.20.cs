using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class liliang8251720 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_coupons_coupons_CouponId",
                table: "order_coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_order_coupons_user_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropIndex(
                name: "IX_order_coupons_CouponId",
                table: "order_coupons");

            migrationBuilder.DropIndex(
                name: "IX_order_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "UserCouponDBCouponId",
                table: "order_coupons");

            migrationBuilder.DropColumn(
                name: "UserCouponDBExpirationDate",
                table: "order_coupons");

            migrationBuilder.RenameColumn(
                name: "UserCouponDBUserId",
                table: "order_coupons",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_order_coupons_UserId_CouponId_ExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserId", "CouponId", "ExpirationDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_order_coupons_user_coupons_UserId_CouponId_ExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserId", "CouponId", "ExpirationDate" },
                principalTable: "user_coupons",
                principalColumns: new[] { "UserId", "CouponId", "ExpirationDate" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_coupons_user_coupons_UserId_CouponId_ExpirationDate",
                table: "order_coupons");

            migrationBuilder.DropIndex(
                name: "IX_order_coupons_UserId_CouponId_ExpirationDate",
                table: "order_coupons");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "order_coupons",
                newName: "UserCouponDBUserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_order_coupons_CouponId",
                table: "order_coupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_order_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserCouponDBUserId", "UserCouponDBCouponId", "UserCouponDBExpirationDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_order_coupons_coupons_CouponId",
                table: "order_coupons",
                column: "CouponId",
                principalTable: "coupons",
                principalColumn: "CouponId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_order_coupons_user_coupons_UserCouponDBUserId_UserCouponDBCouponId_UserCouponDBExpirationDate",
                table: "order_coupons",
                columns: new[] { "UserCouponDBUserId", "UserCouponDBCouponId", "UserCouponDBExpirationDate" },
                principalTable: "user_coupons",
                principalColumns: new[] { "UserId", "CouponId", "ExpirationDate" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
