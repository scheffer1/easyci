using EasyCI.Data;
using EasyCI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class GitRepositoryService
    {
        /// <summary>
        /// Obtém todos os repositórios Git cadastrados
        /// </summary>
        public async Task<List<GitRepository>> GetAllAsync()
        {
            using var context = new EasyCIContext();
            return await context.GitRepositories.ToListAsync();
        }

        /// <summary>
        /// Obtém um repositório Git pelo ID
        /// </summary>
        public async Task<GitRepository?> GetByIdAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.GitRepositories.FindAsync(id);
        }

        /// <summary>
        /// Adiciona um novo repositório Git
        /// </summary>
        public async Task<int> AddAsync(GitRepository repository)
        {
            using var context = new EasyCIContext();
            context.GitRepositories.Add(repository);
            await context.SaveChangesAsync();
            return repository.Id;
        }

        /// <summary>
        /// Atualiza um repositório Git existente
        /// </summary>
        public async Task<bool> UpdateAsync(GitRepository repository)
        {
            using var context = new EasyCIContext();
            context.Entry(repository).State = EntityState.Modified;
            
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RepositoryExistsAsync(repository.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Remove um repositório Git
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var context = new EasyCIContext();
            var repository = await context.GitRepositories.FindAsync(id);
            
            if (repository == null)
            {
                return false;
            }

            context.GitRepositories.Remove(repository);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Verifica se um repositório Git existe
        /// </summary>
        private async Task<bool> RepositoryExistsAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.GitRepositories.AnyAsync(e => e.Id == id);
        }
    }
}
