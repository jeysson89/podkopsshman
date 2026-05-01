using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SshTunnelApp.Models;

namespace SshTunnelApp.Services
{
    public class RouterManagerService
    {
        private readonly string filePath;
        private List<RouterConnection> routers = new();

        public RouterManagerService()
        {
            // Храним файл рядом с exe, но в реальности лучше AppData
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            filePath = Path.Combine(appDir, "routers.json");
            Load();
        }

        public List<RouterConnection> GetRouters() => new(routers);

        public void AddRouter(RouterConnection router)
        {
            routers.Add(router);
            Save();
        }

        public void DeleteRouter(string name)
        {
            routers.RemoveAll(r => r.Name == name);
            Save();
        }

        public RouterConnection? GetRouter(string name)
        {
            return routers.FirstOrDefault(r => r.Name == name);
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(routers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private void Load()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    routers = JsonSerializer.Deserialize<List<RouterConnection>>(json) ?? new();
                }
                catch { routers = new(); }
            }
        }
    }
}