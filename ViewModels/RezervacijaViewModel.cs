using Microsoft.AspNetCore.Mvc.Rendering;
using RentIO.Models;
using System.ComponentModel.DataAnnotations;

namespace RentIO.ViewModels
{
    public class RezervacijaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Odaberite gosta.")]
        [Display(Name = "Gost")]
        public int GostId { get; set; }

        [Required(ErrorMessage = "Odaberite apartman.")]
        [Display(Name = "Apartman")]
        public int ApartmanId { get; set; }

        [Required(ErrorMessage = "Datum dolaska je obavezan.")]
        [Display(Name = "Datum dolaska")]
        [DataType(DataType.Date)]
        public DateTime DatumDolaska { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Datum odlaska je obavezan.")]
        [Display(Name = "Datum odlaska")]
        [DataType(DataType.Date)]
        public DateTime DatumOdlaska { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Status")]
        public StatusRezervacije Status { get; set; } = StatusRezervacije.NaCekanju;

        [Display(Name = "Ukupna cijena (€)")]
        public decimal UkupnaCijena { get; set; }

        public List<RezervacijaUslugaViewModel> Usluge { get; set; } = new();

        public IEnumerable<SelectListItem> GostiDropdown { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ApartmaniDropdown { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> UslugeDropdown { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> StatusDropdown { get; set; } = new List<SelectListItem>();
    }

    public class RezervacijaUslugaViewModel
    {
        public int Id { get; set; }
        public int UslugaId { get; set; }

        [Display(Name = "Usluga")]
        public string UslugaNaziv { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Količina mora biti između 1 i 100.")]
        [Display(Name = "Količina")]
        public int Kolicina { get; set; } = 1;

        [Display(Name = "Cijena/jed. (€)")]
        public decimal CijenaPoJedinici { get; set; }
    }
}
