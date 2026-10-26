// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;

namespace Depthstrap.UI.Elements.Settings.Pages;

internal partial class LinuxSettingsPage : UserControl
{
    public LinuxSettingsPage()
    {
        InitializeComponent();

        App.DepthRPC?.SetPage("Sober Settings");
    }
}
