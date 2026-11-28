using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Api.Querying;
using StockApp.Data;
using StockApp.Data.Entities;
using StockApp.Shared.Dtos;
using StockApp.Shared.Dtos.Query;
using System.Linq.Expressions;

namespace StockApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/warehouses")]
    public class WarehousesController(AppDbContext db) : ControllerBase
    {
        // Kolom yang boleh difilter dari halaman (nama kolom = Field di FilterCriterion).
        private static readonly Dictionary<string, FilterField<Warehouse>> FilterColumns =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = FilterFields.Text<Warehouse>(x => x.Code),
                ["name"] = FilterFields.Text<Warehouse>(x => x.Name),
                ["address"] = FilterFields.Text<Warehouse>(x => x.Address),
                ["isActive"] = FilterFields.Bool<Warehouse>(x => x.IsActive),
            };

        private static readonly Expression<Func<Warehouse, WarehouseDto>> ToDto = e => new WarehouseDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            Address = e.Address,
            IsActive = e.IsActive,
        };

        /// <summary>Semua data tanpa filter (untuk pilihan dropdown di halaman lain).</summary>
        [HttpGet]
        public async Task<List<WarehouseDto>> GetAll() =>
            await db.Warehouses
                .AsNoTracking()
                .OrderBy(e => e.Code).ThenBy(e => e.Id)
                .Select(ToDto)
                .ToListAsync();

        /// <summary>Daftar dengan filter per kolom dan paging, diproses di SQL.</summary>
        [HttpPost("search")]
        public async Task<ActionResult<PagedResult<WarehouseDto>>> Search(PagedRequest request)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            IQueryable<Warehouse> query = db.Warehouses.AsNoTracking();
            try
            {
                query = query.ApplyFilters(request.Filters, FilterColumns);
            }
            catch (FilterException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.Code).ThenBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToListAsync();

            return new PagedResult<WarehouseDto> { Items = items, TotalCount = total };
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WarehouseDto>> GetById(int id)
        {
            var dto = await db.Warehouses
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            if (dto is null) return NotFound();
            return dto;
        }

        [HttpPost]
        public async Task<ActionResult<WarehouseDto>> Create(WarehouseDto dto)
        {
            var entity = new Warehouse
            {
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                IsActive = dto.IsActive,
            };
            db.Warehouses.Add(entity);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Data tidak bisa disimpan. Pastikan kode atau nama belum dipakai." });
            }

            dto.Id = entity.Id;
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, WarehouseDto dto)
        {
            var entity = await db.Warehouses.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null) return NotFound();

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.IsActive = dto.IsActive;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Data tidak bisa disimpan. Pastikan kode atau nama belum dipakai." });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await db.Warehouses.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null) return NotFound();

            // Dinonaktifkan, bukan dihapus, supaya riwayat transaksi tetap utuh.
            entity.IsActive = false;
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}