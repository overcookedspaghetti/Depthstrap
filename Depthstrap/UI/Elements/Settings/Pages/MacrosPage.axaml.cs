// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;

namespace Depthstrap.UI.Elements.Settings.Pages
{
    internal partial class MacrosPage : UserControl
    {
        public MacrosPage()
        {
            InitializeComponent();
            App.DepthRPC?.SetPage("Macros");
        }
    }
}
