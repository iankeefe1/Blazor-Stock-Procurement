using Microsoft.AspNetCore.Components;
using MudBlazor;
using StockApp.Shared.Dtos;
using StockApp.Shared.Dtos.Query;
using StockApp.Web.Components.Filters;
using StockApp.Web.Dialogs;
using StockApp.Web.Services.MasterData.Interface;

namespace StockApp.Web.Pages.MasterData
{
    public partial class WarehouseList
    {
        [Inject] private IWarehouseService WarehouseService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        private MudTable<WarehouseDto> table = default!;
        private bool loading = true;
        private string? error;
        private int totalCount;

        // Satu filter per kolom. Nama kolom di BuildFilters harus sama dengan yang didaftarkan di API.
        private readonly ColumnFilterState fCode = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fName = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fAddress = new(ColumnFilterType.Text);
        private readonly ColumnFilterState fActive = new(ColumnFilterType.Bool);

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

        private List<FilterCriterion> BuildFilters()
        {
            var list = new List<FilterCriterion>();
            foreach (var (field, state) in new[]
            {
                ("code", fCode), ("name", fName), ("address", fAddress), ("isActive", fActive),
            })
            {
                var criterion = state.ToCriterion(field);
                if (criterion is not null) list.Add(criterion);
            }
            return list;
        }

        // Dipanggil MudTable setiap kali halaman dibuka atau berpindah halaman: data diminta ke API.
        private async Task<TableData<WarehouseDto>> LoadServerData(TableState state, CancellationToken ct)
        {
            loading = true;
            error = null;
            try
            {
                var result = await WarehouseService.SearchAsync(new PagedRequest
                {
                    Page = state.Page + 1,          // MudTable mulai dari 0, API dari 1
                    PageSize = state.PageSize,
                    Filters = BuildFilters(),
                }, ct);

                totalCount = result.TotalCount;
                return new TableData<WarehouseDto> { Items = result.Items, TotalItems = result.TotalCount };
            }
            catch (OperationCanceledException)
            {
                return new TableData<WarehouseDto> { Items = [], TotalItems = totalCount };
            }
            catch (HttpRequestException)
            {
                error = "Tidak dapat terhubung ke server.";
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
            }
            catch (Exception)
            {
                error = "Gagal memuat data gudang.";
            }
            finally
            {
                loading = false;
            }

            totalCount = 0;
            return new TableData<WarehouseDto> { Items = [], TotalItems = 0 };
        }

        // Filter berubah: kembali ke halaman pertama lalu minta data lagi ke API.
        private async Task OnFilterChanged()
        {
            if (table.CurrentPage != 0)
                table.NavigateTo(Page.First);          // otomatis memuat ulang data
            else
                await table.ReloadServerData();
        }

        private async Task ClearFilter(ColumnFilterState state)
        {
            state.Clear();
            await OnFilterChanged();
        }

        private async Task ClearAll()
        {
            foreach (var f in new[] { fCode, fName, fAddress, fActive }) f.Clear();
            await OnFilterChanged();
        }

        // ---- tambah / ubah / nonaktifkan ----

        /// <summary>model = null berarti tambah gudang baru.</summary>
        private async Task OpenDialogAsync(WarehouseDto? model)
        {
            var parameters = new DialogParameters<WarehouseDialog> { { x => x.Model, model } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };

            var dialog = await DialogService.ShowAsync<WarehouseDialog>(
                model is null ? "Tambah gudang" : "Ubah gudang", parameters, options);
            var result = await dialog.Result;

            if (result is { Canceled: false })
            {
                Snackbar.Add(model is null ? "Gudang ditambahkan." : "Perubahan disimpan.", Severity.Success);
                await table.ReloadServerData();        // tetap di halaman dan filter yang sama
            }
        }

        private async Task DeleteAsync(WarehouseDto warehouse)
        {
            var confirmed = await DialogService.ShowMessageBoxAsync(
                "Nonaktifkan gudang",
                $"Gudang {warehouse.Code} - {warehouse.Name} akan dinonaktifkan. " +
                "Data tetap tersimpan dan bisa diaktifkan lagi lewat tombol Ubah.",
                yesText: "Nonaktifkan",
                cancelText: "Batal");

            if (confirmed != true) return;

            try
            {
                await WarehouseService.DeleteAsync(warehouse.Id);
                Snackbar.Add("Gudang dinonaktifkan.", Severity.Success);
                await table.ReloadServerData();
            }
            catch (HttpRequestException)
            {
                Snackbar.Add("Tidak dapat terhubung ke server.", Severity.Error);
            }
            catch (InvalidOperationException ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}