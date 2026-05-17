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
        public DbSet<Gost> Gosti { get; set; }
        public DbSet<Usluga> Usluge { get; set; }
        public DbSet<Rezervacija> Rezervacije { get; set; }
        public DbSet<RezervacijaUsluga> RezervacijaUsluge { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rezervacija>()
                .HasOne(r => r.Gost)
                .WithMany(g => g.Rezervacije)
                .HasForeignKey(r => r.GostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rezervacija>()
                .HasOne(r => r.Apartman)
                .WithMany(a => a.Rezervacije)
                .HasForeignKey(r => r.ApartmanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RezervacijaUsluga>()
                .HasOne(ru => ru.Rezervacija)
                .WithMany(r => r.RezervacijaUsluge)
                .HasForeignKey(ru => ru.RezervacijaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RezervacijaUsluga>()
                .HasOne(ru => ru.Usluga)
                .WithMany(u => u.RezervacijaUsluge)
                .HasForeignKey(ru => ru.UslugaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rezervacija>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
