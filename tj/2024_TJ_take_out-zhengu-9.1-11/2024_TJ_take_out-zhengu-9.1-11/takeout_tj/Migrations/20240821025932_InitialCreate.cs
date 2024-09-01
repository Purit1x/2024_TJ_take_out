using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Password = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    CouponId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CouponName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CouponValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CouponPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CouponType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MinPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PeriodOfValidity = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QuantitySold = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsOnShelves = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.CouponId);
                });

            migrationBuilder.CreateTable(
                name: "merchants",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Password = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    MerchantName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    MerchantAddress = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    Contact = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    CouponType = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishType = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    TimeforOpenBusiness = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TimeforCloseBusiness = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Wallet = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    WalletPassword = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchants", x => x.MerchantId);
                });

            migrationBuilder.CreateTable(
                name: "riders",
                columns: table => new
                {
                    RiderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Password = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    RiderName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false),
                    Wallet = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    WalletPassword = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_riders", x => x.RiderId);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    StationId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    StationName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    StationAddress = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stations", x => x.StationId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Wallet = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    WalletPassword = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "dishes",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishName = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    DishPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DishCategory = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ImageUrl = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DishInventory = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dishes", x => new { x.MerchantId, x.DishId });
                    table.ForeignKey(
                        name: "FK_dishes_merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "special_offers",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OfferId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MinPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    AmountRemission = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_special_offers", x => new { x.MerchantId, x.OfferId });
                    table.ForeignKey(
                        name: "FK_special_offers_merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rider_wages",
                columns: table => new
                {
                    WageId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RiderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    WageTimestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Wage = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rider_wages", x => x.WageId);
                    table.ForeignKey(
                        name: "FK_rider_wages_riders_RiderId",
                        column: x => x.RiderId,
                        principalTable: "riders",
                        principalColumn: "RiderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merchant_stations",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    StationId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchant_stations", x => x.MerchantId);
                    table.ForeignKey(
                        name: "FK_merchant_stations_merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_merchant_stations_stations_StationId",
                        column: x => x.StationId,
                        principalTable: "stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rider_stations",
                columns: table => new
                {
                    RiderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    StationId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rider_stations", x => x.RiderId);
                    table.ForeignKey(
                        name: "FK_rider_stations_riders_RiderId",
                        column: x => x.RiderId,
                        principalTable: "riders",
                        principalColumn: "RiderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rider_stations_stations_StationId",
                        column: x => x.StationId,
                        principalTable: "stations",
                        principalColumn: "StationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coupon_purchases",
                columns: table => new
                {
                    CouponPurchaseId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PurchasingTimestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CouponId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PurchasingAmount = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_purchases", x => x.CouponPurchaseId);
                    table.ForeignKey(
                        name: "FK_coupon_purchases_coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_coupon_purchases_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorite_merchants",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorite_merchants", x => new { x.UserId, x.MerchantId });
                    table.ForeignKey(
                        name: "FK_favorite_merchants_merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favorite_merchants_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Level = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Points = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_memberships_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_address",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserAddress = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    HouseNumber = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ContactName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_address", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_user_address_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_coupons",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CouponId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AmountOwned = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_coupons", x => new { x.UserId, x.CouponId, x.ExpirationDate });
                    table.ForeignKey(
                        name: "FK_user_coupons_coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_coupons_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shoppingcarts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ShoppingCartId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishNum = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shoppingcarts", x => new { x.UserId, x.MerchantId, x.DishId, x.ShoppingCartId });
                    table.ForeignKey(
                        name: "FK_shoppingcarts_dishes_MerchantId_DishId",
                        columns: x => new { x.MerchantId, x.DishId },
                        principalTable: "dishes",
                        principalColumns: new[] { "MerchantId", "DishId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shoppingcarts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OrderTimestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpectedTimeOfArrival = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RealTimeOfArrival = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    State = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NeedUtensils = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AddressId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MerchantRating = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RiderRating = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Comment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_orders_user_address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "user_address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_coupons",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CouponId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_coupons", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_order_coupons_coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "coupons",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_coupons_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_dishes",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MerchantId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DishId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_dishes", x => new { x.OrderId, x.MerchantId, x.DishId });
                    table.ForeignKey(
                        name: "FK_order_dishes_dishes_MerchantId_DishId",
                        columns: x => new { x.MerchantId, x.DishId },
                        principalTable: "dishes",
                        principalColumns: new[] { "MerchantId", "DishId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_dishes_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_riders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RiderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RiderPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_riders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_order_riders_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_riders_riders_RiderId",
                        column: x => x.RiderId,
                        principalTable: "riders",
                        principalColumn: "RiderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_users",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_users", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_order_users_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_users_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_purchases_CouponId",
                table: "coupon_purchases",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_purchases_UserId",
                table: "coupon_purchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_favorite_merchants_MerchantId",
                table: "favorite_merchants",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_merchant_stations_StationId",
                table: "merchant_stations",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_order_coupons_CouponId",
                table: "order_coupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_order_dishes_MerchantId_DishId",
                table: "order_dishes",
                columns: new[] { "MerchantId", "DishId" });

            migrationBuilder.CreateIndex(
                name: "IX_order_riders_RiderId",
                table: "order_riders",
                column: "RiderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_users_UserId",
                table: "order_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_AddressId",
                table: "orders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_rider_stations_StationId",
                table: "rider_stations",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_rider_wages_RiderId",
                table: "rider_wages",
                column: "RiderId");

            migrationBuilder.CreateIndex(
                name: "IX_shoppingcarts_MerchantId_DishId",
                table: "shoppingcarts",
                columns: new[] { "MerchantId", "DishId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_address_UserId",
                table: "user_address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_CouponId",
                table: "user_coupons",
                column: "CouponId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "coupon_purchases");

            migrationBuilder.DropTable(
                name: "favorite_merchants");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "merchant_stations");

            migrationBuilder.DropTable(
                name: "order_coupons");

            migrationBuilder.DropTable(
                name: "order_dishes");

            migrationBuilder.DropTable(
                name: "order_riders");

            migrationBuilder.DropTable(
                name: "order_users");

            migrationBuilder.DropTable(
                name: "rider_stations");

            migrationBuilder.DropTable(
                name: "rider_wages");

            migrationBuilder.DropTable(
                name: "shoppingcarts");

            migrationBuilder.DropTable(
                name: "special_offers");

            migrationBuilder.DropTable(
                name: "user_coupons");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "stations");

            migrationBuilder.DropTable(
                name: "riders");

            migrationBuilder.DropTable(
                name: "dishes");

            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "user_address");

            migrationBuilder.DropTable(
                name: "merchants");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
