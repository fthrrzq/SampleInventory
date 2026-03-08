using SampleInventory.Dtos;

namespace SampleInventory.ViweModels
{
    public class StockDashboardViewModel
    {
        public StockSummaryDto Summary { get; set; } = new();
        public List<LowStockAlertDto> CriticalAlerts { get; set; } = new();
        public List<RecentMovementDto> RecentMovements { get; set; } = new();
        public Dictionary<string, int> StockByCategory { get; set; } = new();
        public List<WarehouseStockChartDto> WarehouseComparison { get; set; } = new();
    }

    public class RecentMovementDto
    {
        public string MovementNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string FromWarehouse { get; set; } = string.Empty;
        public string ToWarehouse { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; }
    }

    public class WarehouseStockChartDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public int StockCount { get; set; }
        public decimal StockValue { get; set; }
    }
}
