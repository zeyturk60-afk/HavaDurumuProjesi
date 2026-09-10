using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using HavaDurumuAPI.Models;

namespace HavaDurumuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HavaDurumuController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public HavaDurumuController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Örnek: GET /api/HavaDurumu/İstanbul
        [HttpGet("{sehir}")]
        public async Task<IActionResult> Get(string sehir)
        {
            var konum = await KonumGetir(sehir);

            if (konum == null)
                return NotFound($"{sehir} şehri bulunamadı.");


            var latitude = konum.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = konum.Longitude.ToString(CultureInfo.InvariantCulture);

            var havaUrl =
                $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,weather_code&timezone=Europe%2FIstanbul";
            var havaCevabi =
                await _httpClient.GetFromJsonAsync<HavaCevabi>(havaUrl);

            if (havaCevabi?.Current == null)
                return StatusCode(502, "Hava durumu servisine ulaşılamadı.");

            var havaDurumu = new HavaDurumu
            {
                Id = 1,
                Sehir = konum.Name,
                Sicaklik = (int)Math.Round(havaCevabi.Current.Sicaklik),
                Nem = havaCevabi.Current.Nem,
                Durum = DurumGetir(havaCevabi.Current.WeatherCode)
            };

            return Ok(havaDurumu);
        }

        // Örnek: GET /api/HavaDurumu/İstanbul/7gunluk
        [HttpGet("{sehir}/7gunluk")]
        public async Task<IActionResult> YediGunlukTahmin(string sehir)
        {
            var konum = await KonumGetir(sehir);

            if (konum == null)
                return NotFound($"{sehir} şehri bulunamadı.");

            var latitude = konum.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = konum.Longitude.ToString(CultureInfo.InvariantCulture);

            var havaUrl =
                $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min&timezone=Europe%2FIstanbul&forecast_days=7";

            var havaCevabi =
                await _httpClient.GetFromJsonAsync<YediGunlukHavaCevabi>(havaUrl);

            if (havaCevabi?.Current == null || havaCevabi.Daily == null)
                return StatusCode(502, "Hava durumu servisine ulaşılamadı.");

            var gunler = havaCevabi.Daily.Time
                .Select((tarih, index) => new GunlukTahmin
                {
                    Tarih = tarih,
                    EnDusukSicaklik = (int)Math.Round(havaCevabi.Daily.EnDusukSicaklik[index]),
                    EnYuksekSicaklik = (int)Math.Round(havaCevabi.Daily.EnYuksekSicaklik[index]),
                    Durum = DurumGetir(havaCevabi.Daily.WeatherCode[index]),
                    HavaKodu = havaCevabi.Daily.WeatherCode[index]
                })
                .ToList();

            var sonuc = new YediGunlukTahminSonucu
            {
                AnlikHavaDurumu = new HavaDurumu
                {
                    Id = 1,
                    Sehir = konum.Name,
                    Sicaklik = Math.Round((double)havaCevabi.Current.Sicaklik, 1),
                    Nem = havaCevabi.Current.Nem,
                    Durum = DurumGetir(havaCevabi.Current.WeatherCode)
                },
                Gunler = gunler
            };

            return Ok(sonuc);
        }

        [HttpPost]
        public IActionResult Post(HavaDurumu havaDurumu)
        {
            return Ok(havaDurumu);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, HavaDurumu havaDurumu)
        {
            havaDurumu.Id = id;
            return Ok(havaDurumu);
        }

        private async Task<Konum?> KonumGetir(string sehir)
        {
            var sehirAdi = Uri.EscapeDataString(sehir);
            var konumUrl =
                $"https://geocoding-api.open-meteo.com/v1/search?name={sehirAdi}&count=1&language=tr&format=json";

            var konumCevabi =
                await _httpClient.GetFromJsonAsync<KonumCevabi>(konumUrl);

            return konumCevabi?.Results?.FirstOrDefault();
        }

        private static string DurumGetir(int kod) => kod switch
        {
            0 => "Açık",
            1 or 2 => "Parçalı bulutlu",
            3 => "Bulutlu",
            45 or 48 => "Sisli",
            51 or 53 or 55 => "Çisenti",
            61 or 63 or 65 => "Yağmurlu",
            71 or 73 or 75 => "Karlı",
            80 or 81 or 82 => "Sağanak yağışlı",
            95 or 96 or 99 => "Gök gürültülü",
            _ => "Bilinmiyor"
        };
    }

    public class KonumCevabi
    {
        [JsonPropertyName("results")]
        public List<Konum>? Results { get; set; }
    }

    public class Konum
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class HavaCevabi
    {
        [JsonPropertyName("current")]
        public AnlikHava? Current { get; set; }
    }

    public class AnlikHava
    {
        [JsonPropertyName("temperature_2m")]
        public decimal Sicaklik { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int Nem { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }

    public class YediGunlukHavaCevabi
    {
        [JsonPropertyName("current")]
        public AnlikHava? Current { get; set; }

        [JsonPropertyName("daily")]
        public GunlukHava? Daily { get; set; }
    }

    public class GunlukHava
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = [];

        [JsonPropertyName("temperature_2m_max")]
        public List<decimal> EnYuksekSicaklik { get; set; } = [];

        [JsonPropertyName("temperature_2m_min")]
        public List<decimal> EnDusukSicaklik { get; set; } = [];

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; } = [];
    }

    public class YediGunlukTahminSonucu
    {
        public HavaDurumu AnlikHavaDurumu { get; set; } = new();
        public List<GunlukTahmin> Gunler { get; set; } = [];
    }

    public class GunlukTahmin
    {
        public string Tarih { get; set; } = string.Empty;
        public int EnDusukSicaklik { get; set; }
        public int EnYuksekSicaklik { get; set; }
        public string Durum { get; set; } = string.Empty;
        public int HavaKodu { get; set; }
    }
}
