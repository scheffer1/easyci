using EasyCI.Data;
using EasyCI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class DockerContainerService
    {
        /// <summary>
        /// Obtém todos os containers Docker cadastrados
        /// </summary>
        public async Task<List<DockerContainer>> GetAllAsync()
        {
            using var context = new EasyCIContext();
            return await context.DockerContainers.ToListAsync();
        }

        /// <summary>
        /// Obtém um container Docker pelo ID
        /// </summary>
        public async Task<DockerContainer?> GetByIdAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.DockerContainers.FindAsync(id);
        }

        /// <summary>
        /// Adiciona um novo container Docker
        /// </summary>
        public async Task<int> AddAsync(DockerContainer container)
        {
            using var context = new EasyCIContext();
            context.DockerContainers.Add(container);
            await context.SaveChangesAsync();
            return container.Id;
        }

        /// <summary>
        /// Atualiza um container Docker existente
        /// </summary>
        public async Task<bool> UpdateAsync(DockerContainer container)
        {
            using var context = new EasyCIContext();
            context.Entry(container).State = EntityState.Modified;
            
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContainerExistsAsync(container.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Remove um container Docker
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var context = new EasyCIContext();
            var container = await context.DockerContainers.FindAsync(id);
            
            if (container == null)
            {
                return false;
            }

            context.DockerContainers.Remove(container);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Verifica se um container Docker existe
        /// </summary>
        private async Task<bool> ContainerExistsAsync(int id)
        {
            using var context = new EasyCIContext();
            return await context.DockerContainers.AnyAsync(e => e.Id == id);
        }
    }
}
