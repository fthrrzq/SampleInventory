namespace SampleInventory.Database
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WarehouseType Type { get; set; } // Pusat atau Outlet
        public int? ParentWarehouseId { get; set; } // Untuk relasi hirarki
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Warehouse? ParentWarehouse { get; set; }
        public virtual ICollection<Warehouse> ChildWarehouses { get; set; } = new List<Warehouse>();
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public virtual ICollection<StockMovement> SourceMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<StockMovement> DestinationMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
    }
}
