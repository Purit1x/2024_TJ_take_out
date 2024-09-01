using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Platform;
using Newtonsoft.Json;

namespace takeout_tj.Models.User
{
	public class UserCouponDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Coupon ID is required. ")]
		public int CouponId { get; set; }

		[Required(ErrorMessage = "AmountOwned is required. ")]
		public int AmountOwned { get; set; }  // 拥有指定类型, 指定截止日期的优惠券的数量

		[DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
		public DateTime ExpirationDate { get; set; }  // 优惠券到期时间
        [JsonIgnore]
        public UserDB UserDB { get; set; }
        [JsonIgnore]
        public CouponDB CouponDB { get; set; }
        public ICollection<OrderCouponDB> OrderCouponDB { get; set; }
    }
}
