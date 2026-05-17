using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Controllers;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Tests.PresentationLayer
{
    public class GostiControllerTests
    {
        private ApplicationDbContext KreirajKontekst(string naziv) =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(naziv)
                .Options);

        [Fact]
        public async Task Index_VracaViewSListomGostiju()
        {
            var ctx = KreirajKontekst(nameof(Index_VracaViewSListomGostiju));
            ctx.Gosti.AddRange(
                new Gost { Ime = "Ana", Prezime = "Anić", Email = "ana@test.com", Telefon = "091" },
                new Gost { Ime = "Pero", Prezime = "Perić", Email = "pero@test.com", Telefon = "092" }
            );
            await ctx.SaveChangesAsync();
            var controller = new GostiController(ctx);

            var rezultat = await controller.Index(null!);

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Gost>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_FiltriraPoPrezimenu_KadJeZadanSearchString()
        {
            var ctx = KreirajKontekst(nameof(Index_FiltriraPoPrezimenu_KadJeZadanSearchString));
            ctx.Gosti.AddRange(
                new Gost { Ime = "Ana", Prezime = "Anić", Email = "ana@test.com", Telefon = "091" },
                new Gost { Ime = "Pero", Prezime = "Perić", Email = "pero@test.com", Telefon = "092" }
            );
            await ctx.SaveChangesAsync();
            var controller = new GostiController(ctx);

            var rezultat = await controller.Index("Anić");

            var view = Assert.IsType<ViewResult>(rezultat);
            var model = Assert.IsAssignableFrom<IEnumerable<Gost>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_GET_VracaView()
        {
            var ctx = KreirajKontekst(nameof(Create_GET_VracaView));
            var controller = new GostiController(ctx);

            var rezultat = controller.Create();

            Assert.IsType<ViewResult>(rezultat);
        }

        [Fact]
        public async Task Create_POST_SpremaSeBazu_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Create_POST_SpremaSeBazu_IRedirectaNaIndex));
            var controller = new GostiController(ctx);
            var gost = new Gost { Ime = "Mia", Prezime = "Mić", Email = "mia@test.com", Telefon = "093" };

            var rezultat = await controller.Create(gost);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, await ctx.Gosti.CountAsync());
        }

        [Fact]
        public async Task Edit_GET_VracaNotFound_ZaNepostojeciId()
        {
            var ctx = KreirajKontekst(nameof(Edit_GET_VracaNotFound_ZaNepostojeciId));
            var controller = new GostiController(ctx);

            var rezultat = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(rezultat);
        }

        [Fact]
        public async Task Edit_POST_AzuriraPodatke_IRedirectaNaIndex()
        {
            var ctx = KreirajKontekst(nameof(Edit_POST_AzuriraPodatke_IRedirectaNaIndex));
            var gost = new Gost { Ime = "Staro", Prezime = "Ime", Email = "staro@test.com", Telefon = "091" };
            ctx.Gosti.Add(gost);
            await ctx.SaveChangesAsync();
            var controller = new GostiController(ctx);

            gost.Ime = "Novo";
            var rezultat = await controller.Edit(gost.Id, gost);

            var redirect = Assert.IsType<RedirectToActionResult>(rezultat);
            Assert.Equal("Index", redirect.ActionName);
            var azurirani = await ctx.Gosti.FindAsync(gost.Id);
            Assert.Equal("Novo", azurirani!.Ime);
        }

        [Fact]
        public async Task Delete_POST_UklonjaGosta_IzBaze()
        {
            var ctx = KreirajKontekst(nameof(Delete_POST_UklonjaGosta_IzBaze));
            var gost = new Gost { Ime = "Za", Prezime = "Brisanje", Email = "brisanje@test.com", Telefon = "099" };
            ctx.Gosti.Add(gost);
            await ctx.SaveChangesAsync();
            var controller = new GostiController(ctx);

            await controller.DeleteConfirmed(gost.Id);

            Assert.Equal(0, await ctx.Gosti.CountAsync());
        }
    }
}
