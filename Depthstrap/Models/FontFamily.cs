// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models
{
    internal class FontFamily
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("faces")]
        public IEnumerable<FontFace> Faces { get; set; } = null!;
    }
}
