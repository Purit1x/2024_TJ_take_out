using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;

namespace takeout_tj.Models.Rider
{
	public class StationDB
	{
		[Required(ErrorMessage = "Station ID is required. ")]
		public int StationId { get; set; }

		[Required(ErrorMessage = "Station name is required. ")]
		[StringLength(100, ErrorMessage = "Station name must be at most 100 characters long. ")]
		public string StationName { get; set; }

		[Required(ErrorMessage = "Station address is required. ")]
		[StringLength(255, ErrorMessage = "Station address must be at most 255 characters long. ")]
		public string StationAddress { get; set; }

		public ICollection<RiderStationDB> RiderStationDBs { get; set; }
		public ICollection<MerchantStationDB> MerchantStationDBs { get; set;}
	}
}
