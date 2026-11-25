using System.Net.Http.Json;
using StockApp.Shared.Dtos;

namespace StockApp.Web.Services.MasterData.Service
{
    public class EmployeeService(HttpClient http) : IEmployeeService
    {
        public async Task<List<EmployeeDto>> GetAllAsync() =>
            await http.GetFromJsonAsync<List<EmployeeDto>>("api/employees") ?? new();

        public async Task<string?> SaveAsync(EmployeeDto employee)
        {
            try
            {
                var response = employee.Id == 0
                    ? await http.PostAsJsonAsync("api/employees", employee)
                    : await http.PutAsJsonAsync($"api/employees/{employee.Id}", employee);

                return response.IsSuccessStatusCode ? null : await ApiError.ReadAsync(response);
            }
            catch (HttpRequestException)
            {
                return "Tidak dapat terhubung ke server.";
            }
        }
    }
}