using StockApp.Shared.Dtos;

namespace StockApp.Web.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync();

        Task<string?> SaveAsync(EmployeeDto employee);
    }
}