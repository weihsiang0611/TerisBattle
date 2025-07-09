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
        byte[] singleByteBuffer = new byte[1]; // Buffer for reading one byte at a time
        byte[] buffer = new byte[4096]; // Max message size

        while (true)
        {
            try
            {
                // Read until we find the ':' delimiter for the length prefix
                StringBuilder lengthBuilder = new StringBuilder();
                int bytesReadCount;
                while ((bytesReadCount = await _stream.ReadAsync(singleByteBuffer, 0, 1)) > 0)
                {
                    char c = (char)singleByteBuffer[0];
                    if (c == ':')
                    {
                        break;
                    }
                    lengthBuilder.Append(c);
                }

                if (bytesReadCount == 0) // Stream ended unexpectedly or server disconnected
                {
                    break;
                }

                int messageLength;
                if (!int.TryParse(lengthBuilder.ToString(), out messageLength))
                {
                    Console.WriteLine("Error: Could not parse message length.");
                    continue; // Skip this message
                }

                // Read the actual message content
                byte[] messageBytes = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int bytesRead = await _stream.ReadAsync(messageBytes, totalBytesRead, messageLength - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        // Server disconnected or stream ended prematurely
                        Console.WriteLine("Error: Server disconnected or stream ended prematurely while reading message content.");
                        break;
                    }
                    totalBytesRead += bytesRead;
                }

                if (totalBytesRead == messageLength)
                {
                    string message = Encoding.UTF8.GetString(messageBytes, 0, messageLength);
                    MessageReceived?.Invoke(message);
                }
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