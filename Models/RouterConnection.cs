namespace SshTunnelApp.Models
{
    public class RouterConnection
    {
        public string Name { get; set; } = string.Empty;  // удобное имя роутера
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = "root";
        public string Password { get; set; } = string.Empty;
    }
}