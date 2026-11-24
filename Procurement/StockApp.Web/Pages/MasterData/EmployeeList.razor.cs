using Microsoft.AspNetCore.Components;
using MudBlazor;
using StockApp.Shared.Dtos;
using StockApp.Web.Dialogs;

//using StockApp.Web.Dialogs;
using StockApp.Web.Services;

namespace StockApp.Web.Pages.MasterData
{
    public partial class EmployeeList
    {
        [Inject] private IEmployeeService Service { get; set; } = default!;
        [Inject] private IDialogService Dialogs { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        private List<EmployeeDto> _items = new();
        private string _search = "";
        private bool _loading = true;

        protected override async Task OnInitializedAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            _loading = true;
            _items = await Service.GetAllAsync();
            _loading = false;
        }

        private bool Matches(EmployeeDto e) =>
            string.IsNullOrWhiteSpace(_search)
            || e.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || e.Nik.Contains(_search, StringComparison.OrdinalIgnoreCase)
            || e.Email.Contains(_search, StringComparison.OrdinalIgnoreCase);

        private Task AddAsync() => OpenDialogAsync("Tambah karyawan", new EmployeeDto { Warehouse = "Gudang Utama", Role = "Staf Gudang" });

        private Task EditAsync(EmployeeDto e) => OpenDialogAsync("Ubah karyawan", e.Clone());

        private async Task OpenDialogAsync(string title, EmployeeDto model)
        {
            var parameters = new DialogParameters<EmployeeDialog> { { x => x.Model, model } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };

            var dialog = await Dialogs.ShowAsync<EmployeeDialog>(title, parameters, options);
            var result = await dialog.Result;

            if (result is { Canceled: false, Data: EmployeeDto saved })
                await SaveAsync(saved);
        }

        private async Task ToggleAsync(EmployeeDto e)
        {
            var copy = e.Clone();
            copy.IsActive = !copy.IsActive;
            await SaveAsync(copy);
        }

        private async Task SaveAsync(EmployeeDto employee)
        {
            var error = await Service.SaveAsync(employee);
            if (error is not null)
            {
                Snackbar.Add(error, Severity.Error);
                return;
            }

            Snackbar.Add("Data karyawan disimpan.", Severity.Success);
            await LoadAsync();
        }
    }
}