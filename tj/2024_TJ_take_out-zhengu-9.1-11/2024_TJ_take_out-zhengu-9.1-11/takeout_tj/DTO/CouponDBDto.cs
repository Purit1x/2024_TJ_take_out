using System.ComponentModel.DataAnnotations;

namespace takeout_tj.DTO
{
    public class CouponDBDto
    {
        public int CouponId { get; set; }

        public string CouponName { get; set; }

        public decimal CouponValue { get; set; }  // 优惠券价值

        public decimal CouponPrice { get; set; } // 购买优惠券的价格, 活动获取则为0

        public int CouponType { get; set; } = 0;  // 优惠券类型, 0为通用, 1为特殊

        public decimal MinPrice { get; set; }

        public int PeriodOfValidity { get; set; }  // 优惠券有效天数

        public int QuantitySold { get; set; }

        public int IsOnShelves { get; set; }  //是否上架
    }
}
