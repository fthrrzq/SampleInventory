using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleInventory.Database;
using SampleInventory.Dtos;

namespace SampleInventory.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses()
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.ChildWarehouses)
                .Where(w => w.ParentWarehouseId == null)
                .ToListAsync();

            var result = warehouses.Select(w => MapToDto(w));
            return Ok(result);
        }

        [HttpGet("{id}/inventory")]
        public async Task<ActionResult> GetWarehouseInventory(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.WarehouseId == id)
                .ToListAsync();

            return Ok(inventory);
        }

        private WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Code = warehouse.Code,
                Name = warehouse.Name,
                Type = warehouse.Type.ToString(),
                Location = warehouse.Location,
                Children = warehouse.ChildWarehouses?.Select(c => MapToDto(c)).ToList() ?? new List<WarehouseDto>()
            };
        }
    }
}
