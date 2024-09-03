namespace takeout_tj.DTO
{
    public class DishDBDto
    {
        public int MerchantId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; }
        public decimal DishPrice { get; set; }
        public string DishCategory { get; set; }
        public string ImageUrl { get; set; }
        public int DishInventory { get; set; }

    }
}
