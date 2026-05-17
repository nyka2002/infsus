using System.ComponentModel.DataAnnotations;

namespace RentIO.Models
{
    public class Gost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imati između 2 i 50 znakova.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora imati između 2 i 50 znakova.")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite valjanu email adresu.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon je obavezan.")]
        [StringLength(20, ErrorMessage = "Telefon može imati maksimalno 20 znakova.")]
        public string Telefon { get; set; } = string.Empty;

        public string PunoIme => $"{Ime} {Prezime}";

        public ICollection<Rezervacija> Rezervacije { get; set; } = new List<Rezervacija>();
    }
}
