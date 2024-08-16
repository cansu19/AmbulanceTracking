using System.Net.WebSockets;
using System.Text;

var ws = new ClientWebSocket();
Console.WriteLine("Sunucuya ba�lan�l�yor");
await ws.ConnectAsync(new Uri("wss://localhost:7079/ws"), CancellationToken.None);

Console.WriteLine("Ba�land�!");

var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024];
    while (true)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            Console.WriteLine("Ba�lant� kapat�ld�.");
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine("G�nderdi: " + message);
    }
});

await receiveTask;
