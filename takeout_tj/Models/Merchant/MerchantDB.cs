using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Merchant
{
	public class MerchantDB
	{
		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

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

		public ICollection<FavoriteMerchantDB> FavoriteMerchantDBs { get; set; }  // 用于收藏商家的多对多关系的导航属性
		public ICollection<DishDB> DishDBs { get; set; }
	}
}
