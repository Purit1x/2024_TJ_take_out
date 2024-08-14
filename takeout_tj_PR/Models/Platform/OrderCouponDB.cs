using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.Platform
{
	public class OrderCouponDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		[Required(ErrorMessage = "Coupon ID is required. ")]
		public int CouponId { get; set; }

		public OrderDB OrderDB { get; set; }
		public CouponDB CouponDB { get; set; }
	}
}
