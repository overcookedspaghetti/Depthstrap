// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models.Persistable
{
    internal class RobloxState
    {
        public AppState Player { get; set; } = new();

        public AppState Studio { get; set; } = new();

        public List<string> ModManifest { get; set; } = [];
    }
}