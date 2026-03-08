namespace SampleInventory.Dtos
{
    public class StockEntryResponseDto
    {
        public int Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public List<StockEntryItemDto> Items { get; set; } = new List<StockEntryItemDto>();
    }
}
