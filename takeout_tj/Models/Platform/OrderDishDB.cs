using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;

namespace takeout_tj.Models.Platform
{
	public class OrderDishDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "Dish ID is required. ")]
		public int DishId { get; set; }

        [Required(ErrorMessage = "Dish num is required. ")]
        [Range(1, int.MaxValue)]
        public int DishNum { get; set; }

		public OrderDB OrderDB { get; set; }
		public DishDB DishDB { get; set; }
	}
}
