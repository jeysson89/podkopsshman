using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SshTunnelApp.Services
{
    public class PodkopTunnelEditorService
    {
        private SshService ssh;

        public PodkopTunnelEditorService(SshService sshService)
        {
            ssh = sshService;
        }

        public async Task<string> RunCommandAsync(string command)
        {
            return await ssh.RunCommandAsync(command);
        }

        public async Task<List<string>> GetUrltestLinksAsync(string tunnelName)
        {
            string output = await ssh.RunCommandAsync($"uci show podkop.{tunnelName}");
            var links = new List<string>();
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith($"podkop.{tunnelName}.urltest_proxy_links="))
                {
                    string val = trimmed.Substring(trimmed.IndexOf('=') + 1);
                    var matches = Regex.Matches(val, @"'([^']*)'");
                    foreach (Match m in matches)
                    {
                        string link = m.Groups[1].Value;
                        if (!string.IsNullOrEmpty(link))
                            links.Add(link);
                    }
                }
            }
            return links;
        }

        public async Task<string> GetUserDomainsTextAsync(string tunnelName)
        {
            string output = await ssh.RunCommandAsync($"uci get podkop.{tunnelName}.user_domains_text");
            return output.Trim();
        }

        /// <summary>
        /// Нормализует ввод доменов: разделяет по пробелам, запятым, переносам строк,
        /// удаляет пустые строки и дубли, возвращает строку с '\n' в качестве разделителя.
        /// </summary>
        public string NormalizeDomainsText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            var separators = new[] { ' ', ',', ';', '\n', '\r' };
            var domains = input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                               .Select(d => d.Trim())
                               .Where(d => !string.IsNullOrEmpty(d))
                               .Distinct()
                               .ToList();
            return string.Join("\n", domains);
        }

        public async Task SetUserDomainsTextAsync(string tunnelName, string domainsText)
        {
            string escaped = domainsText.Replace("'", "'\\''");
            await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.user_domains_text='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        public async Task AddUrltestLinkAsync(string tunnelName, string link)
        {
            string escaped = EscapeForShell(link);
            await ssh.RunCommandAsync($"uci add_list podkop.{tunnelName}.urltest_proxy_links='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        public async Task DeleteUrltestLinkAsync(string tunnelName, string link)
        {
            string escaped = EscapeForShell(link);
            await ssh.RunCommandAsync($"uci del_list podkop.{tunnelName}.urltest_proxy_links='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        public async Task SetProxyStringAsync(string tunnelName, string proxyString)
        {
            string escaped = EscapeForShell(proxyString);
            await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.proxy_string='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        private string EscapeForShell(string input)
        {
            return input.Replace("'", "'\\''");
        }
    }
}