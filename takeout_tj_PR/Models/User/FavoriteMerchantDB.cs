using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;

namespace takeout_tj.Models.User
{
	public class FavoriteMerchantDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		// EF Core 需要定义外键属性对应的实体
		public UserDB UserDB { get; set; }
		public MerchantDB MerchantDB { get; set; }
	}
}
