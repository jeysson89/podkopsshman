using System.Threading.Tasks;

namespace SshTunnelApp.Services
{
    public class PodkopStatusService
    {
        private SshService ssh;

        public PodkopStatusService(SshService sshService)
        {
            ssh = sshService;
        }

        /// <summary>
        /// Возвращает статус Podkop: "running" или "stopped"
        /// </summary>
        public async Task<string> GetStatusAsync()
        {
            // Основной способ – через init-скрипт
            string initStatus = await ssh.RunCommandAsync("/etc/init.d/podkop status");
            initStatus = initStatus.Trim().ToLower();

            if (initStatus.Contains("running"))
                return "running";
            if (initStatus.Contains("stopped"))
                return "stopped";

            // Запасной вариант – поиск процесса по имени
            string pid = await ssh.RunCommandAsync("pidof podkop");
            pid = pid.Trim();
            if (!string.IsNullOrEmpty(pid))
                return "running";

            // Ещё один запасной вариант – ps | grep
            pid = await ssh.RunCommandAsync("ps | grep -v grep | grep podkop | awk '{print $1}'");
            pid = pid.Trim();
            if (!string.IsNullOrEmpty(pid))
                return "running";

            return "stopped";
        }
    }
}