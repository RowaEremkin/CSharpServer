using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public class WebSocketManager
{
    private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

    public void AddSocket(string id, WebSocket socket)
    {
        _sockets.TryAdd(id, socket);
    }

    public async Task RemoveSocket(string id)
    {
        _sockets.TryRemove(id, out _);
        await _sockets[id].CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketManager", CancellationToken.None);
    }

    public WebSocket GetSocketById(string id)
    {
        _sockets.TryGetValue(id, out var socket);
        return socket;
    }

    public IEnumerable<WebSocket> GetAllSockets()
    {
        return _sockets.Values;
    }
    public async static Task HandleWebSocket(WebSocket webSocket, string connectionId, WebSocketManager webSocketManager)
    {
        try
        { 
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message: {message}");

                    // Обработка полученного сообщения

                    // Пример отправки обратно клиенту
                    var responseMessage = $"Server received: {message}";
                    var responseBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseMessage));
                    await webSocket.SendAsync(responseBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }

            } while (!result.CloseStatus.HasValue);

            // Удаляем WebSocket из менеджера по завершении соединения
            await webSocketManager.RemoveSocket(connectionId);

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error handling WebSocket for connection {connectionId}: {ex}");
        }
        finally
        {
            // убедитесь, что WebSocket удаляется из менеджера при закрытии
            webSocketManager.RemoveSocket(connectionId);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        }
    }
}