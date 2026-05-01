using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SshTunnelApp.Models
{
    public class ProxyInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("history")]
        public List<ProxyDelayEntry> History { get; set; } = new();

        [JsonPropertyName("now")]
        public string? Now { get; set; }   // для селекторов, какой прокси активен

        // Вычисляемое — последняя задержка или null
        public int? LastDelay
        {
            get
            {
                if (History.Count > 0)
                    return History[^1].Delay;
                return null;
            }
        }

        // Текстовое представление задержки
        public string DelayDisplay => LastDelay.HasValue ? $"{LastDelay} мс" : "—";

        // Доступность: есть ли история (значит, проверялся и ответил)
        public bool HasData => History.Count > 0;
    }

    public class ProxyDelayEntry
    {
        [JsonPropertyName("delay")]
        public int Delay { get; set; }
    }
}