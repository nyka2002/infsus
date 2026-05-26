using System.ComponentModel.DataAnnotations;

namespace RentIO.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [Display(Name = "Korisničko ime")]
        public string KorisnickoIme { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Lozinka { get; set; } = string.Empty;
    }
}
