using System.Threading.Tasks;

namespace SshTunnelApp.Services
{
    public class PodkopServiceManager
    {
        private SshService ssh;

        public PodkopServiceManager(SshService sshService)
        {
            ssh = sshService;
        }

        public async Task StartAsync() => await RunPodkopCommand("start");
        public async Task StopAsync() => await RunPodkopCommand("stop");
        public async Task ReloadAsync() => await RunPodkopCommand("reload");
        public async Task RestartAsync() => await RunPodkopCommand("restart");

        /// <summary>
        /// Перезапускает DNS-компонент Podkop (вызывает reload, который обновляет конфиг sing-box с DNS)
        /// </summary>
        public async Task RestartDnsAsync() => await RunPodkopCommand("reload");

        private async Task RunPodkopCommand(string command)
        {
            await ssh.RunCommandAsync($"podkop {command}");
        }
    }
}