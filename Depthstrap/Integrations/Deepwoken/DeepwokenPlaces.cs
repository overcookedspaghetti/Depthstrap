// SPDX-FileCopyrightText: 2026 Depthstrap
//
// SPDX-License-Identifier: MPL-2.0

namespace Depthstrap.Integrations.Deepwoken
{
    /// <summary>
    /// Maps Deepwoken place IDs to a readable location, based only on which server Roblox joined.
    /// IDs come from https://develop.roblox.com/v1/universes/1359573625/places
    /// </summary>
    internal static class DeepwokenPlaces
    {
        public const long UniverseId = 1359573625;

        private static readonly Dictionary<long, string> Locations = new()
        {
            [4111023553] = "In the main menu",
            [5735553160] = "In the Depths",
            [137000980549032] = "In the Depths",
            [6032399813] = "In the Etrean Luminant",
            [6473861193] = "In the Eastern Luminant",
            [86761619761103] = "In the Eastern Luminant",
            [89993015161920] = "At the Boundary",
            [8668476218] = "In a dungeon",
            [6832944305] = "Fighting in the Arena",
            [8712356310] = "In a 1v1 lobby",
            [12522881268] = "In a 1v1 lobby (Europe)",
            [12522889886] = "In a 1v1 lobby (NA East)",
            [12559711136] = "In a 1v1 lobby (NA West)",
            [12522892314] = "In a 1v1 lobby (Asia)",
            [13789332988] = "In a 1v1 lobby (Oceania)",
            [132160566322181] = "In a 1v1 lobby (South America)",
            [9528956055] = "In a 2v2 lobby",
            [6832934465] = "In a 3v3 lobby",
            [70979303281827] = "In the Gladiator lobby",
            [18139512873] = "In the Fools lobby",
            [13891478131] = "In Battle Royale",
            [14299563111] = "In a Battle Royale lobby (Solos)",
            [14910041900] = "In a Battle Royale lobby (Duos)",
            [15043797592] = "In a Battle Royale lobby (Trios)",
            [15043798946] = "In a Battle Royale lobby (Squads)",
        };

        public static bool IsDeepwoken(long universeId) => universeId == UniverseId;

        /// <summary>
        /// Returns a location label for a Deepwoken place, or a generic label for places added after this list.
        /// Returns null for any other game.
        /// </summary>
        public static string? GetLocation(long universeId, long placeId)
        {
            if (!IsDeepwoken(universeId))
                return null;

            return Locations.TryGetValue(placeId, out string? location) ? location : "Somewhere in Lumen";
        }
    }
}
