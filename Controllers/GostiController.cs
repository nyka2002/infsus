using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Controllers
{
    public class GostiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GostiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var gosti = _context.Gosti.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                gosti = gosti.Where(g =>
                    g.Ime.ToLower().Contains(searchString.ToLower()) ||
                    g.Prezime.ToLower().Contains(searchString.ToLower()) ||
                    g.Email.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await gosti.OrderBy(g => g.Prezime).ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,Email,Telefon")] Gost gost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gost);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var gost = await _context.Gosti.FindAsync(id);
            if (gost == null) return NotFound();
            return View(gost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,Email,Telefon")] Gost gost)
        {
            if (id != gost.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(gost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = gost.Id });
            }
            return View(gost);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var gost = await _context.Gosti.FirstOrDefaultAsync(g => g.Id == id);
            if (gost == null) return NotFound();
            return View(gost);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gost = await _context.Gosti.FindAsync(id);
            if (gost != null) _context.Gosti.Remove(gost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
