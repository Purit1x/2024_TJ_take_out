using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.Merchant
{
	public class SpecialOfferDB
	{
		[Required(ErrorMessage = "Merchant ID is required. ")]
		public int MerchantId { get; set; }

		[Required(ErrorMessage = "Offer ID is required. ")]
		public int OfferId { get; set; }

		[Required(ErrorMessage = "MinPrice is required. ")]
		public decimal MinPrice { get; set; } = 0;  // 满减活动适用的最低价格

		[Required(ErrorMessage = "AmountRemission is required. ")]
		public decimal AmountRemission { get; set; }  // 满减活动减免的金额

		public MerchantDB MerchantDB { get; set; }
	}
}
