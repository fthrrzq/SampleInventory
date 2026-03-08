namespace SampleInventory.Database
{
    public class StockMovement
    {
        public int Id { get; set; }
        public string MovementNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int Quantity { get; set; }
        public MovementStatus Status { get; set; }
        public DateTime MovementDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse SourceWarehouse { get; set; } = null!;
        public virtual Warehouse DestinationWarehouse { get; set; } = null!;
    }
}
