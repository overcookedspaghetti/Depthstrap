// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Depthstrap.UI.Elements.Base;
using Depthstrap.UI.ViewModels.Dialogs;

namespace Depthstrap.UI.Elements.Dialogs
{
    internal partial class CommunityModInfoDialog : AvaloniaWindow
    {
        public CommunityModInfoViewModel? ViewModel { get; }

        public CommunityModInfoDialog()
        {
            InitializeComponent();
        }

        public CommunityModInfoDialog(CommunityMod mod) : this()
        {
            ViewModel = new CommunityModInfoViewModel(mod, this);
            DataContext = ViewModel;

            ViewModel.Initialize();
        }
    }
}