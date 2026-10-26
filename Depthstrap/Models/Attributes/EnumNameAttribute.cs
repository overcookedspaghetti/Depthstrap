// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class EnumNameAttribute : Attribute
    {
        public string? StaticName { get; set; }
        public string? FromTranslation { get; set; }
    }
}