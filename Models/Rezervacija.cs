using System.ComponentModel.DataAnnotations;

namespace RentIO.Models
{
    public enum StatusRezervacije
    {
        NaCekanju,
        Potvrđena,
        Otkazana
    }

    public class Rezervacija
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Gost je obavezan.")]
        [Display(Name = "Gost")]
        public int GostId { get; set; }

        [Required(ErrorMessage = "Apartman je obavezan.")]
        [Display(Name = "Apartman")]
        public int ApartmanId { get; set; }

        [Required(ErrorMessage = "Datum dolaska je obavezan.")]
        [Display(Name = "Datum dolaska")]
        [DataType(DataType.Date)]
        public DateTime DatumDolaska { get; set; }

        [Required(ErrorMessage = "Datum odlaska je obavezan.")]
        [Display(Name = "Datum odlaska")]
        [DataType(DataType.Date)]
        public DateTime DatumOdlaska { get; set; }

        [Display(Name = "Status")]
        public StatusRezervacije Status { get; set; } = StatusRezervacije.NaCekanju;

        [Display(Name = "Ukupna cijena (€)")]
        [Range(0, double.MaxValue)]
        public decimal UkupnaCijena { get; set; }

        public Gost? Gost { get; set; }
        public Apartman? Apartman { get; set; }
        public ICollection<RezervacijaUsluga> RezervacijaUsluge { get; set; } = new List<RezervacijaUsluga>();
    }
}
