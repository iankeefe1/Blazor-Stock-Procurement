using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace StockApp.Web.Components.Filters
{
    public partial class ColumnFilter
    {
        [Parameter, EditorRequired] public string Title { get; set; } = "";
        [Parameter, EditorRequired] public ColumnFilterState State { get; set; } = default!;
        [Parameter] public EventCallback OnChanged { get; set; }

        // Teks pilihan untuk kolom true/false, mis. Aktif / Nonaktif.
        [Parameter] public string TrueLabel { get; set; } = "Ya";
        [Parameter] public string FalseLabel { get; set; } = "Tidak";

        private bool open;
        private ColumnFilterState draft = default!;   // salinan kerja; baru dipakai setelah "Terapkan"

        private void Toggle()
        {
            if (open) { open = false; return; }

            draft = new ColumnFilterState(State.Type);
            draft.CopyFrom(State);
            open = true;
        }

        private void Close() => open = false;

        private async Task ApplyAsync()
        {
            State.CopyFrom(draft);
            open = false;
            await OnChanged.InvokeAsync();
        }

        private async Task ClearAsync()
        {
            State.Clear();
            open = false;
            await OnChanged.InvokeAsync();
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await ApplyAsync();
        }
    }
}