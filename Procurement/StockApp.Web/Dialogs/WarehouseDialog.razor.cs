using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using StockApp.Shared.Dtos;
using StockApp.Web.Services.MasterData.Interface;

namespace StockApp.Web.Dialogs
{
    public partial class WarehouseDialog
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
        [Inject] private IWarehouseService WarehouseService { get; set; } = default!;

        /// <summary>Gudang yang diubah. Kosong (null) berarti menambah gudang baru.</summary>
        [Parameter] public WarehouseDto? Model { get; set; }

        private bool IsEdit => Model is not null;

        // Isian form (salinan, supaya data di tabel tidak berubah sebelum disimpan).
        private string code = "";
        private string name = "";
        private string? address;
        private bool isActive = true;

        private string? codeError;
        private string? nameError;
        private string? error;
        private bool saving;

        protected override void OnInitialized()
        {
            if (Model is null) return;

            code = Model.Code;
            name = Model.Name;
            address = Model.Address;
            isActive = Model.IsActive;
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await SaveAsync();
        }

        private async Task SaveAsync()
        {
            if (saving) return;

            codeError = string.IsNullOrWhiteSpace(code) ? "Kode wajib diisi." : null;
            nameError = string.IsNullOrWhiteSpace(name) ? "Nama gudang wajib diisi." : null;
            error = null;
            if (codeError is not null || nameError is not null) return;

            saving = true;
            try
            {
                var dto = new WarehouseDto
                {
                    Id = Model?.Id ?? 0,
                    Code = code.Trim(),
                    Name = name.Trim(),
                    Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                    IsActive = IsEdit ? isActive : true,
                };

                if (IsEdit) await WarehouseService.UpdateAsync(dto.Id, dto);
                else await WarehouseService.CreateAsync(dto);

                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (HttpRequestException)
            {
                error = "Tidak dapat terhubung ke server.";
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;    // mis. kode sudah dipakai (409 dari API)
            }
            finally
            {
                saving = false;
            }
        }

        private void Cancel() => MudDialog.Cancel();
    }
}