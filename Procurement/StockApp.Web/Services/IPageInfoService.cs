using StockApp.Shared.Dtos;

namespace StockApp.Web.Services
{
    public interface IPageInfoService
    {
        Task<PageInfoDto?> GetAsync(string key);
    }
}