namespace SampleInventory.Database
{
    public class StockEntry
    {
        public int Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public EntryStatus Status { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }

        // Navigation properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<StockEntryItem> Items { get; set; } = new List<StockEntryItem>();
    }
}
