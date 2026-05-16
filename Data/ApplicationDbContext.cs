using Microsoft.EntityFrameworkCore;
using RentIO.Models;

namespace RentIO.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apartman> Apartmani { get; set; }
    }
}