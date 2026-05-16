namespace RentIO.Models
{
    public class Apartman
    {
        public int Id { get; set; }

        public string Naziv { get; set; } = string.Empty;

        public string Lokacija { get; set; } = string.Empty;

        public decimal Cijena { get; set; }

        public string Opis { get; set; } = string.Empty;
    }
}