using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Merchant
{
	public class MerchantDB
	{
		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "User password is required. ")]
		[StringLength(20, MinimumLength = 6, ErrorMessage = "The password should be at least 6 characters and at most 20 characters long. ")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Merchant name is required. ")]
		[StringLength(255, ErrorMessage = "Merchant name must be at most 255 characters long. ")]
		public string MerchantName { get; set; }

		[Required(ErrorMessage = "Merchant address is required. ")]
		[StringLength(255, ErrorMessage = "Merchant address must be at most 255 characters long. ")]
		public string MerchantAddress { get; set; }

		[Required(ErrorMessage = "Contact is required. ")]
		[StringLength(255, ErrorMessage = "Contact must be at most 255 characters long. ")]
		public string Contact { get; set; }

		[Required(ErrorMessage = "Coupon type is required. ")]
		public int CouponType { get; set; } = 0;  // 0为仅通用券, 1为允许特殊类型

		[StringLength(20, ErrorMessage = "DishType must be at most 255 characters long. ")]
		public string DishType { get; set; }

        [Required(ErrorMessage = "Time for Open Business is required. ")]
        public int TimeforOpenBusiness { get; set; }

        [Required(ErrorMessage = "Time for Close Business is required. ")]
        public int TimeforCloseBusiness { get; set; }

        [Required(ErrorMessage = "Wallet is required. ")]
        public decimal Wallet { get; set; } = 0.00m; // 默认值为0.00

        [Required(ErrorMessage = "WalletPassword is required. ")]
        public string WalletPassword { get; set; }

        public ICollection<FavoriteMerchantDB> FavoriteMerchantDBs { get; set; }  // 用于收藏商家的多对多关系的导航属性
		public ICollection<DishDB> DishDBs { get; set; }
		public ICollection<SpecialOfferDB> SpecialOfferDBs { get; set; }
		public MerchantStationDB MerchantStationDB { get; set; }
	}
}
