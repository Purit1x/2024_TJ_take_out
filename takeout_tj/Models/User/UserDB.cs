using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;
using takeout_tj.Models.Platform;

namespace takeout_tj.Models.User
{
	public class UserDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "User name is required.")]
		[StringLength(50, ErrorMessage = "The field UserName must be at most 15 characters long. ")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[StringLength(11, MinimumLength = 11, ErrorMessage = "The field PhoneNumber must be exactly 11 digitss. ")]
		[RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number can only contain digits. ")]
		public string PhoneNumber {  get; set; }

		[Required(ErrorMessage = "User password is required. ")]
		[StringLength(20, MinimumLength = 6, ErrorMessage = "The password should be at least 6 characters and at most 20 characters long. ")]
		public string Password { get; set; }

        [Required(ErrorMessage = "Wallet is required. ")]
        public decimal Wallet { get; set; } = 0.00m; // 默认值为0.00

        [Required(ErrorMessage = "WalletPassword is required. ")]
        public string WalletPassword { get; set; }
        public ICollection<UserAddressDB> UserAddressDBs { get; set; } = new HashSet<UserAddressDB>();	
		// 用于收藏商家的导航属性, 这个导航属性应该指向连接实体, 而非联系集的另一侧
		public ICollection<FavoriteMerchantDB> FavoriteMerchantDBs { get; set; }
        public ICollection<ShoppingCartDB> shoppingCartDBs { get; set; }
        public ICollection<UserCouponDB> UserCouponDBs { get; set; }
        public ICollection<OrderUserDB> OrderUserDBs { get; set; }
		public ICollection<CouponPurchaseDB> CouponPurchaseDBs { get;  set; }
		public UserDefaultAddressDB UserDefaultAddressDB { get; set; }
    }
}
