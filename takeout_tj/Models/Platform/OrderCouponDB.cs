using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Platform
{
	public class OrderCouponDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Coupon ID is required. ")]
		public int CouponId { get; set; }

        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
        public DateTime ExpirationDate { get; set; }  // 优惠券到期时间

        public OrderDB OrderDB { get; set; }
		public UserCouponDB UserCouponDB { get; set; }
	}
}
