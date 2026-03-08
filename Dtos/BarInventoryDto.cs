namespace SampleInventory.Dtos
{
    public class BarInventoryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BarLocation { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
