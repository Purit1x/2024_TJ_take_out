using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Platform
{
	public class CouponPurchaseDB
	{
		[Required(ErrorMessage = "CouponPurchaseId is required. ")]
		public int CouponPurchaseId { get; set; }

		[Required(ErrorMessage = "PurchasingTimestamp is required. ")]
		[DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
		public DateTime PurchasingTimestamp { get; set; }

		[Required(ErrorMessage = "CouponId is required. ")]
		public int CouponId { get; set; }

        [Required(ErrorMessage = "UserId is required. ")]
        public int UserId {  get; set; }

		[Required(ErrorMessage = "Purchasing amount is required. ")]
		[Range(1, int.MaxValue, ErrorMessage = "Purchasing amount must be at least 1. ")]
		public int PurchasingAmount { get; set; } = 1;

        public CouponDB CouponDB { get; set; }

        public UserDB UserDB { get; set; }
	}
}
