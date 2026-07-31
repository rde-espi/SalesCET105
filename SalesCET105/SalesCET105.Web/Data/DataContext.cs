using Microsoft.EntityFrameworkCore;
using SalesCET105.Web.Data.Entities;

namespace SalesCET105.Web.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext>options):base(options)
        {
            
        }

        public DbSet<Country> Countries { get; set; }
    }
}
