using System;

namespace EasyCI.Models
{
    /// <summary>
    /// Modelo para armazenar informações de containers Docker
    /// </summary>
    public class DockerContainer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 2375;
        public string ApiVersion { get; set; } = "v1.41";
        public bool UseTLS { get; set; } = false;
        public string CertificatePath { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
