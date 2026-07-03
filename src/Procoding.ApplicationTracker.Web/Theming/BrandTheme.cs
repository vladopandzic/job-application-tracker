using MudBlazor;

namespace Procoding.ApplicationTracker.Web.Theming;

/// <summary>
/// The product's MudBlazor theme (brand palette + typography). Applied via
/// <c>&lt;MudThemeProvider Theme="BrandTheme.Default" /&gt;</c> in the layouts.
/// </summary>
public static class BrandTheme
{
    public static readonly MudTheme Default = new()
    {
        Palette = new PaletteLight
        {
            Primary = "#4F46E5",          // indigo
            Secondary = "#0EA5E9",        // sky
            Info = "#2563EB",
            Success = "#16A34A",
            Warning = "#F59E0B",
            Error = "#DC2626",
            Background = "#F7F8FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1F2430",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#374151",
            DrawerIcon = "#4F46E5",
            TextPrimary = "#1F2430",
            TextSecondary = "#6B7280",
            ActionDefault = "#6B7280",
            Divider = "#E5E7EB",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "Inter", "Helvetica Neue", "Helvetica", "Arial", "sans-serif" }
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        }
    };
}
