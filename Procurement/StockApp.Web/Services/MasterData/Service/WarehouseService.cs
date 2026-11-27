using System.Net.Http.Json;
using StockApp.Shared.Dtos;
using StockApp.Web.Services.MasterData.Interface;

namespace StockApp.Web.Services.MasterData.Service
{
    public class WarehouseService(HttpClient http) : IWarehouseService
    {
        public async Task<List<WarehouseDto>> GetAllAsync() =>
            await http.GetFromJsonAsync<List<WarehouseDto>>("api/warehouses") ?? [];
    }
}