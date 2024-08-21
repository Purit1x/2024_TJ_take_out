using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Platform
{
	public class OrderDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		[Required(ErrorMessage = "Price is required. ")]
		public decimal Price;

		[Required(ErrorMessage = "OrderTimestamp is required. ")]
		public DateTime OrderTimestamp { get; set; }  // 创建订单的时间

		[Required(ErrorMessage = "Expected time of arrival is required. ")]
		[DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
		public DateTime ExpectedTimeOfArrival { get; set; }

		[DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
		public DateTime RealTimeOfArrival { get; set; }

		// 订单状态(包含0:未支付; 1:已付款; 2:待处理; 3: 已接单; 4:配送中 5: 已送达 6:已完成)
		[Required(ErrorMessage = "State is required. ")]
		public int State { get; set; } = 0;

		[Required(ErrorMessage = "NeedUtensils is required. ")]
		public int NeedUtensils { get; set; } = 1;  // 0为不需要餐具,1为需要餐具

		[Required(ErrorMessage = "Address ID is required.")]
		public int AddressId { get; set; }  // 使用的用户地址

		public int MerchantRating { get; set; } = 5;

		public int RiderRating { get; set; } = 5;

		public string Comment { get; set; }  // 用户对订单的详细评价
		
		public OrderRiderDB OrderRiderDB { get; set; }
		public OrderUserDB OrderUserDB { get; set; }
		public ICollection<OrderDishDB> OrderDishDBs { get;	set;}
		public OrderCouponDB OrderCouponDB { get; set; }
		public UserAddressDB UserAddressDB { get; set; }
	}
}
