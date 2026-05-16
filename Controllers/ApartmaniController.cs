using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Controllers
{
    public class ApartmaniController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartmaniController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Apartmani
        public async Task<IActionResult> Index(string searchString)
        {
            var apartmani = from a in _context.Apartmani
                            select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                apartmani = apartmani.Where(a =>
                    a.Naziv.ToLower().Contains(searchString.ToLower()) ||
                    a.Lokacija.ToLower().Contains(searchString.ToLower()));
            }

            return View(await apartmani.ToListAsync());
        }

        // GET: Apartmani/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartman = await _context.Apartmani
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apartman == null)
            {
                return NotFound();
            }

            return View(apartman);
        }

        // GET: Apartmani/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Apartmani/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Lokacija,Cijena,Opis")] Apartman apartman)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apartman);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apartman);
        }

        // GET: Apartmani/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartman = await _context.Apartmani.FindAsync(id);
            if (apartman == null)
            {
                return NotFound();
            }
            return View(apartman);
        }

        // POST: Apartmani/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Lokacija,Cijena,Opis")] Apartman apartman)
        {
            if (id != apartman.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apartman);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApartmanExists(apartman.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(apartman);
        }

        // GET: Apartmani/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartman = await _context.Apartmani
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apartman == null)
            {
                return NotFound();
            }

            return View(apartman);
        }

        // POST: Apartmani/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartman = await _context.Apartmani.FindAsync(id);
            if (apartman != null)
            {
                _context.Apartmani.Remove(apartman);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApartmanExists(int id)
        {
            return _context.Apartmani.Any(e => e.Id == id);
        }
    }
}
