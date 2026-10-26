// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models.BloxstrapRPC
{
    class RichPresenceImage
    {
        [JsonPropertyName("assetId")]
        public ulong? AssetId { get; set; }

        [JsonPropertyName("hoverText")]
        public string? HoverText { get; set; }

        [JsonPropertyName("clear")]
        public bool Clear { get; set; } = false;

        [JsonPropertyName("reset")]
        public bool Reset { get; set; } = false;
    }
}
