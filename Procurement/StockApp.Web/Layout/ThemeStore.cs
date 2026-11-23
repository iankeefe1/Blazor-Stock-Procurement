using MudBlazor;

namespace StockApp.Web.Layout
{
    // Satu tema dipakai MainLayout dan EmptyLayout. Ubah warna di sini.
    public static class ThemeStore
    {
        public static readonly MudTheme Theme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1565C0",
                Secondary = "#00897B",
                Background = "#F4F6F9",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#424242",
                DrawerBackground = "#FFFFFF",
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#64B5F6",
                Secondary = "#4DB6AC",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
            },
        };
    }
}
