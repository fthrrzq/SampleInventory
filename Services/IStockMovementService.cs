using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;

namespace SampleInventory.Services
{
    public interface IStockMovementService
    {
        Task<StockMovement> CreateMovementAsync(CreateStockMovementDto dto, string createdBy);
        Task<bool> ReceiveMovementAsync(int movementId);
        Task<List<StockMovement>> GetMovementsByWarehouseAsync(int warehouseId);
    }
    public class StockMovementService : IStockMovementService
    {
        private readonly ApplicationDbContext _context;

        public StockMovementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement> CreateMovementAsync(CreateStockMovementDto dto, string createdBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generate movement number
                var movementNumber = $"MOV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

                var movement = new StockMovement
                {
                    MovementNumber = movementNumber,
                    ProductId = dto.ProductId,
                    SourceWarehouseId = dto.SourceWarehouseId,
                    DestinationWarehouseId = dto.DestinationWarehouseId,
                    Quantity = dto.Quantity,
                    Status = MovementStatus.Sent,
                    MovementDate = DateTime.Now,
                    Notes = dto.Notes,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.Now
                };

                _context.StockMovements.Add(movement);

                // Kurangi stok di source warehouse
                var sourceInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.WarehouseId == dto.SourceWarehouseId && i.ProductId == dto.ProductId);

                if (sourceInventory != null)
                {
                    sourceInventory.Quantity -= dto.Quantity;
                    sourceInventory.LastUpdated = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return movement;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<List<StockMovement>> GetMovementsByWarehouseAsync(int warehouseId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ReceiveMovementAsync(int movementId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var movement = await _context.StockMovements
                    .FirstOrDefaultAsync(m => m.Id == movementId && m.Status == MovementStatus.Sent);

                if (movement == null)
                    return false;

                // Update status movement
                movement.Status = MovementStatus.Received;
                movement.ReceivedAt = DateTime.Now;

                // Tambah stok di destination warehouse
                var destinationInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.WarehouseId == movement.DestinationWarehouseId && i.ProductId == movement.ProductId);

                if (destinationInventory != null)
                {
                    destinationInventory.Quantity += movement.Quantity;
                    destinationInventory.LastUpdated = DateTime.Now;
                }
                else
                {
                    // Buat inventory baru jika belum ada
                    _context.Inventories.Add(new Inventory
                    {
                        WarehouseId = movement.DestinationWarehouseId,
                        ProductId = movement.ProductId,
                        Quantity = movement.Quantity,
                        LastUpdated = DateTime.Now
                    });
                }

                // Jika destination adalah Bar, update juga BarInventory
                var destinationWarehouse = await _context.Warehouses.FindAsync(movement.DestinationWarehouseId);
                if (destinationWarehouse?.Name == "Hawaii" || destinationWarehouse?.Name.Contains("Bar") == true)
                {
                    // Update Bar inventory (default ke BAR location)
                    await UpdateBarInventory(movement.DestinationWarehouseId, movement.ProductId, "BAR", movement.Quantity);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdateBarInventory(int warehouseId, int productId, string barLocation, int quantity)
        {
            var barInventory = await _context.BarInventories
                .FirstOrDefaultAsync(b => b.WarehouseId == warehouseId &&
                                          b.ProductId == productId &&
                                          b.BarLocation == barLocation);

            if (barInventory != null)
            {
                barInventory.Quantity += quantity;
                barInventory.LastUpdated = DateTime.Now;
            }
            else
            {
                _context.BarInventories.Add(new BarInventory
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    BarLocation = barLocation,
                    Quantity = quantity,
                    LastUpdated = DateTime.Now
                });
            }
        }
    }
}