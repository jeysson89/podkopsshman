using System.Collections.Generic;
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

        /// <summary>
        /// Получает ВСЕ значения urltest_proxy_links через регулярное выражение.
        /// </summary>
        public async Task<List<string>> GetUrltestLinksAsync(string tunnelName)
        {
            string output = await ssh.RunCommandAsync($"uci show podkop.{tunnelName}");
            var links = new List<string>();

            foreach (var line in output.Split('\n', System.StringSplitOptions.RemoveEmptyEntries))
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

        /// <summary>
        /// Безопасно добавляет ссылку в список (одинарные кавычки с экранированием).
        /// </summary>
        public async Task AddUrltestLinkAsync(string tunnelName, string link)
        {
            string escaped = EscapeForShell(link);
            await ssh.RunCommandAsync($"uci add_list podkop.{tunnelName}.urltest_proxy_links='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        /// <summary>
        /// Удаляет конкретную ссылку из списка.
        /// </summary>
        public async Task DeleteUrltestLinkAsync(string tunnelName, string link)
        {
            string escaped = EscapeForShell(link);
            await ssh.RunCommandAsync($"uci del_list podkop.{tunnelName}.urltest_proxy_links='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        /// <summary>
        /// Устанавливает значение proxy_string с правильным экранированием.
        /// </summary>
        public async Task SetProxyStringAsync(string tunnelName, string proxyString)
        {
            string escaped = EscapeForShell(proxyString);
            await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.proxy_string='{escaped}'");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        /// <summary>
        /// Экранирует строку для безопасного использования внутри одинарных кавычек в shell.
        /// Заменяет кажду одиночную кавычку на '\'' (закрыть кавычку, добавить \', открыть кавычку).
        /// </summary>
        private string EscapeForShell(string input)
        {
            return input.Replace("'", "'\\''");
        }
    }
}