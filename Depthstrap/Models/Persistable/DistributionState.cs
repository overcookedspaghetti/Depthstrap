// SPDX-FileCopyrightText: 2026 Froststrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Models.Persistable
{
    internal class DistributionState
    {
        public string VersionGuid { get; set; } = string.Empty;

        public Dictionary<string, string> PackageHashes { get; set; } = [];

        public List<string> ModManifest { get; set; } = [];
    }
}