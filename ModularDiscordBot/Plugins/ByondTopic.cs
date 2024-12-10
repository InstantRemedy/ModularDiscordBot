using System.Net.Sockets;
using System.Text;

namespace ModularDiscordBot.Plugins;

public sealed class ByondTopic
{
    private const byte PacketId = 0x83;
    private const byte ResponseString = 0x06;
    private const byte ResponseFloat = 0x2A;
    
    /// <summary>
    /// Sends a request to the specified server with the given arguments.
    /// </summary>
    /// <param name="address">The IP address or DNS of the DreamDaemon server.</param>
    /// <param name="port">The port on which the DreamDaemon server is hosting the world.</param>
    /// <param name="args">A dictionary of arguments for the request.</param>
    public void Export(string address, int port, Dictionary<string, string> args)
    {
        string query = BuildQueryString(args);
        Send(address, port, query).Wait();
    }

    /// <summary>
    /// Asynchronously sends a Topic() packet to the server and returns the response.
    /// </summary>
    /// <param name="address">The IP address or DNS of the DreamDaemon server.</param>
    /// <param name="port">The DreamDaemon server port.</param>
    /// <param name="query">The request string.</param>
    /// <returns>A tuple containing the response type and the data.</returns>
    public async Task<(byte responseType, object data)> Send(string address, int port, string query)
    {
        if (string.IsNullOrEmpty(query) || query[0] != '?')
        {
            query = "?" + query;
        }

        int packetSize = Encoding.UTF8.GetByteCount(query) + 6;
        if (packetSize >= 65535)
        {
            throw new Exception("The query string is too large, exceeding the maximum packet size.");
        }

        // Create the packet
        byte[] packet = new byte[9 + Encoding.UTF8.GetByteCount(query) + 1]; // 9 bytes for the header + query + null terminator
        int offset = 0;

        // Build the header
        packet[offset++] = 0x00; // Padding bytes
        packet[offset++] = PacketId; // Packet identifier
        packet[offset++] = (byte)(packetSize >> 8); // High byte of packet size
        packet[offset++] = (byte)(packetSize & 0xFF); // Low byte of packet size
        packet[offset++] = 0x00; // Padding bytes (5 bytes total)
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;

        // Copy the query string
        byte[] queryBytes = Encoding.UTF8.GetBytes(query);
        Array.Copy(queryBytes, 0, packet, offset, queryBytes.Length);
        offset += queryBytes.Length;

        packet[offset++] = 0x00; // Null terminator

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(address, port);
            using (var stream = client.GetStream())
            {
                await stream.WriteAsync(packet, 0, packet.Length);
                await stream.FlushAsync();

                // Read the response header
                byte[] recvHeader = new byte[5];
                int bytesRead = await ReadExactAsync(stream, recvHeader, 0, 5);
                if (bytesRead < 5)
                {
                    throw new Exception("Failed to read the response header.");
                }

                // Parse the response header
                byte recvPacketId = recvHeader[1];
                ushort contentLen = (ushort)((recvHeader[2] << 8) | recvHeader[3]);
                byte responseType = recvHeader[4];

                if (recvPacketId != PacketId)
                {
                    client.Close();
                    throw new Exception($"Invalid packet identifier in response. Expected 0x83, got {recvPacketId}");
                }
                else
                {
                    if (responseType == ResponseString)
                    {
                        contentLen -= 2;
                    }
                    else if (responseType == ResponseFloat)
                    {
                        contentLen -= 1;
                    }

                    byte[] response = new byte[contentLen];
                    bytesRead = await ReadExactAsync(stream, response, 0, contentLen);
                    if (bytesRead < contentLen)
                    {
                        throw new Exception($"Truncated response: {bytesRead} of {contentLen}");
                    }

                    object data;
                    if (responseType == ResponseString)
                    {
                        string responseString = Encoding.UTF8.GetString(response);
                        var dataDict = ParseQueryString(responseString);
                        data = dataDict;
                    }
                    else if (responseType == ResponseFloat)
                    {
                        if (response.Length < 4)
                        {
                            throw new Exception("Incorrect length of float response.");
                        }
                        float floatValue = BitConverter.ToSingle(response, 0);
                        data = floatValue;
                    }
                    else
                    {
                        // Unknown response type, return raw data
                        data = response;
                    }

                    client.Close();
                    return (responseType, data);
                }
            }
        }
    }

    /// <summary>
    /// Requests the server status.
    /// </summary>
    /// <param name="address">The IP address or DNS of the DreamDaemon server.</param>
    /// <param name="port">The DreamDaemon server port.</param>
    /// <returns>A dictionary containing the server status data.</returns>
    public async Task<Dictionary<string, string>?> QueryStatus(string address, int port)
    {
        var (responseType, data) = await Send(address, port, "?status");
        if (responseType == ResponseString && data is Dictionary<string, string> dataDict)
        {
            return dataDict;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Requests the player count on the server.
    /// </summary>
    /// <param name="address">The IP address or DNS of the DreamDaemon server.</param>
    /// <param name="port">The DreamDaemon server port.</param>
    /// <returns>The player count as a string.</returns>
    public async Task<string?> QueryPlayerCount(string address, int port)
    {
        var (responseType, data) = await Send(address, port, "?playing");
        if (responseType == ResponseFloat && data is float floatValue)
        {
            return ((int)floatValue).ToString();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Asynchronously reads the specified number of bytes from the stream.
    /// </summary>
    private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                int bytesRead = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cts.Token);
                if (bytesRead == 0)
                {
                    break; // End of stream
                }
                totalRead += bytesRead;
            }
        }
        return totalRead;
    }

    /// <summary>
    /// Creates a query string from the dictionary of arguments.
    /// </summary>
    private string BuildQueryString(Dictionary<string, string> args)
    {
        var list = new List<string>();
        foreach (var kvp in args)
        {
            string key = Uri.EscapeDataString(kvp.Key);
            string value = Uri.EscapeDataString(kvp.Value);
            list.Add($"{key}={value}");
        }
        return string.Join("&", list);
    }

    /// <summary>
    /// Parses the query string into a dictionary.
    /// </summary>
    private Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>();
        string[] pairs = query.Split('&');
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
            {
                string key = Uri.UnescapeDataString(kv[0]);
                string value = Uri.UnescapeDataString(kv[1]);
                result[key] = value;
            }
            else if (kv.Length == 1)
            {
                string key = Uri.UnescapeDataString(kv[0]);
                result[key] = "";
            }
        }
        return result;
    }
}
