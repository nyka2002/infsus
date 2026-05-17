using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Tests.DataLayer
{
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        [Fact]
        public async Task Gost_MozeSeSpremitiIUcitati()
        {
            using var context = KreirajKontekst(nameof(Gost_MozeSeSpremitiIUcitati));

            context.Gosti.Add(new Gost { Ime = "Ana", Prezime = "Anić", Email = "ana@test.com", Telefon = "091" });
            await context.SaveChangesAsync();

            var gost = await context.Gosti.FirstOrDefaultAsync(g => g.Email == "ana@test.com");

            Assert.NotNull(gost);
            Assert.Equal("Ana", gost.Ime);
        }

        [Fact]
        public async Task Usluga_MozeSeSpremitiIUcitati()
        {
            using var context = KreirajKontekst(nameof(Usluga_MozeSeSpremitiIUcitati));

            context.Usluge.Add(new Usluga { Naziv = "Doručak", Opis = "Kontinentalni", CijenaPoJedinici = 15 });
            await context.SaveChangesAsync();

            var usluga = await context.Usluge.FirstOrDefaultAsync(u => u.Naziv == "Doručak");

            Assert.NotNull(usluga);
            Assert.Equal(15, usluga.CijenaPoJedinici);
        }

        [Fact]
        public async Task Rezervacija_MozeSeSpremitiSOdnosimaGostIApartman()
        {
            using var context = KreirajKontekst(nameof(Rezervacija_MozeSeSpremitiSOdnosimaGostIApartman));

            var gost = new Gost { Ime = "Pero", Prezime = "Perić", Email = "pero@test.com", Telefon = "092" };
            var apartman = new Apartman { Naziv = "Studio A", Lokacija = "Split", Cijena = 80 };
            context.Gosti.Add(gost);
            context.Apartmani.Add(apartman);
            await context.SaveChangesAsync();

            context.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id,
                ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 5),
                Status = StatusRezervacije.NaCekanju
            });
            await context.SaveChangesAsync();

            var rezervacija = await context.Rezervacije
                .Include(r => r.Gost)
                .Include(r => r.Apartman)
                .FirstOrDefaultAsync();

            Assert.NotNull(rezervacija);
            Assert.Equal("Pero", rezervacija.Gost!.Ime);
            Assert.Equal("Studio A", rezervacija.Apartman!.Naziv);
        }

        [Fact]
        public async Task RezervacijaUsluga_MozeSeSpremitiSOdnosom()
        {
            using var context = KreirajKontekst(nameof(RezervacijaUsluga_MozeSeSpremitiSOdnosom));

            var gost = new Gost { Ime = "Mia", Prezime = "Mić", Email = "mia@test.com", Telefon = "093" };
            var apartman = new Apartman { Naziv = "Vila", Lokacija = "Hvar", Cijena = 150 };
            var usluga = new Usluga { Naziv = "Čišćenje", CijenaPoJedinici = 30 };
            context.Gosti.Add(gost);
            context.Apartmani.Add(apartman);
            context.Usluge.Add(usluga);
            await context.SaveChangesAsync();

            var rezervacija = new Rezervacija
            {
                GostId = gost.Id,
                ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 8, 1),
                DatumOdlaska = new DateTime(2025, 8, 3),
                RezervacijaUsluge = new List<RezervacijaUsluga>
                {
                    new() { UslugaId = usluga.Id, Kolicina = 2 }
                }
            };
            context.Rezervacije.Add(rezervacija);
            await context.SaveChangesAsync();

            var rezultat = await context.Rezervacije
                .Include(r => r.RezervacijaUsluge)
                .FirstOrDefaultAsync();

            Assert.NotNull(rezultat);
            Assert.Single(rezultat.RezervacijaUsluge);
            Assert.Equal(2, rezultat.RezervacijaUsluge.First().Kolicina);
        }
    }
}
