using System.ComponentModel.DataAnnotations;

namespace SampleInventory.Dtos
{
    public class CreateProductDto
    {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(10)]
        public string Unit { get; set; } = string.Empty; // pcs, box, kg, etc

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }
    }

    // DTO untuk update product
    public class UpdateProductDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Unit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinimumStock { get; set; }

        public bool? IsActive { get; set; }
    }

    // DTO untuk response product
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MinimumStock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalStock { get; set; } // Total stock di semua warehouse
    }

    // DTO untuk stock movement history
    public class StockMovementHistoryDto
    {
        public string MovementNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string SourceWarehouse { get; set; } = string.Empty;
        public string DestinationWarehouse { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // DTO untuk stock per warehouse
    public class WarehouseStockDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}