using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Platform;

namespace takeout_tj.DTO
{
    public class CouponPurchaseDto
    {
        public int CouponPurchaseId { get; set; }

        public DateTime PurchasingTimestamp { get; set; }

        public int CouponId { get; set; }

        public int UserId { get; set; }

        public int PurchasingAmount { get; set; }

    }
}
