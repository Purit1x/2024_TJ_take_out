using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Rider;
using Newtonsoft.Json;

namespace takeout_tj.Models.Platform
{
	public class OrderRiderDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		public int? RiderId { get; set; }  // RiderId可空是为了创建订单时能记录派送费

		[Required(ErrorMessage = "Order price is required. ")]
		public decimal RiderPrice { get; set; }  // 骑手该单可获得多少报酬

		[JsonIgnore]
		public OrderDB  OrderDB { get; set; }
		public RiderDB RiderDB { get; set; }
	}
}
