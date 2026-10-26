// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Controls;
using Depthstrap.UI.ViewModels.Settings;

namespace Depthstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Implementation of IDialogService for FastFlags editing
    /// </summary>
    internal class FastFlagsDialogService(MainWindowViewModel mainVm) : IDialogService
    {
        private readonly MainWindowViewModel _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));

        public Task OpenFastFlagEditorAsync()
        {
            _mainVm.NavigateToFastFlagEditorCommand.Execute(null);
            return Task.CompletedTask;
        }
    }

    internal partial class FastFlagsPage : UserControl
    {
        public FastFlagsPage()
        {
            InitializeComponent();
            App.DepthRPC?.SetPage("FastFlags Settings");
        }
    }
}