using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Controllers;
using RentIO.Data;
using RentIO.Models;
using RentIO.Services;
using RentIO.ViewModels;

namespace RentIO.Tests.Integration
{
    public class RezervacijeIntegrationTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        private async Task<(Gost gost, Apartman apartman, Usluga usluga)> PripremiPodatke(ApplicationDbContext ctx)
        {
            var gost = new Gost { Ime = "Ivan", Prezime = "Ivić", Email = "ivan@test.com", Telefon = "091" };
            var apartman = new Apartman { Naziv = "Penthouse", Lokacija = "Dubrovnik", Cijena = 200 };
            var usluga = new Usluga { Naziv = "Transfer", CijenaPoJedinici = 50 };
            ctx.Gosti.Add(gost);
            ctx.Apartmani.Add(apartman);
            ctx.Usluge.Add(usluga);
            await ctx.SaveChangesAsync();
            return (gost, apartman, usluga);
        }

        [Fact]
        public async Task KreiranjeRezervacije_SpremaSeBazu_PrezentacijskiSlojPozivaPoslovniSloj()
        {
            var ctx = KreirajKontekst(nameof(KreiranjeRezervacije_SpremaSeBazu_PrezentacijskiSlojPozivaPoslovniSloj));
            var (gost, apartman, _) = await PripremiPodatke(ctx);
            var service = new RezervacijaService(ctx);
            var controller = new RezervacijeController(ctx, service);

            var vm = new RezervacijaViewModel
            {
                GostId = gost.Id,
                ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 9, 1),
                DatumOdlaska = new DateTime(2025, 9, 5)
            };

            var rezultat = await controller.Create(vm);

            Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal(1, await ctx.Rezervacije.CountAsync());
        }

        [Fact]
        public async Task OverbookingValidacija_BlokiraKreiranjeRezervacije_KadTerminZauzet()
        {
            var ctx = KreirajKontekst(nameof(OverbookingValidacija_BlokiraKreiranjeRezervacije_KadTerminZauzet));
            var (gost, apartman, _) = await PripremiPodatke(ctx);

            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 9, 1),
                DatumOdlaska = new DateTime(2025, 9, 10),
                Status = StatusRezervacije.Potvrđena
            });
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);
            var controller = new RezervacijeController(ctx, service);

            var vm = new RezervacijaViewModel
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 9, 5),
                DatumOdlaska = new DateTime(2025, 9, 8)
            };

            var rezultat = await controller.Create(vm);

            Assert.IsType<ViewResult>(rezultat);
            Assert.Equal(1, await ctx.Rezervacije.CountAsync());
        }

        [Fact]
        public async Task BrisanjeRezervacije_UklonjaIStavkeUsluga_CasCadeDelete()
        {
            var ctx = KreirajKontekst(nameof(BrisanjeRezervacije_UklonjaIStavkeUsluga_CasCadeDelete));
            var (gost, apartman, usluga) = await PripremiPodatke(ctx);

            var rezervacija = new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 10, 1),
                DatumOdlaska = new DateTime(2025, 10, 5),
                RezervacijaUsluge = new List<RezervacijaUsluga>
                {
                    new() { UslugaId = usluga.Id, Kolicina = 1 }
                }
            };
            ctx.Rezervacije.Add(rezervacija);
            await ctx.SaveChangesAsync();

            var service = new RezervacijaService(ctx);
            var controller = new RezervacijeController(ctx, service);

            await controller.DeleteConfirmed(rezervacija.Id);

            Assert.Equal(0, await ctx.Rezervacije.CountAsync());
            Assert.Equal(0, await ctx.RezervacijaUsluge.CountAsync());
        }

        [Fact]
        public async Task UkupnaCijena_IspravnoIzracunata_PriKreiranjuRezervacije()
        {
            var ctx = KreirajKontekst(nameof(UkupnaCijena_IspravnoIzracunata_PriKreiranjuRezervacije));
            var (gost, apartman, usluga) = await PripremiPodatke(ctx);

            var service = new RezervacijaService(ctx);
            var controller = new RezervacijeController(ctx, service);

            var vm = new RezervacijaViewModel
            {
                GostId = gost.Id,
                ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 11, 1),
                DatumOdlaska = new DateTime(2025, 11, 4),
                Usluge = new List<RezervacijaUslugaViewModel>
                {
                    new() { UslugaId = usluga.Id, Kolicina = 2 }
                }
            };

            await controller.Create(vm);

            var spasena = await ctx.Rezervacije.FirstOrDefaultAsync();
            Assert.NotNull(spasena);
            Assert.Equal(700, spasena.UkupnaCijena);
        }
    }
}
