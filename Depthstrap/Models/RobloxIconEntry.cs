// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

using Avalonia.Media;
using Depthstrap.UI.ViewModels;

namespace Depthstrap.Models
{
    internal class RobloxIconEntry : NotifyPropertyChangedViewModel
    {
        public RobloxIcon IconType { get; set; }
        public IImage ImageSource => IconType.GetIcon().GetImageSource();
        public void RefreshImage() => OnPropertyChanged(nameof(ImageSource));
    }
}