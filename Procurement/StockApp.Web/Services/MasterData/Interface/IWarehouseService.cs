using StockApp.Shared.Dtos;

namespace StockApp.Web.Services.MasterData.Interface
{
    public interface IWarehouseService
    {
        Task<List<WarehouseDto>> GetAllAsync();
    }
}