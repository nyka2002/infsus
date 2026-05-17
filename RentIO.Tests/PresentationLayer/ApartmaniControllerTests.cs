using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Controllers;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Tests.PresentationLayer
{
    public class ApartmaniControllerTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        [Fact]
        public async Task Index_VracaViewSListomApartmana()
        {
            var ctx = KreirajKontekst(nameof(Index_VracaViewSListomApartmana));
            ctx.Apartmani.AddRange(
                new Apartman { Naziv = "Studio A", Lokacija = "Split", Cijena = 80 },
                new Apartman { Naziv = "Vila B", Lokacija = "Hvar", Cijena = 150 }
            );
            await ctx.SaveChangesAsync();
            var controller = new ApartmaniController(ctx);

            var rezultat = await controller.Index(null!);

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Apartman>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_FiltriraPoNazivuILokaciji_KadJeZadanSearchString()
        {
            var ctx = KreirajKontekst(nameof(Index_FiltriraPoNazivuILokaciji_KadJeZadanSearchString));
            ctx.Apartmani.AddRange(
                new Apartman { Naziv = "Studio A", Lokacija = "Split", Cijena = 80 },
                new Apartman { Naziv = "Vila B", Lokacija = "Hvar", Cijena = 150 }
            );
            await ctx.SaveChangesAsync();
            var controller = new ApartmaniController(ctx);

            var rezultat = await controller.Index("Split");

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Apartman>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_GET_VracaView()
        {
            var ctx = KreirajKontekst(nameof(Create_GET_VracaView));
            var controller = new ApartmaniController(ctx);

            var rezultat = controller.Create();

            Assert.IsType<ViewResult>(rezultat);
        }

        [Fact]
        public async Task Create_POST_SpremaSeBazu_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_SpremaSeBazu_IRedirectaNaIndex));
            var controller = new ApartmaniController(ctx);
            var apartman = new Apartman { Naziv = "Novi", Lokacija = "Zadar", Cijena = 100 };

            var rezultat = await controller.Create(apartman);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, await ctx.Apartmani.CountAsync());
        }

        [Fact]
        public async Task Details_VracaNotFound_ZaNepostojeciId()
        {
            var ctx = KreirajKontekst(nameof(Details_VracaNotFound_ZaNepostojeciId));
            var controller = new ApartmaniController(ctx);

            var rezultat = await controller.Details(999);

            Assert.IsType<NotFoundResult>(rezultat);
        }

        [Fact]
        public async Task Edit_POST_AzuriraCijenu_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Edit_POST_AzuriraCijenu_IRedirectaNaIndex));
            var apartman = new Apartman { Naziv = "Studio", Lokacija = "Rijeka", Cijena = 70 };
            ctx.Apartmani.Add(apartman);
            await ctx.SaveChangesAsync();
            var controller = new ApartmaniController(ctx);

            apartman.Cijena = 90;
            var rezultat = await controller.Edit(apartman.Id, apartman);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            var azurirani = await ctx.Apartmani.FindAsync(apartman.Id);
            Assert.Equal(90, azurirani!.Cijena);
        }

        [Fact]
        public async Task Delete_POST_UklonjaApartman_IzBaze()
        {
            var ctx = KreirajKontekst(nameof(Delete_POST_UklonjaApartman_IzBaze));
            var apartman = new Apartman { Naziv = "Za brisanje", Lokacija = "Zagreb", Cijena = 50 };
            ctx.Apartmani.Add(apartman);
            await ctx.SaveChangesAsync();
            var controller = new ApartmaniController(ctx);

            await controller.DeleteConfirmed(apartman.Id);

            Assert.Equal(0, await ctx.Apartmani.CountAsync());
        }
    }
}
