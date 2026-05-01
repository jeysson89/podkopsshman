using System.Text;
using System.Threading.Tasks;

namespace SshTunnelApp
{
    public class PodkopService
    {
        private SshService ssh;

        public PodkopService(SshService sshService)
        {
            ssh = sshService;
        }

        /// <summary>
        /// Возвращает полный статус Podkop и список туннелей
        /// </summary>
        public async Task<string> GetFullStatusAsync()
        {
            var sb = new StringBuilder();

            // Проверка, запущен ли Podkop
            string pid = await GetPodkopPidAsync();
            sb.AppendLine(string.IsNullOrEmpty(pid) ? "Podkop: Не запущен" : $"Podkop: Запущен (PID: {pid})");
            sb.AppendLine(new string('-', 40));

            // Список туннелей
            string tunnels = await ssh.RunCommandAsync(
                "uci show podkop | grep '=section' | cut -d'.' -f2 | cut -d'=' -f1"
            );
            var tunnelNames = tunnels.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            sb.AppendLine("Туннели Podkop:");
            if (tunnelNames.Length > 0)
                foreach (var name in tunnelNames)
                    sb.AppendLine($"  • {name}");
            else
                sb.AppendLine("  (нет туннелей)");

            return sb.ToString();
        }

        /// <summary>
        /// Получает PID процесса podkop, используя pidof (или ps как запасной вариант)
        /// </summary>
        private async Task<string> GetPodkopPidAsync()
        {
            string pid = await ssh.RunCommandAsync("pidof podkop");
            pid = pid.Trim();
            if (string.IsNullOrEmpty(pid))
            {
                pid = await ssh.RunCommandAsync("ps | grep -v grep | grep podkop | awk '{print $1}'");
                pid = pid.Trim();
            }
            return pid;
        }
    }
}