using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Rider;

namespace takeout_tj.Models.Merchant
{
	public class MerchantStationDB
	{
		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "Station ID is required. ")]
		public int StationId { get; set; }

		public MerchantDB MerchantDB { get; set; }
		public StationDB StationDB { get; set; }
	}
}
