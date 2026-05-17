using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Controllers;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Tests.PresentationLayer
{
    public class UslugeControllerTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        [Fact]
        public async Task Index_VracaViewSListomUsluga()
        {
            var ctx = KreirajKontekst(nameof(Index_VracaViewSListomUsluga));
            ctx.Usluge.AddRange(
                new Usluga { Naziv = "Doručak", CijenaPoJedinici = 15 },
                new Usluga { Naziv = "Čišćenje", CijenaPoJedinici = 30 }
            );
            await ctx.SaveChangesAsync();
            var controller = new UslugeController(ctx);

            var rezultat = await controller.Index(null!);

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Usluga>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_FiltriraPoNazivu_KadJeZadanSearchString()
        {
            var ctx = KreirajKontekst(nameof(Index_FiltriraPoNazivu_KadJeZadanSearchString));
            ctx.Usluge.AddRange(
                new Usluga { Naziv = "Doručak", CijenaPoJedinici = 15 },
                new Usluga { Naziv = "Čišćenje", CijenaPoJedinici = 30 }
            );
            await ctx.SaveChangesAsync();
            var controller = new UslugeController(ctx);

            var rezultat = await controller.Index("Doručak");

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Usluga>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_GET_VracaView()
        {
            var ctx = KreirajKontekst(nameof(Create_GET_VracaView));
            var controller = new UslugeController(ctx);

            var rezultat = controller.Create();

            Assert.IsType<ViewResult>(rezultat);
        }

        [Fact]
        public async Task Create_POST_SpremaSeBazu_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_SpremaSeBazu_IRedirectaNaIndex));
            var controller = new UslugeController(ctx);
            var usluga = new Usluga { Naziv = "Transfer", Opis = "Prijevoz", CijenaPoJedinici = 50 };

            var rezultat = await controller.Create(usluga);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, await ctx.Usluge.CountAsync());
        }

        [Fact]
        public async Task Edit_GET_VracaNotFound_ZaNepostojeciId()
        {
            var ctx = KreirajKontekst(nameof(Edit_GET_VracaNotFound_ZaNepostojeciId));
            var controller = new UslugeController(ctx);

            var rezultat = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(rezultat);
        }

        [Fact]
        public async Task Edit_POST_AzuriraCijenu_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Edit_POST_AzuriraCijenu_IRedirectaNaIndex));
            var usluga = new Usluga { Naziv = "Doručak", CijenaPoJedinici = 10 };
            ctx.Usluge.Add(usluga);
            await ctx.SaveChangesAsync();
            var controller = new UslugeController(ctx);

            usluga.CijenaPoJedinici = 20;
            var rezultat = await controller.Edit(usluga.Id, usluga);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            var azurirana = await ctx.Usluge.FindAsync(usluga.Id);
            Assert.Equal(20, azurirana!.CijenaPoJedinici);
        }

        [Fact]
        public async Task Delete_POST_UklonjaUslugu_IzBaze()
        {
            var ctx = KreirajKontekst(nameof(Delete_POST_UklonjaUslugu_IzBaze));
            var usluga = new Usluga { Naziv = "Za brisanje", CijenaPoJedinici = 5 };
            ctx.Usluge.Add(usluga);
            await ctx.SaveChangesAsync();
            var controller = new UslugeController(ctx);

            await controller.DeleteConfirmed(usluga.Id);

            Assert.Equal(0, await ctx.Usluge.CountAsync());
        }
    }
}
