using Microsoft.AspNetCore.Components;
using MudBlazor;
using StockApp.Shared.Dtos;
using StockApp.Web.Services;

namespace StockApp.Web.Components
{
    public partial class PageHeader
    {
        [Inject] private IPageInfoService PageInfoService { get; set; } = default!;

        /// <summary>Kunci halaman untuk dibaca dari service/DB, mis. "dashboard".</summary>
        [Parameter] public string? PageKey { get; set; }

        /// <summary>Opsional: menimpa judul/deskripsi/ikon dari DB.</summary>
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public string? IconKey { get; set; }

        [Parameter] public bool ShowDate { get; set; } = true;

        /// <summary>Isi menu tombol "…". Kalau kosong, tombolnya tidak tampil.</summary>
        [Parameter] public RenderFragment? Actions { get; set; }

        private PageInfoDto? _info;
        private string? _loadedKey;
        private bool _ready;

        private static readonly string[] Months =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember",
        };

        // Kunci ikon (disimpan di DB) -> ikon MudBlazor.
        private static readonly Dictionary<string, string> IconMap = new()
        {
            ["dashboard"] = Icons.Material.Filled.Dashboard,
            ["inventory"] = Icons.Material.Filled.Inventory,
            ["truck"] = Icons.Material.Filled.LocalShipping,
            ["warehouse"] = Icons.Material.Filled.Warehouse,
            ["category"] = Icons.Material.Filled.Category,
            ["po"] = Icons.Material.Filled.Description,
            ["inbox"] = Icons.Material.Filled.MoveToInbox,
            ["outbox"] = Icons.Material.Filled.Outbox,
            ["opname"] = Icons.Material.Filled.FactCheck,
            ["chart"] = Icons.Material.Filled.Assessment,
            ["book"] = Icons.Material.Filled.MenuBook,
            ["swap"] = Icons.Material.Filled.SwapVert,
            ["users"] = Icons.Material.Filled.People,
            ["history"] = Icons.Material.Filled.History,
            ["key"] = Icons.Material.Filled.VpnKey,
            ["bug"] = Icons.Material.Filled.BugReport,
        };

        private string DisplayTitle => Title ?? _info?.Title ?? PageKey ?? "";
        private string DisplayDescription => Description ?? _info?.Description ?? "";

        private string IconSvg =>
            IconMap.TryGetValue(IconKey ?? _info?.Icon ?? "", out var svg) ? svg : Icons.Material.Filled.Description;

        private string Today
        {
            get
            {
                var d = DateTime.Today;
                return $"{d.Day} {Months[d.Month - 1]} {d.Year}";
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrWhiteSpace(PageKey) && PageKey != _loadedKey)
            {
                _info = await PageInfoService.GetAsync(PageKey);
                _loadedKey = PageKey;
            }

            _ready = true;
        }
    }
}