using System.Text.Json;
using System.Threading.Tasks;
using SshTunnelApp.Models;

namespace SshTunnelApp.Services
{
    public class PodkopDnsService
    {
        private SshService ssh;

        public PodkopDnsService(SshService sshService)
        {
            ssh = sshService;
        }

        public async Task<DnsStatus?> GetDnsStatusAsync()
        {
            string output = await ssh.RunCommandAsync("podkop check_dns_available");
            output = output.Trim();
            if (string.IsNullOrEmpty(output)) return null;

            try
            {
                // Убираем возможный посторонний текст до/после JSON (например, логи)
                int jsonStart = output.IndexOf('{');
                int jsonEnd = output.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd < 0) return null;
                string json = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                return JsonSerializer.Deserialize<DnsStatus>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}