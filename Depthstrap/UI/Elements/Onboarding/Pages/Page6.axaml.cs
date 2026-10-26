// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;
using Depthstrap.UI.ViewModels.Onboarding;

namespace Depthstrap.UI.Elements.Onboarding.Pages
{
    internal partial class Page6 : UserControl
    {
        public Page6()
        {
            DataContext = new Page6ViewModel();
            InitializeComponent();
        }
    }
}
