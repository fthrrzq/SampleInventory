using System.ComponentModel.DataAnnotations;

namespace SampleInventory.Dtos
{
    public class CreateStockEntryDto
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Supplier { get; set; } = string.Empty;

        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime? EntryDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateStockEntryItemDto> Items { get; set; } = new List<CreateStockEntryItemDto>();
    }

    public class CreateStockEntryItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string? BatchNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class StockEntryDto
    {
        public int Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class StockEntryDetailDto
    {
        public int Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
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

    public class StockEntryItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}