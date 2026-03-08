using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;

namespace SampleInventory.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/product
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto productDto)
        {
            try
            {
                // Validasi input
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Cek apakah kode produk sudah ada
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == productDto.Code);

                if (existingProduct != null)
                {
                    return BadRequest(new { message = $"Product with code '{productDto.Code}' already exists" });
                }

                // Buat produk baru
                var product = new Product
                {
                    Code = productDto.Code.ToUpper(),
                    Name = productDto.Name,
                    Description = productDto.Description ?? string.Empty,
                    Unit = productDto.Unit,
                    Price = productDto.Price,
                    MinimumStock = productDto.MinimumStock,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Log activity
                _logger.LogInformation($"Product created: {product.Code} - {product.Name}");

                // Return response dengan CreatedAtAction
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "An error occurred while creating the product" });
            }
        }

        // GET: api/product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Products.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    p.Code.ToLower().Contains(search) ||
                    p.Name.ToLower().Contains(search));
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .OrderBy(p => p.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Unit = p.Unit,
                    Price = p.Price,
                    MinimumStock = p.MinimumStock,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    TotalStock = _context.Inventories.Where(i => i.ProductId == p.Id).Sum(i => i.Quantity)
                })
                .ToListAsync();

            // Add pagination metadata in response headers
            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(products);
        }

        // PUT: api/product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with id {id} not found" });
                }

                // Update product properties
                product.Name = productDto.Name ?? product.Name;
                product.Description = productDto.Description ?? product.Description;
                product.Unit = productDto.Unit ?? product.Unit;

                if (productDto.Price.HasValue)
                    product.Price = productDto.Price.Value;

                if (productDto.MinimumStock.HasValue)
                    product.MinimumStock = productDto.MinimumStock.Value;

                if (productDto.IsActive.HasValue)
                    product.IsActive = productDto.IsActive.Value;

                product.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product updated: {product.Code} - {product.Name}");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, new { message = "An error occurred while updating the product" });
            }
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with id {id} not found" });
                }

                // Cek apakah produk memiliki stock movement
                var hasMovements = await _context.StockMovements.AnyAsync(m => m.ProductId == id);
                if (hasMovements)
                {
                    // Soft delete - hanya nonaktifkan
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Product deactivated successfully" });
                }

                // Hard delete - hapus permanent jika belum ada transaksi
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product deleted: {product.Code} - {product.Name}");

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, new { message = "An error occurred while deleting the product" });
            }
        }

        // GET: api/product/{id}/stock-history
        [HttpGet("{id}/stock-history")]
        public async Task<ActionResult<IEnumerable<StockMovementHistoryDto>>> GetProductStockHistory(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var query = _context.StockMovements
                .Include(m => m.SourceWarehouse)
                .Include(m => m.DestinationWarehouse)
                .Where(m => m.ProductId == id);

            if (startDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.MovementDate <= endDate.Value);
            }

            var history = await query
                .OrderByDescending(m => m.MovementDate)
                .Select(m => new StockMovementHistoryDto
                {
                    MovementNumber = m.MovementNumber,
                    Date = m.MovementDate,
                    SourceWarehouse = m.SourceWarehouse.Name,
                    DestinationWarehouse = m.DestinationWarehouse.Name,
                    Quantity = m.Quantity,
                    Status = m.Status.ToString(),
                    Notes = m.Notes
                })
                .ToListAsync();

            return Ok(history);
        }

        // GET: api/product/{id}/stock-by-warehouse
        [HttpGet("{id}/stock-by-warehouse")]
        public async Task<ActionResult<IEnumerable<WarehouseStockDto>>> GetProductStockByWarehouse(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var stocks = await _context.Inventories
                .Include(i => i.Warehouse)
                .Where(i => i.ProductId == id && i.Quantity > 0)
                .Select(i => new WarehouseStockDto
                {
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    WarehouseType = i.Warehouse.Type.ToString(),
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated
                })
                .ToListAsync();

            return Ok(stocks);
        }
    }
}
