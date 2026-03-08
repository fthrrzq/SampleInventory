namespace SampleInventory.Database
{
    public class BarInventory
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string BarLocation { get; set; } = string.Empty; // BAR, LT1, LT2, LT5, TOP1
        public DateTime LastUpdated { get; set; }

        // Navigation properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
