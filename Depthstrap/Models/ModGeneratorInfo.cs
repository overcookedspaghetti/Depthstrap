using System.Text.Json.Nodes;

namespace Depthstrap.Models
{
    internal sealed class ModGeneratorInfo
    {
        public string DepthstrapVersion { get; set; } = "";
        public string RobloxVersion { get; set; } = "";
        public string RobloxVersionHash { get; set; } = "";
        public JsonNode? ColorsUsed { get; set; }
    }
}