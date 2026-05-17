using System.ComponentModel.DataAnnotations;

namespace RentIO.Models
{
    public class Usluga
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv mora imati između 2 i 100 znakova.")]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Opis može imati maksimalno 300 znakova.")]
        public string Opis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cijena je obavezna.")]
        [Range(0.01, 10000, ErrorMessage = "Cijena mora biti između 0.01 i 10000.")]
        [Display(Name = "Cijena po jedinici (€)")]
        public decimal CijenaPoJedinici { get; set; }

        public ICollection<RezervacijaUsluga> RezervacijaUsluge { get; set; } = new List<RezervacijaUsluga>();
    }
}
