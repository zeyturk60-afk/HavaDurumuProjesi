namespace HavaDurumuAPI.Models
{
    public class HavaDurumu
    {
        public int Id { get; set; }
        public string Sehir { get; set; } = string.Empty;
        public double Sicaklik { get; set; }
        public int Nem { get; set; }
        public string Durum { get; set; } = string.Empty;
    }
}