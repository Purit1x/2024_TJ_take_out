using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Platform;
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

        [Required(ErrorMessage = "Image Url is required. ")]
        public string ImageUrl { get; set; } = "https://img.zcool.cn/community/0138245e5218c4a80120a8950b14a0.png@1280w_1l_2o_100sh.png"; //菜品图片链接

        [Required(ErrorMessage = "Dish inventory is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Dish inventory must be non-negative. ")]
        public int DishInventory {  get; set; }  //菜品库存

        public MerchantDB MerchantDB { get; set; }
		public ICollection<ShoppingCartDB> ShoppingCartDBs { get; set; }
		public ICollection<OrderDishDB> OrderDishDBs { get; set; }
	}
}
