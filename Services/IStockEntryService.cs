using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;

namespace SampleInventory.Services
{
    public interface IStockEntryService
    {
        Task<StockEntry> CreateStockEntryAsync(CreateStockEntryDto dto, string createdBy);
        Task<StockEntry> CompleteStockEntryAsync(int entryId);
        Task<List<StockEntryDto>> GetStockEntriesAsync(int warehouseId, DateTime? startDate, DateTime? endDate);
        Task<StockEntryDetailDto> GetStockEntryDetailAsync(int entryId);
    }

    public class StockEntryService : IStockEntryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockEntryService> _logger;

        public StockEntryService(ApplicationDbContext context, ILogger<StockEntryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StockEntry> CreateStockEntryAsync(CreateStockEntryDto dto, string createdBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generate entry number
                var entryNumber = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                var stockEntry = new StockEntry
                {
                    EntryNumber = entryNumber,
                    WarehouseId = dto.WarehouseId,
                    Supplier = dto.Supplier,
                    InvoiceNumber = dto.InvoiceNumber,
                    EntryDate = dto.EntryDate ?? DateTime.Now,
                    Notes = dto.Notes ?? string.Empty,
                    Status = EntryStatus.Draft,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.Now
                };

                _context.StockEntries.Add(stockEntry);
                await _context.SaveChangesAsync();

                // Add items
                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {item.ProductId} not found");
                    }

                    var entryItem = new StockEntryItem
                    {
                        StockEntryId = stockEntry.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PurchasePrice = item.PurchasePrice,
                        ExpiryDate = item.ExpiryDate,
                        BatchNumber = item.BatchNumber ?? string.Empty,
                        Notes = item.Notes ?? string.Empty
                    };

                    _context.StockEntryItems.Add(entryItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Stock entry created: {entryNumber}");

                return stockEntry;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating stock entry");
                throw;
            }
        }

        public async Task<StockEntry> CompleteStockEntryAsync(int entryId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var stockEntry = await _context.StockEntries
                    .Include(e => e.Items)
                    .FirstOrDefaultAsync(e => e.Id == entryId && e.Status == EntryStatus.Draft);

                if (stockEntry == null)
                {
                    throw new Exception($"Stock entry with ID {entryId} not found or already completed");
                }

                // Update inventory for each item
                foreach (var item in stockEntry.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.WarehouseId == stockEntry.WarehouseId &&
                                                 i.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity += item.Quantity;
                        inventory.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            WarehouseId = stockEntry.WarehouseId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            LastUpdated = DateTime.Now
                        });
                    }

                    // Create stock movement record
                    var movement = new StockMovement
                    {
                        MovementNumber = $"IN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        ProductId = item.ProductId,
                        SourceWarehouseId = stockEntry.WarehouseId, // Using same warehouse for internal movement
                        DestinationWarehouseId = stockEntry.WarehouseId,
                        Quantity = item.Quantity,
                        Status = MovementStatus.Received,
                        MovementDate = DateTime.Now,
                        Notes = $"Stock entry from {stockEntry.Supplier} - Invoice: {stockEntry.InvoiceNumber}",
                        CreatedBy = stockEntry.CreatedBy,
                        CreatedAt = DateTime.Now,
                        ReceivedAt = DateTime.Now
                    };

                    _context.StockMovements.Add(movement);
                }

                // Update stock entry status
                stockEntry.Status = EntryStatus.Completed;
                stockEntry.ReceivedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Stock entry completed: {stockEntry.EntryNumber}");

                return stockEntry;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing stock entry");
                throw;
            }
        }

        public async Task<List<StockEntryDto>> GetStockEntriesAsync(int warehouseId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.StockEntries
                .Include(e => e.Items)
                .Where(e => e.WarehouseId == warehouseId);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EntryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EntryDate <= endDate.Value);
            }

            var entries = await query
                .OrderByDescending(e => e.EntryDate)
                .Select(e => new StockEntryDto
                {
                    Id = e.Id,
                    EntryNumber = e.EntryNumber,
                    Supplier = e.Supplier,
                    InvoiceNumber = e.InvoiceNumber,
                    EntryDate = e.EntryDate,
                    Status = e.Status.ToString(),
                    TotalItems = e.Items.Count,
                    TotalQuantity = e.Items.Sum(i => i.Quantity),
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return entries;
        }

        public async Task<StockEntryDetailDto> GetStockEntryDetailAsync(int entryId)
        {
            var entry = await _context.StockEntries
                .Include(e => e.Warehouse)
                .Include(e => e.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(e => e.Id == entryId);

            if (entry == null)
            {
                throw new Exception($"Stock entry with ID {entryId} not found");
            }

            return new StockEntryDetailDto
            {
                Id = entry.Id,
                EntryNumber = entry.EntryNumber,
                WarehouseName = entry.Warehouse.Name,
                Supplier = entry.Supplier,
                InvoiceNumber = entry.InvoiceNumber,
                EntryDate = entry.EntryDate,
                Notes = entry.Notes,
                Status = entry.Status.ToString(),
                CreatedBy = entry.CreatedBy,
                CreatedAt = entry.CreatedAt,
                ReceivedAt = entry.ReceivedAt,
                Items = entry.Items.Select(i => new StockEntryItemDto
                {
                    ProductId = i.ProductId,
                    ProductCode = i.Product.Code,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    PurchasePrice = i.PurchasePrice,
                    TotalPrice = i.Quantity * i.PurchasePrice,
                    BatchNumber = i.BatchNumber,
                    ExpiryDate = i.ExpiryDate,
                    Notes = i.Notes
                }).ToList()
            };
        }
    }
}
