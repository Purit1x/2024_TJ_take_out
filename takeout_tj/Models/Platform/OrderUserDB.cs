using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.Models.Platform
{
	public class OrderUserDB
	{
		[Required(ErrorMessage = "OrderId is required. ")]
		public int OrderId { get; set; }

		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		public UserDB UserDB { get; set; }
		public OrderDB OrderDB { get; set; }
	}
}
