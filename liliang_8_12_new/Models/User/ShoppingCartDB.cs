using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;

namespace takeout_tj.Models.User
{
	public class ShoppingCartDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Shopping cart ID is required.")]
		public int ShoppingCartId { get; set; }

		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "Dish ID is required. ")]
		public int DishId { get; set; }

		[Range(1,int.MaxValue)]
		public int DishNum { get; set; }  // 菜品数量

		public UserDB UserDB { get; set; }
		public DishDB DishDB { get; set; }
	}
}
