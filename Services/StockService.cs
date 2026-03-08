using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;

namespace SampleInventory.Services
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockService> _logger;

        public StockService(ApplicationDbContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StockResponseDto> GetStockByWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
                throw new Exception("Warehouse not found");

            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.WarehouseId == warehouseId)
                .ToListAsync();

            var productStocks = inventories.Select(i => new ProductStockDto
            {
                ProductId = i.Product.Id,
                ProductCode = i.Product.Code,
                ProductName = i.Product.Name,
                Description = i.Product.Description,
                Unit = i.Product.Unit,
                Quantity = i.Quantity,
                MinimumStock = i.Product.MinimumStock,
                StockStatus = GetStockStatus(i.Quantity, i.Product.MinimumStock),
                Price = i.Product.Price,
                TotalValue = i.Quantity * i.Product.Price,
                LastUpdated = i.LastUpdated
            }).OrderBy(p => p.ProductName).ToList();

            return new StockResponseDto
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                WarehouseCode = warehouse.Code,
                WarehouseType = warehouse.Type.ToString(),
                Location = warehouse.Location,
                Products = productStocks,
                TotalProducts = productStocks.Count,
                TotalQuantity = productStocks.Sum(p => p.Quantity),
                LastUpdated = DateTime.Now
            };
        }

        public async Task<List<StockResponseDto>> GetAllStocksAsync(StockFilterDto filter)
        {
            var query = _context.Warehouses
                .Include(w => w.Inventories)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            // Filter by warehouse type
            if (!string.IsNullOrEmpty(filter.WarehouseType))
            {
                if (Enum.TryParse<WarehouseType>(filter.WarehouseType, out var type))
                {
                    query = query.Where(w => w.Type == type);
                }
            }

            // Filter by specific warehouse
            if (filter.WarehouseId.HasValue)
            {
                query = query.Where(w => w.Id == filter.WarehouseId);
            }

            var warehouses = await query.ToListAsync();
            var result = new List<StockResponseDto>();

            foreach (var warehouse in warehouses)
            {
                var productStocks = warehouse.Inventories
                    .Select(i => new ProductStockDto
                    {
                        ProductId = i.Product.Id,
                        ProductCode = i.Product.Code,
                        ProductName = i.Product.Name,
                        Description = i.Product.Description,
                        Unit = i.Product.Unit,
                        Quantity = i.Quantity,
                        MinimumStock = i.Product.MinimumStock,
                        StockStatus = GetStockStatus(i.Quantity, i.Product.MinimumStock),
                        Price = i.Product.Price,
                        TotalValue = i.Quantity * i.Product.Price,
                        LastUpdated = i.LastUpdated
                    });

                // Apply filters
                if (!string.IsNullOrEmpty(filter.ProductName))
                {
                    productStocks = productStocks.Where(p =>
                        p.ProductName.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filter.StockStatus))
                {
                    productStocks = productStocks.Where(p =>
                        p.StockStatus.Equals(filter.StockStatus, StringComparison.OrdinalIgnoreCase));
                }

                if (filter.MinQuantity.HasValue)
                {
                    productStocks = productStocks.Where(p => p.Quantity >= filter.MinQuantity);
                }

                if (filter.MaxQuantity.HasValue)
                {
                    productStocks = productStocks.Where(p => p.Quantity <= filter.MaxQuantity);
                }

                var filteredProducts = productStocks.ToList();

                result.Add(new StockResponseDto
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCode = warehouse.Code,
                    WarehouseType = warehouse.Type.ToString(),
                    Location = warehouse.Location,
                    Products = filteredProducts,
                    TotalProducts = filteredProducts.Count,
                    TotalQuantity = filteredProducts.Sum(p => p.Quantity),
                    LastUpdated = DateTime.Now
                });
            }

            return result;
        }

        public async Task<StockSummaryDto> GetStockSummaryAsync()
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.Inventories)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            var allInventories = warehouses.SelectMany(w => w.Inventories);

            var summary = new StockSummaryDto
            {
                TotalWarehouses = warehouses.Count,
                TotalProducts = allInventories.Select(i => i.ProductId).Distinct().Count(),
                TotalStockQuantity = allInventories.Sum(i => i.Quantity),
                TotalStockValue = allInventories.Sum(i => i.Quantity * i.Product.Price),
                WarehouseSummaries = warehouses.Select(w => new WarehouseSummaryDto
                {
                    WarehouseId = w.Id,
                    WarehouseName = w.Name,
                    ProductCount = w.Inventories.Count,
                    TotalQuantity = w.Inventories.Sum(i => i.Quantity),
                    TotalValue = w.Inventories.Sum(i => i.Quantity * i.Product.Price)
                }).ToList(),
                LowStockAlerts = await GetLowStockAlertsAsync()
            };

            return summary;
        }

        public async Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int? warehouseId = null)
        {
            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Where(i => i.Quantity <= i.Product.MinimumStock)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == warehouseId);
            }

            var lowStockItems = await query.ToListAsync();

            return lowStockItems.Select(i => new LowStockAlertDto
            {
                ProductId = i.Product.Id,
                ProductName = i.Product.Name,
                ProductCode = i.Product.Code,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                CurrentStock = i.Quantity,
                MinimumStock = i.Product.MinimumStock,
                Deficit = i.Product.MinimumStock - i.Quantity,
                Unit = i.Product.Unit
            }).OrderBy(a => a.Deficit).ToList();
        }

        public async Task<ProductStockDto> GetProductStockDetailAsync(int productId, int? warehouseId = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new Exception("Product not found");

            var query = _context.Inventories
                .Include(i => i.Warehouse)
                .Where(i => i.ProductId == productId)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == warehouseId);
            }

            var inventories = await query.ToListAsync();
            var totalQuantity = inventories.Sum(i => i.Quantity);

            return new ProductStockDto
            {
                ProductId = product.Id,
                ProductCode = product.Code,
                ProductName = product.Name,
                Description = product.Description,
                Unit = product.Unit,
                Quantity = totalQuantity,
                MinimumStock = product.MinimumStock,
                StockStatus = GetStockStatus(totalQuantity, product.MinimumStock),
                Price = product.Price,
                TotalValue = totalQuantity * product.Price,
                LastUpdated = DateTime.Now
            };
        }

        public async Task<Dictionary<string, int>> GetStockByLocationAsync(int warehouseId, string location)
        {
            // Untuk Bar inventory dengan lokasi spesifik
            var barInventories = await _context.BarInventories
                .Where(b => b.WarehouseId == warehouseId && b.BarLocation == location)
                .Include(b => b.Product)
                .ToListAsync();

            return barInventories.ToDictionary(
                b => b.Product.Name,
                b => b.Quantity
            );
        }

        public async Task<byte[]> ExportStockToExcelAsync(int? warehouseId = null)
        {
            // Implementasi export ke Excel
            // Bisa menggunakan library seperti EPPlus atau ClosedXML

            var stocks = await GetAllStocksAsync(new StockFilterDto
            {
                WarehouseId = warehouseId
            });

            // Contoh sederhana, sebaiknya implementasi dengan library Excel
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);

            writer.WriteLine("Warehouse,Product Code,Product Name,Quantity,Unit,Status,Value");

            foreach (var warehouse in stocks)
            {
                foreach (var product in warehouse.Products)
                {
                    writer.WriteLine($"{warehouse.WarehouseName},{product.ProductCode},{product.ProductName},{product.Quantity},{product.Unit},{product.StockStatus},{product.TotalValue}");
                }
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        private string GetStockStatus(int quantity, int minimumStock)
        {
            if (quantity <= 0)
                return "Out of Stock";
            else if (quantity <= minimumStock)
                return "Low Stock";
            else
                return "Normal";
        }
    }
}
