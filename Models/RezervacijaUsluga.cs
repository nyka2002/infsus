using System.ComponentModel.DataAnnotations;

namespace RentIO.Models
{
    public class RezervacijaUsluga
    {
        public int Id { get; set; }

        [Required]
        public int RezervacijaId { get; set; }

        [Required]
        public int UslugaId { get; set; }

        [Required(ErrorMessage = "Količina je obavezna.")]
        [Range(1, 100, ErrorMessage = "Količina mora biti između 1 i 100.")]
        [Display(Name = "Količina")]
        public int Kolicina { get; set; } = 1;

        // Navigacijska svojstva
        public Rezervacija? Rezervacija { get; set; }
        public Usluga? Usluga { get; set; }
    }
}
