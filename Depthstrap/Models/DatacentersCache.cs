// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models
{
    internal class DatacentersCache
    {
        [JsonPropertyName("regions")]
        public Dictionary<string, List<int>> Regions { get; set; } = [];

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }
    }
}
