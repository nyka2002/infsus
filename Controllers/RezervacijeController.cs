using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;
using RentIO.Services;
using RentIO.ViewModels;

namespace RentIO.Controllers
{
    public class RezervacijeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRezervacijaService _rezervacijaService;

        public RezervacijeController(ApplicationDbContext context, IRezervacijaService rezervacijaService)
        {
            _context = context;
            _rezervacijaService = rezervacijaService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var rezervacije = _context.Rezervacije
                .Include(r => r.Gost)
                .Include(r => r.Apartman)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                rezervacije = rezervacije.Where(r =>
                    r.Gost!.Ime.ToLower().Contains(searchString.ToLower()) ||
                    r.Gost!.Prezime.ToLower().Contains(searchString.ToLower()) ||
                    r.Apartman!.Naziv.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await rezervacije.OrderByDescending(r => r.DatumDolaska).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rezervacija = await _context.Rezervacije
                .Include(r => r.Gost)
                .Include(r => r.Apartman)
                .Include(r => r.RezervacijaUsluge)
                    .ThenInclude(ru => ru.Usluga)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null) return NotFound();
            return View(rezervacija);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new RezervacijaViewModel();
            await PopuniDropdownove(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RezervacijaViewModel vm)
        {
            if (vm.DatumOdlaska <= vm.DatumDolaska)
                ModelState.AddModelError("DatumOdlaska", "Datum odlaska mora biti nakon datuma dolaska.");

            if (ModelState.IsValid)
            {
                bool preklapanje = await _rezervacijaService.PostojiPreklapanjeTermina(
                    vm.ApartmanId, vm.DatumDolaska, vm.DatumOdlaska);

                if (preklapanje)
                {
                    ModelState.AddModelError(string.Empty,
                        "Apartman je već rezerviran u odabranom terminu. Molimo odaberite drugi termin ili apartman.");
                    await PopuniDropdownove(vm);
                    return View(vm);
                }

                var usluge = vm.Usluge.Select(u => new RezervacijaUsluga
                {
                    UslugaId = u.UslugaId,
                    Kolicina = u.Kolicina
                }).ToList();

                var rezervacija = new Rezervacija
                {
                    GostId = vm.GostId,
                    ApartmanId = vm.ApartmanId,
                    DatumDolaska = vm.DatumDolaska,
                    DatumOdlaska = vm.DatumOdlaska,
                    Status = vm.Status,
                    UkupnaCijena = await _rezervacijaService.IzracunajUkupnuCijenu(
                        vm.ApartmanId, vm.DatumDolaska, vm.DatumOdlaska, usluge),
                    RezervacijaUsluge = usluge
                };

                _context.Rezervacije.Add(rezervacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopuniDropdownove(vm);
            return View(vm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var rezervacija = await _context.Rezervacije
                .Include(r => r.RezervacijaUsluge)
                    .ThenInclude(ru => ru.Usluga)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null) return NotFound();

            var vm = new RezervacijaViewModel
            {
                Id = rezervacija.Id,
                GostId = rezervacija.GostId,
                ApartmanId = rezervacija.ApartmanId,
                DatumDolaska = rezervacija.DatumDolaska,
                DatumOdlaska = rezervacija.DatumOdlaska,
                Status = rezervacija.Status,
                UkupnaCijena = rezervacija.UkupnaCijena,
                Usluge = rezervacija.RezervacijaUsluge.Select(ru => new RezervacijaUslugaViewModel
                {
                    Id = ru.Id,
                    UslugaId = ru.UslugaId,
                    UslugaNaziv = ru.Usluga?.Naziv ?? string.Empty,
                    Kolicina = ru.Kolicina,
                    CijenaPoJedinici = ru.Usluga?.CijenaPoJedinici ?? 0
                }).ToList()
            };

            await PopuniDropdownove(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RezervacijaViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (vm.DatumOdlaska <= vm.DatumDolaska)
                ModelState.AddModelError("DatumOdlaska", "Datum odlaska mora biti nakon datuma dolaska.");

            if (ModelState.IsValid)
            {
                bool preklapanje = await _rezervacijaService.PostojiPreklapanjeTermina(
                    vm.ApartmanId, vm.DatumDolaska, vm.DatumOdlaska, vm.Id);

                if (preklapanje)
                {
                    ModelState.AddModelError(string.Empty,
                        "Apartman je već rezerviran u odabranom terminu. Molimo odaberite drugi termin ili apartman.");
                    await PopuniDropdownove(vm);
                    return View(vm);
                }

                var rezervacija = await _context.Rezervacije
                    .Include(r => r.RezervacijaUsluge)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rezervacija == null) return NotFound();

                rezervacija.GostId = vm.GostId;
                rezervacija.ApartmanId = vm.ApartmanId;
                rezervacija.DatumDolaska = vm.DatumDolaska;
                rezervacija.DatumOdlaska = vm.DatumOdlaska;
                rezervacija.Status = vm.Status;

                _context.RezervacijaUsluge.RemoveRange(rezervacija.RezervacijaUsluge);
                var noveUsluge = vm.Usluge.Select(u => new RezervacijaUsluga
                {
                    RezervacijaId = rezervacija.Id,
                    UslugaId = u.UslugaId,
                    Kolicina = u.Kolicina
                }).ToList();
                rezervacija.RezervacijaUsluge = noveUsluge;
                rezervacija.UkupnaCijena = await _rezervacijaService.IzracunajUkupnuCijenu(
                    vm.ApartmanId, vm.DatumDolaska, vm.DatumOdlaska, noveUsluge);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = id });
            }

            await PopuniDropdownove(vm);
            return View(vm);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var rezervacija = await _context.Rezervacije
                .Include(r => r.Gost)
                .Include(r => r.Apartman)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null) return NotFound();
            return View(rezervacija);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.Rezervacije
                .Include(r => r.RezervacijaUsluge)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija != null)
                _context.Rezervacije.Remove(rezervacija);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopuniDropdownove(RezervacijaViewModel vm)
        {
            vm.GostiDropdown = (await _context.Gosti.ToListAsync())
                .Select(g => new SelectListItem($"{g.Ime} {g.Prezime} ({g.Email})", g.Id.ToString()));

            vm.ApartmaniDropdown = (await _context.Apartmani.ToListAsync())
                .Select(a => new SelectListItem($"{a.Naziv} – {a.Lokacija} ({a.Cijena}€/noć)", a.Id.ToString()));

            vm.UslugeDropdown = (await _context.Usluge.ToListAsync())
                .Select(u => new SelectListItem($"{u.Naziv} ({u.CijenaPoJedinici}€)", u.Id.ToString()));

            vm.StatusDropdown = Enum.GetValues<StatusRezervacije>()
                .Select(s => new SelectListItem(s.ToString(), s.ToString()));
        }
    }
}
