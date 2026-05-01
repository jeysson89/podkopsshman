using System;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SshTunnelApp
{
    public class SshService
    {
        private SshClient? client;

        public bool IsConnected => client?.IsConnected ?? false;

        public async Task ConnectAsync(string host, int port, string username, string password)
        {
            await Task.Run(() =>
            {
                client = new SshClient(host, port, username, password);
                client.Connect();
            });
        }

        public async Task<string> RunCommandAsync(string command)
        {
            if (!IsConnected)
                throw new InvalidOperationException("SSH не подключён.");

            return await Task.Run(() =>
            {
                var cmd = client!.RunCommand(command);
                return cmd.Result;
            });
        }

        public void Disconnect()
        {
            client?.Disconnect();
            client?.Dispose();
        }
    }
}