using StockApp.Shared.Dtos;
using StockApp.Shared.Dtos.Query;

namespace StockApp.Web.Services.MasterData.Interface
{
    public interface IWarehouseService
    {
        Task<List<WarehouseDto>> GetAllAsync();
        Task<PagedResult<WarehouseDto>> SearchAsync(PagedRequest request, CancellationToken ct = default);
        Task<WarehouseDto?> GetByIdAsync(int id);
        Task<WarehouseDto> CreateAsync(WarehouseDto dto);
        Task UpdateAsync(int id, WarehouseDto dto);
        Task DeleteAsync(int id);
    }
}