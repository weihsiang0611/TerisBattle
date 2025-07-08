using System.Net.Sockets;
using System.Text;

namespace TetrisClient { 

public class ServerConnection
{
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly string _serverIp;
    private readonly int _serverPort;

    public event Action<string> MessageReceived;

    public ServerConnection(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
    }

    public async Task ConnectAsync()
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_serverIp, _serverPort);
        _stream = _client.GetStream();
        _ = ReceiveMessagesAsync(); // Start listening for messages
    }

    public async Task SendMessageAsync(string message)
    {
        if (_stream == null)
        {
            return;
        }
        byte[] data = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(data, 0, data.Length);
    }

    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[4096];
        while (true)
        {
            try
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Server disconnected
                    break;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                // Handle disconnection or error
                Console.WriteLine($"Error receiving message: {ex.Message}");
                break;
            }
        }
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
    }
}

    }