using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.Rider
{
	public class RiderStationDB
	{
		[Required(ErrorMessage = "Rider ID is required. ")]
		public int RiderId { get; set; }

		[Required(ErrorMessage = "Station ID is required. ")]
		public int StationId { get; set; }

		public RiderDB RiderDB { get; set; }
		public StationDB StationDB { get; set; }
	}
}
