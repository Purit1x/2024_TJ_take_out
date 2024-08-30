namespace takeout_tj.DTO
{
    public class OrderCreate
    {
        public decimal Price { get; set; }
        public DateTime OrderTimestamp { get; set; }
        //public int State { get; set; }
        public int NeedUtensils { get; set; }  // 0为不需要餐具,1为需要餐具
        public int AddressId { get; set; }  // 使用的用户地址
        public int UserId { get; set; }
        public int CouponId { get; set; }
        public DateTime ExpirationDate { get; set; }  // 优惠券到期时间
        public int MerchantId { get; set; }
        public ShoppingCartDto[] shoppingCart { get; set; }
		public decimal RiderPrice { get; set; }  // 骑手该单可获得多少报酬
	}
}
