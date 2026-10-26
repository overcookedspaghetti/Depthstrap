// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Depthstrap.UI.ViewModels.Dialogs;

namespace Depthstrap.UI.Elements.Dialogs
{
    internal partial class ManualCookieDialog : Base.AvaloniaWindow
    {
        public ManualCookieDialogViewModel ViewModel { get; }

        public ManualCookieDialog()
        {
            ViewModel = new ManualCookieDialogViewModel(this);
            DataContext = ViewModel;

            InitializeComponent();
        }
    }
}