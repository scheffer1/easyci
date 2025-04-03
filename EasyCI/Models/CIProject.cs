using System;

namespace EasyCI.Models
{
    /// <summary>
    /// Modelo para associar repositórios Git com containers Docker
    /// </summary>
    public class CIProject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GitRepositoryId { get; set; }
        public int DockerContainerId { get; set; }
        public string ComposeFilePath { get; set; } = "docker-compose.yml";
        public bool AutoBuild { get; set; } = true;
        public DateTime LastBuildDate { get; set; }
        public string Status { get; set; } = "Não iniciado";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        
        // Propriedades de navegação (serão utilizadas com Entity Framework)
        public GitRepository? GitRepository { get; set; }
        public DockerContainer? DockerContainer { get; set; }
    }
}
