// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;

namespace Depthstrap.UI.Elements.Settings.Pages;

internal partial class BehaviourPage : UserControl
{
    public BehaviourPage()
    {
        InitializeComponent();

        App.DepthRPC?.SetPage("Bootstrapper");
    }
}
