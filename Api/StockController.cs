using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;
using SampleInventory.Services;

namespace SampleInventory.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StockController> _logger;
        private readonly ApplicationDbContext _context;  // Tambahkan ini
        public StockController(IStockService stockService, ILogger<StockController> logger, ApplicationDbContext context)
        {
            _stockService = stockService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get stock by specific warehouse
        /// </summary>
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<StockResponseDto>> GetStockByWarehouse(int warehouseId)
        {
            try
            {
                var result = await _stockService.GetStockByWarehouseAsync(warehouseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock for warehouse {WarehouseId}", warehouseId);
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get all stocks with filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StockResponseDto>>> GetAllStocks([FromQuery] StockFilterDto filter)
        {
            try
            {
                var result = await _stockService.GetAllStocksAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stocks");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get stock summary across all warehouses
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<StockSummaryDto>> GetStockSummary()
        {
            try
            {
                var result = await _stockService.GetStockSummaryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock summary");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get low stock alerts
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<List<LowStockAlertDto>>> GetLowStockAlerts([FromQuery] int? warehouseId = null)
        {
            try
            {
                var result = await _stockService.GetLowStockAlertsAsync(warehouseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock alerts");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get product stock detail
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductStockDto>> GetProductStock(int productId, [FromQuery] int? warehouseId = null)
        {
            try
            {
                var result = await _stockService.GetProductStockDetailAsync(productId, warehouseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product stock for product {ProductId}", productId);
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get stock by specific location in warehouse (for Bar)
        /// </summary>
        [HttpGet("location/{warehouseId}")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStockByLocation(int warehouseId, [FromQuery] string location)
        {
            try
            {
                var result = await _stockService.GetStockByLocationAsync(warehouseId, location);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock by location");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Export stock to Excel
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportStock([FromQuery] int? warehouseId = null)
        {
            try
            {
                var excelData = await _stockService.ExportStockToExcelAsync(warehouseId);
                var fileName = warehouseId.HasValue
                    ? $"Stock_Warehouse_{warehouseId}_{DateTime.Now:yyyyMMdd}.csv"
                    : $"All_Stocks_{DateTime.Now:yyyyMMdd}.csv";

                return File(excelData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting stock");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get stock movement history for a product
        /// </summary>
        [HttpGet("movements/product/{productId}")]
        public async Task<ActionResult<List<StockMovement>>> GetProductMovementHistory(int productId,
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var query = _context.StockMovements
                    .Include(m => m.SourceWarehouse)
                    .Include(m => m.DestinationWarehouse)
                    .Where(m => m.ProductId == productId)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(m => m.MovementDate >= startDate);

                if (endDate.HasValue)
                    query = query.Where(m => m.MovementDate <= endDate);

                var movements = await query
                    .OrderByDescending(m => m.MovementDate)
                    .ToListAsync();

                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movement history");
                return BadRequest(ex.Message);
            }
        }
    }
}
