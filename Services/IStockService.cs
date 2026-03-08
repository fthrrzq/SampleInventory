using SampleInventory.Dtos;

namespace SampleInventory.Services
{
    public interface IStockService
    {
        Task<StockResponseDto> GetStockByWarehouseAsync(int warehouseId);
        Task<List<StockResponseDto>> GetAllStocksAsync(StockFilterDto filter);
        Task<StockSummaryDto> GetStockSummaryAsync();
        Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int? warehouseId = null);
        Task<byte[]> ExportStockToExcelAsync(int? warehouseId = null);
        Task<ProductStockDto> GetProductStockDetailAsync(int productId, int? warehouseId = null);
        Task<Dictionary<string, int>> GetStockByLocationAsync(int warehouseId, string location);
    }
}
