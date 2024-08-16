using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.MapGet("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        while (true)
        {
            var message = "Þuan Ki Zaman : " + DateTime.Now.ToString("HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            if (ws.State == System.Net.WebSockets.WebSocketState.Open)
            {
                await ws.SendAsync(arraySegment,
                    System.Net.WebSockets.WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else if (ws.State == System.Net.WebSockets.WebSocketState.Closed ||
            ws.State == System.Net.WebSockets.WebSocketState.Aborted)
            {
                break;
            }
            Thread.Sleep(1000);
        }

    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Run();