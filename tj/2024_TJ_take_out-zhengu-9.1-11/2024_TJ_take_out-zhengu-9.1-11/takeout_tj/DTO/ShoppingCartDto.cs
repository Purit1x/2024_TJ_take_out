namespace takeout_tj.DTO
{
    public class ShoppingCartDto
    {
        public int UserId { get; set; }
        public int ShoppingCartId { get; set; }
        public int MerchantId { get; set; }
        public int DishId { get; set; }
        public int DishNum { get; set; }  // 菜品数量
    }
}
