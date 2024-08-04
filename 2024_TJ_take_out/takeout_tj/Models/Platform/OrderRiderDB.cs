using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Rider;

namespace takeout_tj.Models.Platform
{
	public class OrderRiderDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		[Required(ErrorMessage = "Rider ID is required. ")]
		public int RiderId { get; set; }

		[Required(ErrorMessage = "Order price is required. ")]
		public decimal RiderPrice { get; set; }  // 骑手该单可获得多少报酬

		public OrderDB  OrderDB { get; set; }
		public RiderDB RiderDB { get; set; }
	}
}
