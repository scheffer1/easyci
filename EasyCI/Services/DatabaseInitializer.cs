using EasyCI.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyCI.Services
{
    public class DatabaseInitializer
    {
        public static void Initialize()
        {
            using (var context = new EasyCIContext())
            {
                // Cria o banco de dados se não existir
                context.Database.EnsureCreated();
            }
        }
    }
}
