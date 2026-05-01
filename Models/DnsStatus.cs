using System.Text.Json.Serialization;

namespace SshTunnelApp.Models
{
    public class DnsStatus
    {
        [JsonPropertyName("dns_type")]
        public string DnsType { get; set; } = string.Empty;

        [JsonPropertyName("dns_server")]
        public string DnsServer { get; set; } = string.Empty;

        [JsonPropertyName("dns_status")]
        public int DnsStatusValue { get; set; }

        [JsonPropertyName("dns_on_router")]
        public int DnsOnRouter { get; set; }

        [JsonPropertyName("bootstrap_dns_server")]
        public string BootstrapDnsServer { get; set; } = string.Empty;

        [JsonPropertyName("bootstrap_dns_status")]
        public int BootstrapDnsStatus { get; set; }

        [JsonPropertyName("dhcp_config_status")]
        public int DhcpConfigStatus { get; set; }

        // Вспомогательное свойство для отображения
        public string DnsStatusText => DnsStatusValue == 1 ? "✓ Доступен" : "✗ Недоступен";
        public string BootstrapStatusText => BootstrapDnsStatus == 1 ? "✓ Доступен" : "✗ Недоступен";
        public string DhcpConfigText => DhcpConfigStatus == 1 ? "✓ Настроен" : "✗ Не настроен";
        public string DnsOnRouterText => DnsOnRouter == 1 ? "✓ DNS на роутере" : "✗ Не используется";
    }
}