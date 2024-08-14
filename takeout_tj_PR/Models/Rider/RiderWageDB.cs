using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.Rider
{
	public class RiderWageDB
	{
		[Required(ErrorMessage = "Wage ID is required. ")]
		public int WageId { get; set; }  // 唯一标识一条工资记录

		[Required(ErrorMessage = "Rider ID is required. ")]
		public int RiderId { get; set; }

		[Required(ErrorMessage = "Wage timestamp is required. ")]
		[DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
		public DateTime WageTimestamp { get; set; }  // 记录上次发工资到该时刻间的工资

		[Required(ErrorMessage = "Wage is required. ")]
		public decimal Wage {  get; set; }

		public RiderDB RiderDB { get; set; }
	}
}
