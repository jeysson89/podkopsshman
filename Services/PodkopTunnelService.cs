using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SshTunnelApp.Services
{
    public class PodkopTunnelService
    {
        private SshService ssh;

        public PodkopTunnelService(SshService sshService)
        {
            ssh = sshService;
        }

        /// <summary>
        /// Получить список имён всех туннелей (секций)
        /// </summary>
        public async Task<List<string>> GetTunnelNamesAsync()
        {
            string result = await ssh.RunCommandAsync(
                "uci show podkop | grep '=section' | cut -d'.' -f2 | cut -d'=' -f1"
            );
            return result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .ToList();
        }

        /// <summary>
        /// Получить подробную информацию о туннеле (все UCI-ключи секции)
        /// </summary>
        public async Task<string> GetTunnelDetailsAsync(string tunnelName)
        {
            if (string.IsNullOrWhiteSpace(tunnelName))
                throw new ArgumentException("Имя туннеля не может быть пустым");
            return await ssh.RunCommandAsync($"uci show podkop.{tunnelName}");
        }

        /// <summary>
        /// Добавить новый туннель с базовыми параметрами
        /// </summary>
        public async Task AddTunnelAsync(string tunnelName, string remoteHost, string remotePort, string localPort, string type = "tcp")
        {
            if (string.IsNullOrWhiteSpace(tunnelName))
                throw new ArgumentException("Имя туннеля не может быть пустым");

            // Создаём новую секцию и задаём тип
            await ssh.RunCommandAsync($"uci set podkop.{tunnelName}=section");
            await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.type={type}");

            // Основные параметры (если переданы)
            if (!string.IsNullOrWhiteSpace(remoteHost))
                await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.remote_host={remoteHost}");
            if (!string.IsNullOrWhiteSpace(remotePort))
                await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.remote_port={remotePort}");
            if (!string.IsNullOrWhiteSpace(localPort))
                await ssh.RunCommandAsync($"uci set podkop.{tunnelName}.local_port={localPort}");

            // Сохраняем и перезапускаем Podkop
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }

        /// <summary>
        /// Удалить туннель по имени
        /// </summary>
        public async Task DeleteTunnelAsync(string tunnelName)
        {
            if (string.IsNullOrWhiteSpace(tunnelName))
                throw new ArgumentException("Имя туннеля не может быть пустым");

            await ssh.RunCommandAsync($"uci delete podkop.{tunnelName}");
            await ssh.RunCommandAsync("uci commit podkop");
            await ssh.RunCommandAsync("/etc/init.d/podkop reload");
        }
    }
}