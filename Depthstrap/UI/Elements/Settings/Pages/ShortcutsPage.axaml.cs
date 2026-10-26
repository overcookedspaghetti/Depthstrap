// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;

namespace Depthstrap.UI.Elements.Settings.Pages
{
    internal partial class ShortcutsPage : UserControl
    {
        public ShortcutsPage()
        {
            InitializeComponent();
            App.DepthRPC?.SetPage("Shortcut");

            Loaded += (_, _) =>
            {
                ShortcutsGrid.ColumnDefinitions[1].Width = OperatingSystem.IsWindows()
                    ? new GridLength(1, GridUnitType.Star)
                    : new GridLength(0);
            };
        }
    }
}