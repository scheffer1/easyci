using System;

namespace EasyCI.Models
{
    public class DockerContainer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 2375;
        public string ApiVersion { get; set; } = "v1.41";
        public bool UseTLS { get; set; } = false;
        public string CertificatePath { get; set; } = string.Empty;
        public string RemoteWorkspacePath { get; set; } = "/var/easyci/workspace";
        public bool UseDockerApi { get; set; } = true;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
