using Microsoft.AspNetCore.Components;
using MudBlazor;
using StockApp.Shared.Dtos;

namespace StockApp.Web.Dialogs
{
    public partial class EmployeeDialog
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public EmployeeDto Model { get; set; } = new();

        private string? _error;

        private static readonly string[] Warehouses = { "Gudang Utama", "Gudang B", "Gudang C", "-" };
        private static readonly string[] Roles = { "Administrator", "Admin Gudang", "Staf Gudang", "Pembelian", "Owner" };

        private void Cancel() => MudDialog.Cancel();

        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(Model.Nik) || string.IsNullOrWhiteSpace(Model.Name))
            {
                _error = "NIK dan nama wajib diisi.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(Model.Email) && !Model.Email.Contains('@'))
            {
                _error = "Format email tidak valid.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.Role))
            {
                _error = "Role wajib dipilih.";
                return;
            }

            MudDialog.Close(DialogResult.Ok(Model));
        }
    }
}