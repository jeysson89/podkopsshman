using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SshTunnelApp.Models;

namespace SshTunnelApp.Services
{
    public class PodkopProxyService
    {
        private SshService ssh;

        public PodkopProxyService(SshService sshService)
        {
            ssh = sshService;
        }

        /// <summary>
        /// Получает список всех прокси с их состоянием через Clash API.
        /// </summary>
        public async Task<List<ProxyInfo>> GetProxiesAsync()
        {
            string output = await ssh.RunCommandAsync("podkop clash_api get_proxies");
            output = output.Trim();
            if (string.IsNullOrEmpty(output)) return new List<ProxyInfo>();

            try
            {
                using var doc = JsonDocument.Parse(output);
                var root = doc.RootElement;
                if (root.TryGetProperty("proxies", out var proxiesElement))
                {
                    var list = new List<ProxyInfo>();
                    foreach (var proxy in proxiesElement.EnumerateObject())
                    {
                        var info = JsonSerializer.Deserialize<ProxyInfo>(proxy.Value.GetRawText());
                        if (info != null)
                        {
                            info.Name = proxy.Name; // ключ – имя прокси
                            list.Add(info);
                        }
                    }
                    return list;
                }
            }
            catch { /* игнорируем ошибки парсинга */ }

            return new List<ProxyInfo>();
        }
    }
}