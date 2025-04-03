using System;

namespace EasyCI.Models
{
    /// <summary>
    /// Modelo para armazenar informações de repositórios Git
    /// </summary>
    public class GitRepository
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string SshKeyPath { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
