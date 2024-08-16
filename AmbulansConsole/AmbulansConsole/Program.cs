using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new ClientWebSocket();
        string serverUri = "wss://localhost:7080/ws";
        await client.ConnectAsync(new Uri(serverUri), CancellationToken.None);

        Console.WriteLine("Sunucuya bağlandı.");

        while (true)
        {
            Console.Write("Ambulans Plakasını Giriniz : ");
            string plate = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(plate)) break;

            Console.Write("Koordinatları Giriniz (X,Y): ");
            string coordinates = Console.ReadLine();

            string message = $"{plate}:{coordinates}";
            var buffer = Encoding.UTF8.GetBytes(message);

            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Gönderildi: {message}");

            if (Console.ReadKey().KeyChar == 'q') break;
        }

        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        Console.WriteLine("Bağlantı Kapatıldı.");
    }
}
