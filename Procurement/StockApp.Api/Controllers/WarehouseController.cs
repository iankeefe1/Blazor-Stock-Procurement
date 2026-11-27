using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Data;
using StockApp.Shared.Dtos;

namespace StockApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/warehouses")]
    public class WarehousesController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<List<WarehouseDto>> GetAll() =>
            await db.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Code)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Address = w.Address,
                    IsActive = w.IsActive,
                })
                .ToListAsync();
    }
}