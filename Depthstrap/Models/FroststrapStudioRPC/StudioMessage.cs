// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models.DepthstrapStudioRPC;

internal class StudioMessage
{
    [JsonPropertyName("command")]
    public string StudioCommand { get; set; } = null!;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
