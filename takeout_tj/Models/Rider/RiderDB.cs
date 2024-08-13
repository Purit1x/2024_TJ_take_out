using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Platform;

namespace takeout_tj.Models.Rider
{
	public class RiderDB
	{
		[Required(ErrorMessage = "Rider ID is required. ")]
		public int RiderId { get; set; }

		[Required(ErrorMessage = "User password is required. ")]
		[StringLength(20, MinimumLength = 6, ErrorMessage = "The password should be at least 6 characters and at most 20 characters long. ")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Rider name is required. ")]
		[StringLength(50, ErrorMessage = "Rider name must be at most 50 characters long. ")]
		public string RiderName { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[StringLength(11, MinimumLength = 11, ErrorMessage = "The field PhoneNumber must be exactly 10 digitss. ")]
		[RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number can only contain digits. ")]
		public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Wallet is required. ")]
        public decimal Wallet { get; set; } = 0.00m; // 默认值为0.00

        [Required(ErrorMessage = "WalletPassword is required. ")]
        public string WalletPassword { get; set; }

        public ICollection<RiderWageDB> RiderWageDBs { get; set; }
		public RiderStationDB RiderStationDB { get; set; }
		public ICollection<OrderRiderDB> OrderRiderDBs { get; set;}
	}
}
