using EasyCI.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EasyCI.Data
{
    public class EasyCIContext : DbContext
    {
        public DbSet<GitRepository> GitRepositories { get; set; }
        public DbSet<DockerContainer> DockerContainers { get; set; }
        public DbSet<CIProject> CIProjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasyCI",
                "easyci.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GitRepository>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<DockerContainer>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<CIProject>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<CIProject>()
                .HasOne(c => c.GitRepository)
                .WithMany()
                .HasForeignKey(c => c.GitRepositoryId);

            modelBuilder.Entity<CIProject>()
                .HasOne(c => c.DockerContainer)
                .WithMany()
                .HasForeignKey(c => c.DockerContainerId);
        }
    }
}
