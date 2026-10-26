using Depthstrap.Enums.AppStoragePresets;

namespace Depthstrap.Extensions;

internal class RobloxThemeEx
{
    public static IReadOnlyCollection<RobloxTheme> Selections =>
    [
        RobloxTheme.Default,
        RobloxTheme.Light,
        RobloxTheme.Dark
    ];
}
