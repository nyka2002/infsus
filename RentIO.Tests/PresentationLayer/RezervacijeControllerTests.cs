using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RentIO.Controllers;
using RentIO.Data;
using RentIO.Models;
using RentIO.Services;
using RentIO.ViewModels;

namespace RentIO.Tests.PresentationLayer
{
    public class RezervacijeControllerTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        private async Task<(Gost gost, Apartman apartman)> DodajTestnePodatke(ApplicationDbContext ctx)
        {
            var gost = new Gost { Ime = "Test", Prezime = "Gost", Email = "test@test.com", Telefon = "099" };
            var apartman = new Apartman { Naziv = "Apartman 1", Lokacija = "Split", Cijena = 100 };
            ctx.Gosti.Add(gost);
            ctx.Apartmani.Add(apartman);
            await ctx.SaveChangesAsync();
            return (gost, apartman);
        }

        [Fact]
        public async Task Index_VracaViewSListomRezervacija()
        {
            var ctx = KreirajKontekst(nameof(Index_VracaViewSListomRezervacija));
            var (gost, apartman) = await DodajTestnePodatke(ctx);
            ctx.Rezervacije.Add(new Rezervacija
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 5)
            });
            await ctx.SaveChangesAsync();

            var mockService = new Mock<IRezervacijaService>();
            var controller = new RezervacijeController(ctx, mockService.Object);

            var rezultat = await controller.Index(null!);

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Rezervacija>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Index_FiltriraPoDatumuDolaska_KadJeZadanSearchString()
        {
            var ctx = KreirajKontekst(nameof(Index_FiltriraPoDatumuDolaska_KadJeZadanSearchString));
            var (gost, apartman) = await DodajTestnePodatke(ctx);
            ctx.Rezervacije.AddRange(
                new Rezervacija { GostId = gost.Id, ApartmanId = apartman.Id, DatumDolaska = new DateTime(2025, 7, 1), DatumOdlaska = new DateTime(2025, 7, 5) },
                new Rezervacija { GostId = gost.Id, ApartmanId = apartman.Id, DatumDolaska = new DateTime(2025, 8, 1), DatumOdlaska = new DateTime(2025, 8, 5) }
            );
            await ctx.SaveChangesAsync();

            var mockService = new Mock<IRezervacijaService>();
            var controller = new RezervacijeController(ctx, mockService.Object);

            var rezultat = await controller.Index("Test");

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Rezervacija>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Create_GET_VracaViewSPrazanimViewModelom()
        {
            var ctx = KreirajKontekst(nameof(Create_GET_VracaViewSPrazanimViewModelom));
            await DodajTestnePodatke(ctx);

            var mockService = new Mock<IRezervacijaService>();
            var controller = new RezervacijeController(ctx, mockService.Object);

            var rezultat = await controller.Create();

            var view = Assert.IsType<ViewResult>(rezultat);
            Assert.IsType<RezervacijaViewModel>(view.Model);
        }

        [Fact]
        public async Task Create_POST_VracaViewSGreskom_KadJeDatumOdlaskaPrijeIliIstiKaoDolazak()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_VracaViewSGreskom_KadJeDatumOdlaskaPrijeIliIstiKaoDolazak));
            await DodajTestnePodatke(ctx);

            var mockService = new Mock<IRezervacijaService>();
            var controller = new RezervacijeController(ctx, mockService.Object);

            var vm = new RezervacijaViewModel
            {
                GostId = 1, ApartmanId = 1,
                DatumDolaska = new DateTime(2025, 7, 10),
                DatumOdlaska = new DateTime(2025, 7, 5)
            };

            var rezultat = await controller.Create(vm);

            var view = Assert.IsType<ViewResult>(rezultat);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_POST_VracaViewSGreskom_KadPostojiPreklapanjeTermina()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_VracaViewSGreskom_KadPostojiPreklapanjeTermina));
            var (gost, apartman) = await DodajTestnePodatke(ctx);

            var mockService = new Mock<IRezervacijaService>();
            mockService
                .Setup(s => s.PostojiPreklapanjeTermina(apartman.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .ReturnsAsync(true);

            var controller = new RezervacijeController(ctx, mockService.Object);

            var vm = new RezervacijaViewModel
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 5)
            };

            var rezultat = await controller.Create(vm);

            var view = Assert.IsType<ViewResult>(rezultat);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_POST_RedirectNaIndex_KadSuPodaciValjani()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_RedirectNaIndex_KadSuPodaciValjani));
            var (gost, apartman) = await DodajTestnePodatke(ctx);

            var mockService = new Mock<IRezervacijaService>();
            mockService
                .Setup(s => s.PostojiPreklapanjeTermina(apartman.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .ReturnsAsync(false);
            mockService
                .Setup(s => s.IzracunajUkupnuCijenu(apartman.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<RezervacijaUsluga>>()))
                .ReturnsAsync(400m);

            var controller = new RezervacijeController(ctx, mockService.Object);

            var vm = new RezervacijaViewModel
            {
                GostId = gost.Id, ApartmanId = apartman.Id,
                DatumDolaska = new DateTime(2025, 7, 1),
                DatumOdlaska = new DateTime(2025, 7, 5)
            };

            var rezultat = await controller.Create(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Delete_VracaNotFound_ZaNepostojeciId()
        {
            var ctx = KreirajKontekst(nameof(Delete_VracaNotFound_ZaNepostojeciId));
            var mockService = new Mock<IRezervacijaService>();
            var controller = new RezervacijeController(ctx, mockService.Object);

            var rezultat = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(rezultat);
        }
    }
}
