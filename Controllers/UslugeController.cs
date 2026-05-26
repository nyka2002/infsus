using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Controllers
{
    [Authorize]
    public class UslugeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UslugeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var usluge = _context.Usluge.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usluge = usluge.Where(u =>
                    u.Naziv.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await usluge.OrderBy(u => u.Naziv).ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,CijenaPoJedinici")] Usluga usluga)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usluga);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usluga);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var usluga = await _context.Usluge.FindAsync(id);
            if (usluga == null) return NotFound();
            return View(usluga);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,CijenaPoJedinici")] Usluga usluga)
        {
            if (id != usluga.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(usluga);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usluga);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var usluga = await _context.Usluge.FirstOrDefaultAsync(u => u.Id == id);
            if (usluga == null) return NotFound();
            return View(usluga);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usluga = await _context.Usluge.FindAsync(id);
            if (usluga != null) _context.Usluge.Remove(usluga);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
