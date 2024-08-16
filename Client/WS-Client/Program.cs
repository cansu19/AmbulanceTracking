using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connections = new List<(WebSocket Socket, string Name, string Coordinates)>();

app.UseWebSockets();
app.UseStaticFiles();

app.MapGet("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var curName = context.Request.Query["name"];
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        connections.Add((ws, curName, string.Empty));

        // Send current state of all markers to the new client
        foreach (var (socket, name, coordinates) in connections)
        {
            if (socket != ws)
            {
                var message = $"{name}:{coordinates}";
                await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        await BroadCast($"{curName} odaya katýldý");
        await BroadCast($"{connections.Count} kullanýcý baðlandý");

        await ReceiveMessage(ws, async (result, buffer) =>
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Koordinat mesajý formatýný kontrol et
                if (message.Contains(":") && message.Split(':').Length == 2)
                {
                    var parts = message.Split(':');
                    var name = parts[0].Trim();
                    var coordinates = parts[1].Trim();

                    // Koordinatlarý kontrol et
                    var coords = coordinates.Split(',');
                    if (coords.Length == 2)
                    {
                        var x = coords[0].Trim();
                        var y = coords[1].Trim();

                        if (double.TryParse(x, out _) && double.TryParse(y, out _))
                        {
                            // Koordinatlarý güncelle
                            connections.RemoveAll(c => c.Socket == ws);
                            connections.Add((ws, name, coordinates));

                            await BroadCast(message); // Mesajý diðerlerine gönder
                        }
                        else
                        {
                            Console.WriteLine($"Invalid coordinates: {x}, {y}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid coordinates format: {coordinates}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid message format: {message}");
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
            {
                connections.RemoveAll(c => c.Socket == ws);
                await BroadCast($"{curName} odadan ayrýldý");
                await BroadCast($"{connections.Count} kullanýcý baðlandý");
                await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

async Task ReceiveMessage(WebSocket socket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
{
    var buffer = new byte[1024 * 4];
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        await handleMessage(result, buffer);
    }
}

async Task BroadCast(string message)
{
    var bytes = Encoding.UTF8.GetBytes(message);
    foreach (var (socket, _, _) in connections)
    {
        if (socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

app.Run();
