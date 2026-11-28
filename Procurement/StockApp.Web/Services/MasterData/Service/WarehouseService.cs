// ===== StockApp.Web/Services/MasterData/Service/WarehouseService.cs =====
using System.Net;
using System.Net.Http.Json;
using StockApp.Shared.Dtos;
using StockApp.Shared.Dtos.Query;
using StockApp.Web.Services.MasterData.Interface;

namespace StockApp.Web.Services.MasterData.Service
{
    public class WarehouseService(HttpClient http) : IWarehouseService
    {
        private const string Url = "api/warehouses";

        public async Task<List<WarehouseDto>> GetAllAsync() =>
            await http.GetFromJsonAsync<List<WarehouseDto>>(Url) ?? [];

        public async Task<PagedResult<WarehouseDto>> SearchAsync(PagedRequest request, CancellationToken ct = default)
        {
            var response = await http.PostAsJsonAsync($"{Url}/search", request, ct);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<PagedResult<WarehouseDto>>(ct) ?? new();
        }

        public async Task<WarehouseDto?> GetByIdAsync(int id)
        {
            var response = await http.GetAsync($"{Url}/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<WarehouseDto>();
        }

        public async Task<WarehouseDto> CreateAsync(WarehouseDto dto)
        {
            var response = await http.PostAsJsonAsync(Url, dto);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<WarehouseDto>() ?? dto;
        }

        public async Task UpdateAsync(int id, WarehouseDto dto)
        {
            var response = await http.PutAsJsonAsync($"{Url}/{id}", dto);
            await EnsureSuccessAsync(response);
        }

        public async Task DeleteAsync(int id)
        {
            var response = await http.DeleteAsync($"{Url}/{id}");
            await EnsureSuccessAsync(response);
        }

        // Kesalahan dari API (mis. 409 kode sudah dipakai) dilempar sebagai pesan yang siap ditampilkan.
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(await ApiError.ReadAsync(response));
        }
    }
}

// ===== Daftarkan di StockApp.Web/Program.cs =====
// builder.Services.AddScoped<IWarehouseService, WarehouseService>();