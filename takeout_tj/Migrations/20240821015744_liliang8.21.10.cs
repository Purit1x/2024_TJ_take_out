using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace takeout_tj.Migrations
{
    public partial class liliang82110 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_merchant_stations",
                table: "merchant_stations");

            migrationBuilder.DropIndex(
                name: "IX_merchant_stations_MerchantId",
                table: "merchant_stations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchant_stations",
                table: "merchant_stations",
                column: "MerchantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_merchant_stations",
                table: "merchant_stations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchant_stations",
                table: "merchant_stations",
                columns: new[] { "MerchantId", "StationId" });

            migrationBuilder.CreateIndex(
                name: "IX_merchant_stations_MerchantId",
                table: "merchant_stations",
                column: "MerchantId",
                unique: true);
        }
    }
}
