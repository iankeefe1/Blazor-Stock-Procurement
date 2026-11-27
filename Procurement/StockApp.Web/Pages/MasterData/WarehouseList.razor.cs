using Microsoft.AspNetCore.Components;
using StockApp.Shared.Dtos;
using StockApp.Web.Components.Filters;
using StockApp.Web.Services.MasterData.Interface;

namespace StockApp.Web.Pages.MasterData
{
    public partial class WarehouseList
    {
        [Inject] private IWarehouseService WarehouseService { get; set; } = default!;

        private List<WarehouseDto> items = [];
        private bool loading = true;
        private string? error;
        private int currentPage;

        // Satu filter per kolom: teks = mengandung/sama dengan, bool = sama dengan.
        private readonly ColumnFilterState fCode = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fName = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fAddress = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fActive = new(ColumnFilterType.Bool);

        private IEnumerable<WarehouseDto> Filtered =>
            items.Where(w => fCode.Matches(w.Code)
                          && fName.Matches(w.Name)
                          && fAddress.Matches(w.Address)
                          && fActive.Matches(w.IsActive));

        private IEnumerable<(ColumnFilterState State, string Text)> ActiveFilters
        {
            get
            {
                if (fCode.IsActive) yield return (fCode, fCode.Describe("Kode"));
                if (fName.IsActive) yield return (fName, fName.Describe("Nama"));
                if (fAddress.IsActive) yield return (fAddress, fAddress.Describe("Alamat"));
                if (fActive.IsActive) yield return (fActive, fActive.Describe("Status", "Aktif", "Nonaktif"));
            }
        }

        private bool AnyFilter => ActiveFilters.Any();

        private void OnFilterChanged() => currentPage = 0;

        private void ClearFilter(ColumnFilterState state)
        {
            state.Clear();
            currentPage = 0;
        }

        private void ClearAll()
        {
            foreach (var f in new[] { fCode, fName, fAddress, fActive }) f.Clear();
            currentPage = 0;
        }

        protected override async Task OnInitializedAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            loading = true;
            error = null;
            try
            {
                items = await WarehouseService.GetAllAsync();
            }
            catch (HttpRequestException)
            {
                error = "Tidak dapat terhubung ke server.";
            }
            catch (Exception)
            {
                error = "Gagal memuat data gudang.";
            }
            finally
            {
                loading = false;
            }
        }
    }
}