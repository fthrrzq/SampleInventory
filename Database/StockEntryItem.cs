namespace SampleInventory.Database
{
    public class StockEntryItem
    {
        public int Id { get; set; }
        public int StockEntryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual StockEntry StockEntry { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
