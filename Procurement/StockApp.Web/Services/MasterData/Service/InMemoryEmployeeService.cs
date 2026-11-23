using StockApp.Shared.Dtos;

namespace StockApp.Web.Services
{
    // SEMENTARA: data contoh di memori. Nanti diganti ApiEmployeeService (HttpClient -> API -> SQL Server).
    public class InMemoryEmployeeService : IEmployeeService
    {
        private static readonly List<EmployeeDto> Data = new()
        {
            new() { Id = 1, Nik = "EMP-001", Name = "Ian Keefe", Email = "ian@perusahaan.co.id", Position = "IT Manager", Warehouse = "Gudang Utama", Role = "Administrator" },
            new() { Id = 2, Nik = "EMP-002", Name = "Budi Santoso", Email = "budi@perusahaan.co.id", Position = "Staf Gudang", Warehouse = "Gudang Utama", Role = "Staf Gudang" },
            new() { Id = 3, Nik = "EMP-003", Name = "Sari Wulandari", Email = "sari@perusahaan.co.id", Position = "Staf Pembelian", Warehouse = "Gudang B", Role = "Pembelian" },
            new() { Id = 4, Nik = "EMP-004", Name = "Andi Pratama", Email = "andi@perusahaan.co.id", Position = "Kepala Gudang", Warehouse = "Gudang B", Role = "Admin Gudang" },
            new() { Id = 5, Nik = "EMP-005", Name = "Rina Marlina", Email = "rina@perusahaan.co.id", Position = "Direktur", Warehouse = "-", Role = "Owner", IsActive = false },
        };

        public Task<List<EmployeeDto>> GetAllAsync() =>
            Task.FromResult(Data.Select(e => e.Clone()).OrderBy(e => e.Nik).ToList());

        public Task<string?> SaveAsync(EmployeeDto employee)
        {
            if (Data.Any(e => e.Id != employee.Id && e.Nik.Equals(employee.Nik, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult<string?>($"NIK {employee.Nik} sudah dipakai.");

            if (employee.Id == 0)
            {
                employee.Id = Data.Count == 0 ? 1 : Data.Max(e => e.Id) + 1;
                Data.Add(employee.Clone());
            }
            else
            {
                var i = Data.FindIndex(e => e.Id == employee.Id);
                if (i >= 0) Data[i] = employee.Clone();
            }

            return Task.FromResult<string?>(null);
        }
    }
}