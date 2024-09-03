using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Merchant;

namespace takeout_tj.DTO
{
    public class SpecialOfferDBDto
    {
        public int MerchantId { get; set; }
        public int OfferId { get; set; }
        public decimal MinPrice { get; set; } = 0;  // 满减活动适用的最低价格
        public decimal AmountRemission { get; set; }  // 满减活动减免的金额

    }
}
