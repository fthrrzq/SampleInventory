using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SampleInventory.Database;
using SampleInventory.Dtos;
using SampleInventory.Services;

namespace SampleInventory.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _movementService;

        public StockMovementController(IStockMovementService movementService)
        {
            _movementService = movementService;
        }

        [HttpPost]
        public async Task<ActionResult<StockMovement>> CreateMovement(CreateStockMovementDto dto)
        {
            try
            {
                var movement = await _movementService.CreateMovementAsync(dto, User.Identity?.Name ?? "System");
                return Ok(movement);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/receive")]
        public async Task<IActionResult> ReceiveMovement(int id)
        {
            var result = await _movementService.ReceiveMovementAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }
    }
}
