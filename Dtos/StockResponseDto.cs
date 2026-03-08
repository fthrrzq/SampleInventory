namespace SampleInventory.Dtos
{
    public class StockResponseDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<ProductStockDto> Products { get; set; } = new List<ProductStockDto>();
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public string StockStatus { get; set; } = string.Empty; // "Normal", "Low", "Out of Stock"
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class StockSummaryDto
    {
        public int TotalWarehouses { get; set; }
        public int TotalProducts { get; set; }
        public int TotalStockQuantity { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<WarehouseSummaryDto> WarehouseSummaries { get; set; } = new List<WarehouseSummaryDto>();
        public List<LowStockAlertDto> LowStockAlerts { get; set; } = new List<LowStockAlertDto>();
    }

    public class WarehouseSummaryDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class LowStockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int Deficit { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // DTOs/StockFilterDto.cs
    public class StockFilterDto
    {
        public int? WarehouseId { get; set; }
        public string? WarehouseType { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? StockStatus { get; set; } // "All", "Low", "OutOfStock", "Normal"
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
