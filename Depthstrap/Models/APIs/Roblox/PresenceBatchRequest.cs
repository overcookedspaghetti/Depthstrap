namespace Depthstrap.Models.APIs.Roblox
{
    internal sealed class PresenceBatchRequest
    {
        [JsonPropertyName("userIds")]
        public long[] UserIds { get; set; } = [];
    }
}