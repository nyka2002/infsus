using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Models;

namespace RentIO.Services
{
    public interface IRezervacijaService
    {
        Task<bool> PostojiPreklapanjeTermina(int apartmanId, DateTime dolazak, DateTime odlazak, int? iskljuciRezervacijuId = null);
        Task<decimal> IzracunajUkupnuCijenu(int apartmanId, DateTime dolazak, DateTime odlazak, IEnumerable<RezervacijaUsluga> usluge);
    }

    public class RezervacijaService : IRezervacijaService
    {
        private readonly ApplicationDbContext _context;

        public RezervacijaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PostojiPreklapanjeTermina(int apartmanId, DateTime dolazak, DateTime odlazak, int? iskljuciRezervacijuId = null)
        {
            var query = _context.Rezervacije
                .Where(r => r.ApartmanId == apartmanId
                         && r.Status != StatusRezervacije.Otkazana
                         && r.DatumDolaska < odlazak
                         && r.DatumOdlaska > dolazak);

            if (iskljuciRezervacijuId.HasValue)
                query = query.Where(r => r.Id != iskljuciRezervacijuId.Value);

            return await query.AnyAsync();
        }

        public async Task<decimal> IzracunajUkupnuCijenu(int apartmanId, DateTime dolazak, DateTime odlazak, IEnumerable<RezervacijaUsluga> usluge)
        {
            var apartman = await _context.Apartmani.FindAsync(apartmanId);
            if (apartman == null) return 0;

            int brojNoci = (odlazak.Date - dolazak.Date).Days;
            decimal cijenaSmjestaja = apartman.Cijena * brojNoci;

            decimal cijenaUsluga = 0;
            foreach (var ru in usluge)
            {
                var usluga = await _context.Usluge.FindAsync(ru.UslugaId);
                if (usluga != null)
                    cijenaUsluga += usluga.CijenaPoJedinici * ru.Kolicina;
            }

            return cijenaSmjestaja + cijenaUsluga;
        }
    }
}
