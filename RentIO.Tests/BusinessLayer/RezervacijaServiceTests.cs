using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;
using RentIO.Services;

namespace RentIO.Tests.BusinessLayer
{
    public class RezervacijaServiceTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        private async Task<(ApplicationDbContext ctx, Gost gost, Apartman apartman)> PripremiPodatke(string dbNaziv)
        {
            var ctx = KreirajKontekst(dbNaziv);
            var gost = new Gost { Ime = "Test", Prezime = "Gost", Email = "test@test.com", Telefon = "099" };
            var apartman = new Apartman { Naziv = "Apartman 1", Lokacija = "Zagreb", Cijena = 100 };
            ctx.Gosti.Add(gost);
            ctx.Apartmani.Add(apartman);
            await ctx.SaveChangesAsync();
            return (ctx, gost, apartman);
        }

        [Fact]
        public async Task PostojiPreklapanje_VracaTrue_KadSeTerminiPotpunoPreklapaju()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(PostojiPreklapanje_VracaTrue_KadSeTerminiPotpunoPreklapaju));

            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 10),
                Status = StatusRezervacije.Potvrđena
            });
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            bool rezultat = await service.PostojiPreklapanjeTermina(
                apartman.Id,
                new DateTime(2025, 7, 3),
                new DateTime(2025, 7, 7));

            Assert.True(rezultat);
        }

        [Fact]
        public async Task PostojiPreklapanje_VracaTrue_KadNoviTerminObuhvacaPostojeci()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(PostojiPreklapanje_VracaTrue_KadNoviTerminObuhvacaPostojeci));

            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 5),
                DatumOdlaska = new DateTime(2025, 7, 8),
                Status = StatusRezervacije.NaCekanju
            });
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            bool rezultat = await service.PostojiPreklapanjeTermina(
                apartman.Id,
                new DateTime(2025, 7, 1),
                new DateTime(2025, 7, 15));

            Assert.True(rezultat);
        }

        [Fact]
        public async Task PostojiPreklapanje_VracaFalse_KadSeTerminiNePreklapaju()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(PostojiPreklapanje_VracaFalse_KadSeTerminiNePreklapaju));

            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 10),
                Status = StatusRezervacije.Potvrđena
            });
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            bool rezultat = await service.PostojiPreklapanjeTermina(
                apartman.Id,
                new DateTime(2025, 7, 10),
                new DateTime(2025, 7, 15));

            Assert.False(rezultat);
        }

        [Fact]
        public async Task PostojiPreklapanje_VracaFalse_ZaOtkazanuRezervaciju()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(PostojiPreklapanje_VracaFalse_ZaOtkazanuRezervaciju));

            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 10),
                Status = StatusRezervacije.Otkazana
            });
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            bool rezultat = await service.PostojiPreklapanjeTermina(
                apartman.Id,
                new DateTime(2025, 7, 3),
                new DateTime(2025, 7, 7));

            Assert.False(rezultat);
        }

        [Fact]
        public async Task PostojiPreklapanje_VracaFalse_KadIsklucujeSamuSebe()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(PostojiPreklapanje_VracaFalse_KadIsklucujeSamuSebe));

            var rezervacija = new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 10),
                Status = StatusRezervacije.Potvrđena
            };
            ctx.Rezervacije.Add(rezervacija);
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            bool rezultat = await service.PostojiPreklapanjeTermina(
                apartman.Id,
                new DateTime(2025, 7, 1),
                new DateTime(2025, 7, 10),
                iskljuciRezervacijuId: rezervacija.Id);

            Assert.False(rezultat);
        }

        [Fact]
        public async Task IzracunajUkupnuCijenu_IspravnoRacunaSmjestajIUsluge()
        {
            var (ctx, gost, apartman) = await PripremiPodatke(nameof(IzracunajUkupnuCijenu_IspravnoRacunaSmjestajIUsluge));
            var usluga = new Usluga { Naziv = "Doručak", CijenaPoJedinici = 15 };
            ctx.Usluge.Add(usluga);
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);

            var stavke = new List<RezervacijaUsluga>
            {
                new() { UslugaId = usluga.Id, Kolicina = 3 }
            };

            decimal cijena = await service.IzracunajUkupnuCijenu(
                apartman.Id,
                new DateTime(2025, 7, 1),
                new DateTime(2025, 7, 4),
                stavke);

            Assert.Equal(345, cijena);
        }
    }
}
