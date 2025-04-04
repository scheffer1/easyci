using EasyCI.Data;
using EasyCI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class CIProjectService
    {
        /// <summary>
        /// Obtém todos os projetos CI cadastrados
        /// </summary>
        public async Task<List<CIProject>> GetAllAsync()
        {
            using var context = new EasyCIContext();
            return await context.CIProjects
                .Include(p => p.GitRepository)
                .Include(p => p.DockerContainer)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém um projeto CI pelo ID
        /// </summary>
        public async Task<CIProject?> GetByIdAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.CIProjects
                .Include(p => p.GitRepository)
                .Include(p => p.DockerContainer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Adiciona um novo projeto CI
        /// </summary>
        public async Task<int> AddAsync(CIProject project)
        {
            using var context = new EasyCIContext();
            context.CIProjects.Add(project);
            await context.SaveChangesAsync();
            return project.Id;
        }

        /// <summary>
        /// Atualiza um projeto CI existente
        /// </summary>
        public async Task<bool> UpdateAsync(CIProject project)
        {
            using var context = new EasyCIContext();
            context.Entry(project).State = EntityState.Modified;
            
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExistsAsync(project.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Remove um projeto CI
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var context = new EasyCIContext();
            var project = await context.CIProjects.FindAsync(id);
            
            if (project == null)
            {
                return false;
            }

            context.CIProjects.Remove(project);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Verifica se um projeto CI existe
        /// </summary>
        private async Task<bool> ProjectExistsAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.CIProjects.AnyAsync(e => e.Id == id);
        }
    }
}
