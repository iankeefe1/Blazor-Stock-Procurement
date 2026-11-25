using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Data;
using StockApp.Data.Entities;
using StockApp.Shared.Dtos;

namespace StockApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/employees")]
    public class EmployeesController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<List<EmployeeDto>> GetAll() =>
            await db.Employees.AsNoTracking()
                .OrderBy(e => e.Nik)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Nik = e.Nik,
                    Name = e.Name,
                    Email = e.Email,
                    Position = e.Position,
                    Warehouse = e.Warehouse,
                    Role = e.Role,
                    IsActive = e.IsActive,
                })
                .ToListAsync();

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<EmployeeDto>> Create(EmployeeDto dto)
        {
            if (await db.Employees.AnyAsync(e => e.Nik == dto.Nik))
                return Conflict(new { message = $"NIK {dto.Nik} sudah dipakai." });

            var employee = new Employee();
            Apply(employee, dto);
            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            dto.Id = employee.Id;
            return CreatedAtAction(nameof(GetAll), dto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, EmployeeDto dto)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            if (await db.Employees.AnyAsync(e => e.Nik == dto.Nik && e.Id != id))
                return Conflict(new { message = $"NIK {dto.Nik} sudah dipakai." });

            Apply(employee, dto);
            await db.SaveChangesAsync();
            return NoContent();
        }

        private static void Apply(Employee e, EmployeeDto dto)
        {
            e.Nik = dto.Nik.Trim();
            e.Name = dto.Name.Trim();
            e.Email = dto.Email.Trim();
            e.Position = dto.Position.Trim();
            e.Warehouse = dto.Warehouse;
            e.Role = dto.Role;
            e.IsActive = dto.IsActive;
        }
    }
}