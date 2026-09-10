using System.Net.Http.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var client = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7163/")
};

while (true)
{
    Console.Clear();

    Console.WriteLine("╔══════════════════════════════╗");
    Console.WriteLine("║      HAVA DURUMU SİSTEMİ     ║");
    Console.WriteLine("╚══════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("Bir şehir yaz veya çıkmak için 0 yaz.");
    Console.Write("Şehir: ");

    var sehir = Console.ReadLine()?.Trim();

    if (sehir == "0")
    {
        Console.WriteLine("Program kapatıldı.");
        break;
    }

    if (string.IsNullOrWhiteSpace(sehir))
    {
        Console.WriteLine("\nŞehir adı boş olamaz.");
        Console.WriteLine("Devam etmek için bir tuşa bas...");
        Console.ReadKey(true);
        continue;
    }

    try
    {
        var url = $"api/HavaDurumu/{Uri.EscapeDataString(sehir)}";

        using var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"\nAPI hata verdi: {(int)response.StatusCode}");
            Console.WriteLine("Şehir bulunamadı veya API çalışmıyor.");
        }
        else
        {
            var havaDurumu =
                await response.Content.ReadFromJsonAsync<HavaDurumu>();

            if (havaDurumu is null)
            {
                Console.WriteLine("\nHava durumu bilgisi alınamadı.");
            }
            else
            {
                Console.WriteLine("\n--- GÜNCEL HAVA DURUMU ---");
                Console.WriteLine($"Şehir     : {havaDurumu.Sehir}");
                Console.WriteLine($"Sıcaklık : {havaDurumu.Sicaklik} °C");
                Console.WriteLine($"Nem      : %{havaDurumu.Nem}");
                Console.WriteLine($"Durum    : {havaDurumu.Durum}");
            }
        }
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("\nAPI'ye bağlanılamadı.");
        Console.WriteLine("HavaDurumuAPI projesinin çalıştığından emin ol.");
    }
    catch (Exception hata)
    {
        Console.WriteLine($"\nBeklenmeyen hata: {hata.Message}");
    }

    Console.WriteLine("\nYeni bir şehir sorgulamak için bir tuşa bas...");
    Console.ReadKey(true);
}

public class HavaDurumu
{
    public int Id { get; set; }
    public string Sehir { get; set; } = "";
    public int Sicaklik { get; set; }
    public int Nem { get; set; }
    public string Durum { get; set; } = "";
}