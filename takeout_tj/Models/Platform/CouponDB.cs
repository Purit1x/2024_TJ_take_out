using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Platform
{
	public class CouponDB
	{
		[Required(ErrorMessage = "Coupon ID is required. ")]
		public int CouponId { get; set; }

		[Required(ErrorMessage = "Coupon name is required. ")]
		[StringLength(100, ErrorMessage = "Coupon name must be at most 20 characeters long")]
		public string CouponName { get; set; }

		[Required(ErrorMessage = "Coupon value is required. ")]
		public decimal CouponValue { get; set; }  // 优惠券价值

		public decimal CouponPrice { get; set; } = 0; // 购买优惠券的价格, 活动获取则为0

		[Required(ErrorMessage = "Coupon type is required. ")] 
		public int CouponType { get; set; } = 0;  // 优惠券类型, 0为通用, 1为特殊

		[Required(ErrorMessage = "MinPrice is required. ")]
		public decimal MinPrice { get; set; } = 0;

		public int PeriodOfValidity { get; set; }  // 优惠券有效天数

		[Required(ErrorMessage = "QuantitySold is required. ")]
		public int QuantitySold { get; set; }

        [Required(ErrorMessage = "IsOnShelves is required.")]
        public int IsOnShelves { get; set; }  //是否上架
        public ICollection<CouponPurchaseDB> CouponPurchaseDBs { get; set; }
		public ICollection<UserCouponDB> UserCouponDBs { get; set; }

	}
}
