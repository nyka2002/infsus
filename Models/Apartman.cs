using System.ComponentModel.DataAnnotations;

namespace RentIO.Models
{
    public class Apartman
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Naziv mora imati između 3 i 100 znakova.")]
        public string Naziv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lokacija je obavezna.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Lokacija mora imati između 2 i 100 znakova.")]
        public string Lokacija { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cijena je obavezna.")]
        [Display(Name = "Cijena")]
        [Range(1, 10000,
            ErrorMessage = "Cijena mora biti broj između 1 i 10000.")]
        public decimal Cijena { get; set; }

        [StringLength(300,
            ErrorMessage = "Opis može imati maksimalno 300 znakova.")]
        public string Opis { get; set; } = string.Empty;
    }
}