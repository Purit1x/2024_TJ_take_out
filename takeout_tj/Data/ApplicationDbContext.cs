using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;
using takeout_tj.Models.User;

namespace takeout_tj.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<UserDB> Users { get; set; }
		public DbSet<UserAddressDB> UserAddresses { get; set; }
		public DbSet<Membership> Memberships { get; set; }
		
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			if(!optionsBuilder.IsConfigured)
			{

			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<UserDB>().HasKey(u => u.UserId);
			modelBuilder.Entity<UserAddressDB>().HasKey(u => new {u.UserId, u.UserAddress});
			// 定义地址到用户的多对一关系
			modelBuilder.Entity<UserAddressDB>()
				.HasOne(a => a.UserDB)
				.WithMany()
				.HasForeignKey(a => a.UserId);
			modelBuilder.Entity<Membership>().HasKey(m => m.UserId);
			modelBuilder.Entity<Membership>()
				.HasOne(m => m.UserDB)
				.WithOne()
				.HasForeignKey<Membership>(m => m.UserId);
				
			modelBuilder.Entity<UserDB>().ToTable("Users");
			modelBuilder.Entity<UserAddressDB>().ToTable("UserAddresses");
			modelBuilder.Entity<Membership>().ToTable("Memberships");
		}
	}
}
