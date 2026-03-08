using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;
using SampleInventory.Services;
using System.ComponentModel.DataAnnotations;

namespace SampleInventory.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockEntryController : ControllerBase
    {
        private readonly IStockEntryService _stockEntryService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockEntryController> _logger;

        public StockEntryController(
            IStockEntryService stockEntryService,
            ApplicationDbContext context,
            ILogger<StockEntryController> logger)
        {
            _stockEntryService = stockEntryService;
            _context = context;
            _logger = logger;
        }

        // POST: api/stockentry
        [HttpPost]
        public async Task<ActionResult<StockEntry>> CreateStockEntry([FromBody] CreateStockEntryDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validasi warehouse adalah Gudang Pusat
                var warehouse = await _context.Warehouses.FindAsync(dto.WarehouseId);
                if (warehouse == null)
                {
                    return BadRequest(new { message = "Warehouse not found" });
                }

                if (warehouse.Type != WarehouseType.Pusat)
                {
                    return BadRequest(new { message = "Stock entry can only be created for Gudang Pusat" });
                }

                var stockEntry = await _stockEntryService.CreateStockEntryAsync(
                    dto,
                    User.Identity?.Name ?? "System"
                );

                return CreatedAtAction(nameof(GetStockEntry), new { id = stockEntry.Id }, stockEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock entry");
                return StatusCode(500, new { message = "Error creating stock entry", error = ex.Message });
            }
        }

        // PUT: api/stockentry/{id}/complete
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteStockEntry(int id)
        {
            try
            {
                var stockEntry = await _stockEntryService.CompleteStockEntryAsync(id);
                return Ok(stockEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing stock entry");
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/stockentry/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StockEntryDetailDto>> GetStockEntry(int id)
        {
            try
            {
                var entry = await _stockEntryService.GetStockEntryDetailAsync(id);
                return Ok(entry);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/stockentry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockEntryDto>>> GetStockEntries(
            [FromQuery] int warehouseId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var entries = await _stockEntryService.GetStockEntriesAsync(warehouseId, startDate, endDate);
            return Ok(entries);
        }

        // POST: api/stockentry/quick-add
        [HttpPost("quick-add")]
        public async Task<ActionResult> QuickAddStock([FromBody] QuickAddStockDto dto)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Cari atau buat inventory
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.WarehouseId == dto.WarehouseId &&
                                             i.ProductId == dto.ProductId);

                if (inventory != null)
                {
                    inventory.Quantity += dto.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
                else
                {
                    _context.Inventories.Add(new Inventory
                    {
                        WarehouseId = dto.WarehouseId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                        LastUpdated = DateTime.Now
                    });
                }

                // Buat stock movement record
                var movement = new StockMovement
                {
                    MovementNumber = $"QA-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    ProductId = dto.ProductId,
                    SourceWarehouseId = dto.WarehouseId,
                    DestinationWarehouseId = dto.WarehouseId,
                    Quantity = dto.Quantity,
                    Status = MovementStatus.Received,
                    MovementDate = DateTime.Now,
                    Notes = dto.Notes ?? "Quick add stock",
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedAt = DateTime.Now,
                    ReceivedAt = DateTime.Now
                };

                _context.StockMovements.Add(movement);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Stock added successfully",
                    movementNumber = movement.MovementNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick add stock");
                return StatusCode(500, new { message = "Error adding stock", error = ex.Message });
            }
        }
    }

    // DTO untuk quick add
    public class QuickAddStockDto
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
