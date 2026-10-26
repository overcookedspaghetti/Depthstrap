// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Depthstrap.UI.Utility
{
    internal partial class AvaloniaEditTheme : Styles
    {
        public AvaloniaEditTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
