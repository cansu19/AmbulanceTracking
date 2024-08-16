using System.Net.WebSockets;
using System.Text;

var ws = new ClientWebSocket();
Console.WriteLine("Sunucuya bağlanılıyor");
await ws.ConnectAsync(new Uri("wss://localhost:7079/ws"), CancellationToken.None);

Console.WriteLine("Bağlandı!");

var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024];
    while (true)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            Console.WriteLine("Bağlantı kapatıldı.");
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine("Gönderdi: " + message);
    }
});

await receiveTask;
