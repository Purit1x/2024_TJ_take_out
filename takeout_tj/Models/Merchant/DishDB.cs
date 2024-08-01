using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Merchant
{
	public class DishDB
	{
		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "Dish ID is required. ")]
		public int DishId { get; set; }

		[Required(ErrorMessage = "Dish name is required. ")]
		[StringLength(255,ErrorMessage = "Dish name must be at most 15 characters long. ")]
		public string DishName { get; set; }

		[Required(ErrorMessage = "Dish price is required. ")]
		public decimal DishPrice { get; set; }

		public string DishCategory { get; set; }  // 菜品列别, 可以为空

		public MerchantDB MerchantDB { get; set; }
		public ICollection<ShoppingCartDB> ShoppingCartDBs { get; set; }
	}
}
