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
                DrawerBackground = "#0F2A4A",
                DrawerText = "#FFFFFF",
                DrawerIcon = "#FFFFFF",
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#64B5F6",
                Secondary = "#4DB6AC",
                DrawerBackground = "#0B1A2E",
                DrawerText = "#FFFFFF",
                DrawerIcon = "#FFFFFF",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
            },
        };
    }
}
