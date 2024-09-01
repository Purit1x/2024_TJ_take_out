using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;
using takeout_tj.Models.Merchant;
using takeout_tj.Models.Platform;
using takeout_tj.Models.Rider;
using takeout_tj.Models.User;

namespace takeout_tj.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<UserDB> Users { get; set; }
		public DbSet<UserAddressDB> UserAddresses { get; set; }
		public DbSet<MembershipDB> Memberships { get; set; }
		public DbSet<MerchantDB> Merchants { get; set; }
		public DbSet<FavoriteMerchantDB> FavoriteMerchants { get; set; }
		public DbSet<DishDB> Dishes { get; set; }
		public DbSet<ShoppingCartDB> ShoppingCarts { get; set; }
		public DbSet<CouponDB> Coupons { get; set; }
		public DbSet<CouponPurchaseDB> CouponPurchases { get; set; }
		public DbSet<UserCouponDB> UserCoupons { get; set; }
		public DbSet<RiderDB> Riders { get; set; }
		public DbSet<RiderWageDB> RiderWages { get; set; }
		public DbSet<StationDB> Stations { get; set; }
		public DbSet<RiderStationDB> RiderStations { get; set; }
		public DbSet<SpecialOfferDB> SpecialOffers { get; set; }
		public DbSet<OrderDB> Orders { get; set; }
		public DbSet<OrderRiderDB> OrderRiders { get; set; }
		public DbSet<OrderUserDB> OrderUsers { get; set; }
		public DbSet<OrderDishDB> OrderDishes { get; set; }
		public DbSet<OrderCouponDB> OrderCoupons { get; set; }
		public DbSet<AdminDB> Admins { get; set; }
		public DbSet<MerchantStationDB> MerchantStations { get; set; }
		public DbSet<UserDefaultAddressDB> UserDefaultAddresses { get; set; }
		
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
		{
			
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// modelBuilder.HasDefaultSchema("C##TAKEOUT");

			modelBuilder.Entity<UserDB>().HasKey(u => u.UserId);
			modelBuilder.Entity<UserAddressDB>().HasKey(u => u.AddressId);
			modelBuilder.Entity<MerchantDB>().HasKey(m => m.MerchantId);
			modelBuilder.Entity<FavoriteMerchantDB>().HasKey(um => new {um.UserId, um.MerchantId});
			modelBuilder.Entity<DishDB>().HasKey(d => new { d.MerchantId, d.DishId });
			modelBuilder.Entity<ShoppingCartDB>().HasKey(s => new {s.UserId, s.MerchantId, s.DishId, s.ShoppingCartId});
			modelBuilder.Entity<CouponDB>().HasKey(c => c.CouponId);
			modelBuilder.Entity<CouponPurchaseDB>().HasKey(c => c.CouponPurchaseId);
			modelBuilder.Entity<UserCouponDB>().HasKey(u => new { u.UserId, u.CouponId, u.ExpirationDate });
			modelBuilder.Entity<RiderDB>().HasKey(r => r.RiderId);
			modelBuilder.Entity<RiderWageDB>().HasKey(r => r.WageId);
			modelBuilder.Entity<StationDB>().HasKey(s =>s.StationId);
			modelBuilder.Entity<RiderStationDB>().HasKey(rs => rs.RiderId);
			modelBuilder.Entity<SpecialOfferDB>().HasKey(s => new { s.MerchantId, s.OfferId });
			modelBuilder.Entity<OrderDB>().HasKey(o => o.OrderId);
			modelBuilder.Entity<OrderRiderDB>().HasKey(o => o.OrderId);
			modelBuilder.Entity<OrderUserDB>().HasKey(ou => ou.OrderId);
			modelBuilder.Entity<OrderDishDB>().HasKey(od => new { od.OrderId, od.MerchantId, od.DishId });
			modelBuilder.Entity<OrderCouponDB>().HasKey(oc => oc.OrderId);
			modelBuilder.Entity<AdminDB>().HasKey(a => a.AdminId);
			modelBuilder.Entity<MerchantStationDB>().HasKey(ms => ms.MerchantId);
			modelBuilder.Entity<MembershipDB>().HasKey(m => m.UserId);
			modelBuilder.Entity<UserDefaultAddressDB>().HasKey(uda => uda.AddressId);

			// 定义地址到用户的多对一关系
			modelBuilder.Entity<UserAddressDB>()
				.HasOne(a => a.UserDB)
				.WithMany(u => u.UserAddressDBs)
				.HasForeignKey(a => a.UserId);
			// 会员身份与用户的一对一关系
			modelBuilder.Entity<MembershipDB>()
				.HasOne(m => m.UserDB)
				.WithOne()
				.HasForeignKey<MembershipDB>(m => m.UserId);
			// 用户收藏商家的多对多联系集
			modelBuilder.Entity<FavoriteMerchantDB>()
				.HasOne(um => um.UserDB)
				.WithMany(u => u.FavoriteMerchantDBs)
				.HasForeignKey(um => um.UserId);
			modelBuilder.Entity<FavoriteMerchantDB>()
				.HasOne(um => um.MerchantDB)
				.WithMany(m => m.FavoriteMerchantDBs)
				.HasForeignKey(um => um.MerchantId);
			// 定义菜品与商家的多对一联系
			modelBuilder.Entity<DishDB>()
				.HasOne(d => d.MerchantDB)
				.WithMany(m => m.DishDBs)
				.HasForeignKey(d => d.MerchantId);
			// 定义购物车项与用户的多对一联系
			modelBuilder.Entity<ShoppingCartDB>()
				.HasOne(s => s.UserDB)
				.WithMany(u => u.shoppingCartDBs)
				.HasForeignKey(s => s.UserId);
			// 定义购物车项与菜品的多对一联系
			modelBuilder.Entity<ShoppingCartDB>()
				.HasOne(s => s.DishDB)
				.WithMany(d => d.ShoppingCartDBs)
				.HasForeignKey(s => new { s.MerchantId, s.DishId });
			// 定义优惠券购买订单与优惠券的多对一联系
			modelBuilder.Entity<CouponPurchaseDB>()
				.HasOne(p => p.CouponDB)
				.WithMany(c => c.CouponPurchaseDBs)
				.HasForeignKey(p => p.CouponId);
			//定义优惠券购买订单与用户的多对一联系
            modelBuilder.Entity<CouponPurchaseDB>()
                .HasOne(cp => cp.UserDB)
                .WithMany(u => u.CouponPurchaseDBs)
                .HasForeignKey(cp => cp.UserId);
            // 用户拥有优惠券的多对多联系集
            modelBuilder.Entity<UserCouponDB>()
				.HasOne(uc => uc.UserDB)
				.WithMany(u => u.UserCouponDBs)
				.HasForeignKey(uc => uc.UserId);
			modelBuilder.Entity<UserCouponDB>()
				.HasOne(uc => uc.CouponDB)
				.WithMany(c => c.UserCouponDBs)
				.HasForeignKey(uc => uc.CouponId);
			// 定义工资记录与骑手的多对一联系
			modelBuilder.Entity<RiderWageDB>()
				.HasOne(rw => rw.RiderDB)
				.WithMany(r => r.RiderWageDBs)
				.HasForeignKey(rw => rw.RiderId);
			// 骑手与站点的多对一联系集
			modelBuilder.Entity<RiderStationDB>()
				.HasOne(rs => rs.RiderDB)
				.WithOne(r => r.RiderStationDB)
				.HasForeignKey<RiderStationDB>(rs => rs.RiderId);
			modelBuilder.Entity<RiderStationDB>()
				.HasOne(rs => rs.StationDB)
				.WithMany(s => s.RiderStationDBs)
				.HasForeignKey(rs => rs.StationId);
			// 定义满减活动到商家的多对一关系
			modelBuilder.Entity<SpecialOfferDB>()
				.HasOne(so => so.MerchantDB)
				.WithMany(m => m.SpecialOfferDBs)
				.HasForeignKey(so => so.MerchantId);
			// 订单与骑手的多对一联系集
			modelBuilder.Entity<OrderRiderDB>()
				.HasOne(or => or.OrderDB)
				.WithOne(o => o.OrderRiderDB)
				.HasForeignKey<OrderRiderDB>(or => or.OrderId);
			modelBuilder.Entity<OrderRiderDB>()
				.HasOne(or => or.RiderDB)
				.WithMany(r => r.OrderRiderDBs)
				.HasForeignKey(or => or.RiderId);
			// 订单与用户的多对一联系集
			modelBuilder.Entity<OrderUserDB>()
				.HasOne(ou => ou.UserDB)
				.WithMany(u => u.OrderUserDBs)
				.HasForeignKey(ou => ou.UserId);
			modelBuilder.Entity<OrderUserDB>()
				.HasOne(ou => ou.OrderDB)
				.WithOne(o => o.OrderUserDB)
				.HasForeignKey<OrderUserDB>(ou => ou.OrderId);
			// 订单与收货地址的多对一联系
			modelBuilder.Entity<OrderDB>()
				.HasOne(o => o.UserAddressDB)
				.WithMany(ua => ua.OrderDBs)
				.HasForeignKey(o => o.AddressId);
			// 订单到菜品的多对多联系集
			modelBuilder.Entity<OrderDishDB>()
				.HasOne(od => od.DishDB)
				.WithMany(d => d.OrderDishDBs)
				.HasForeignKey(od => new { od.MerchantId, od.DishId });
			modelBuilder.Entity<OrderDishDB>()
				.HasOne(od => od.OrderDB)
				.WithMany(o => o.OrderDishDBs)
				.HasForeignKey(od => od.OrderId);
			// 订单到优惠券的多对一联系集
			modelBuilder.Entity<OrderCouponDB>()
				.HasOne(oc => oc.OrderDB)
				.WithOne(o => o.OrderCouponDB)
				.HasForeignKey<OrderCouponDB>(oc => oc.OrderId);
			modelBuilder.Entity<OrderCouponDB>()
				.HasOne(oc => oc.UserCouponDB)
				.WithMany(uc => uc.OrderCouponDB)
				.HasForeignKey(oc => new { oc.UserId,oc.CouponId,oc.ExpirationDate});
            // 商家到站点的多对一联系集
            modelBuilder.Entity<MerchantStationDB>()
				.HasOne(ms => ms.MerchantDB)
				.WithOne(m => m.MerchantStationDB)
				.HasForeignKey<MerchantStationDB>(ms => ms.MerchantId);
			modelBuilder.Entity<MerchantStationDB>()
				.HasOne(ms => ms.StationDB)
				.WithMany(s => s.MerchantStationDBs)
				.HasForeignKey(ms => ms.StationId);
			// 用户地址到默认地址的一对一联系
			modelBuilder.Entity<UserDefaultAddressDB>()
				.HasOne(uda => uda.UserAddressDB)
				.WithOne(ua => ua.UserDefaultAddressDB)
				.HasForeignKey<UserDefaultAddressDB>(uda => uda.AddressId);
			// 用户地址与用户的一对一关系
			modelBuilder.Entity<UserDefaultAddressDB>()
				.HasOne(uda => uda.UserDB)
				.WithOne(u => u.UserDefaultAddressDB)
				.HasForeignKey<UserDefaultAddressDB>(uda => uda.UserId);

			// 每个用户只能有一个默认地址
			modelBuilder.Entity<UserDefaultAddressDB>()
				.HasIndex(uda => uda.UserId)
				.IsUnique();

			modelBuilder.Entity<DishDB>()
				.Property(d => d.DishPrice)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<CouponDB>()
				.Property(c => c.CouponValue)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<CouponDB>()
				.Property(c => c.CouponPrice)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<CouponDB>()
				.Property(c => c.MinPrice)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<RiderWageDB>()
				.Property(rw => rw.Wage)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<SpecialOfferDB>()
				.Property(so => so.MinPrice)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<SpecialOfferDB>()
				.Property(so => so.AmountRemission)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<OrderDB>()
				.Property(o => o.Price)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<OrderRiderDB>()
				.Property(or => or.RiderPrice)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<UserDB>()
				.Property(u => u.Wallet)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<RiderDB>()
				.Property(u => u.Wallet)
				.HasColumnType("numeric(10,2)");
			modelBuilder.Entity<MerchantDB>()
				.Property(u => u.Wallet)
				.HasColumnType("numeric(10,2)");

			modelBuilder.Entity<UserDB>().ToTable("users");
			modelBuilder.Entity<UserAddressDB>().ToTable("user_address");
			modelBuilder.Entity<MembershipDB>().ToTable("memberships");
			modelBuilder.Entity<MerchantDB>().ToTable("merchants");
			modelBuilder.Entity<FavoriteMerchantDB>().ToTable("favorite_merchants");
			modelBuilder.Entity<DishDB>().ToTable("dishes");
			modelBuilder.Entity<ShoppingCartDB>().ToTable("shoppingcarts");
			modelBuilder.Entity<CouponDB>().ToTable("coupons");
			modelBuilder.Entity<CouponPurchaseDB>().ToTable("coupon_purchases");
			modelBuilder.Entity<UserCouponDB>().ToTable("user_coupons");
			modelBuilder.Entity<RiderDB>().ToTable("riders");
			modelBuilder.Entity<RiderWageDB>().ToTable("rider_wages");
			modelBuilder.Entity<StationDB>().ToTable("stations");
			modelBuilder.Entity<RiderStationDB>().ToTable("rider_stations");
			modelBuilder.Entity<SpecialOfferDB>().ToTable("special_offers");
			modelBuilder.Entity<OrderDB>().ToTable("orders");
			modelBuilder.Entity<OrderRiderDB>().ToTable("order_riders");
			modelBuilder.Entity<OrderUserDB>().ToTable("order_users");
			modelBuilder.Entity<OrderDishDB>().ToTable("order_dishes");
			modelBuilder.Entity<OrderCouponDB>().ToTable("order_coupons");
			modelBuilder.Entity<AdminDB>().ToTable("admins");
			modelBuilder.Entity<MerchantStationDB>().ToTable("merchant_stations");
			modelBuilder.Entity<UserDefaultAddressDB>().ToTable("user_default_addresses");
		}
	}
}
