using System.Net.Sockets;
using System.Text;

namespace ModularDiscordBot.Plugins;

public sealed class ByondTopic
{
    private const byte PacketId = 0x83;
    private const byte ResponseString = 0x06;
    private const byte ResponseFloat = 0x2A;
    
    /// <summary>
    /// Отправляет запрос на указанный сервер с заданными аргументами.
    /// </summary>
    /// <param name="address">IP-адрес или DNS сервера DreamDaemon.</param>
    /// <param name="port">Порт, на котором сервер DreamDaemon обслуживает мир.</param>
    /// <param name="args">Словарь аргументов для запроса.</param>
    public void Export(string address, int port, Dictionary<string, string> args)
    {
        string query = BuildQueryString(args);
        Send(address, port, query).Wait();
    }

    /// <summary>
    /// Асинхронно отправляет пакет Topic() на сервер и возвращает ответ.
    /// </summary>
    /// <param name="address">IP-адрес или DNS сервера DreamDaemon.</param>
    /// <param name="port">Порт сервера DreamDaemon.</param>
    /// <param name="query">Строка запроса.</param>
    /// <returns>Кортеж из типа ответа и данных.</returns>
    public async Task<(byte responseType, object data)> Send(string address, int port, string query)
    {
        if (string.IsNullOrEmpty(query) || query[0] != '?')
        {
            query = "?" + query;
        }

        int packetSize = Encoding.UTF8.GetByteCount(query) + 6;
        if (packetSize >= 65535)
        {
            throw new Exception("Строка запроса слишком велика, превышен максимальный размер пакета.");
        }

        // Создание пакета
        byte[] packet = new byte[9 + Encoding.UTF8.GetByteCount(query) + 1]; // 9 байт заголовка + запрос + нуль-терминатор
        int offset = 0;

        // Построение заголовка
        packet[offset++] = 0x00; // Байты заполнения
        packet[offset++] = PacketId; // Идентификатор пакета
        packet[offset++] = (byte)(packetSize >> 8); // Старший байт размера пакета
        packet[offset++] = (byte)(packetSize & 0xFF); // Младший байт размера пакета
        packet[offset++] = 0x00; // Байты заполнения (5 байт)
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;
        packet[offset++] = 0x00;

        // Копирование строки запроса
        byte[] queryBytes = Encoding.UTF8.GetBytes(query);
        Array.Copy(queryBytes, 0, packet, offset, queryBytes.Length);
        offset += queryBytes.Length;

        packet[offset++] = 0x00; // Нуль-терминатор

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(address, port);
            using (var stream = client.GetStream())
            {
                await stream.WriteAsync(packet, 0, packet.Length);
                await stream.FlushAsync();

                // Чтение заголовка ответа
                byte[] recvHeader = new byte[5];
                int bytesRead = await ReadExactAsync(stream, recvHeader, 0, 5);
                if (bytesRead < 5)
                {
                    throw new Exception("Не удалось прочитать заголовок ответа.");
                }

                // Разбор заголовка ответа
                byte recvPacketId = recvHeader[1];
                ushort contentLen = (ushort)((recvHeader[2] << 8) | recvHeader[3]);
                byte responseType = recvHeader[4];

                if (recvPacketId != PacketId)
                {
                    client.Close();
                    throw new Exception($"Неверный идентификатор пакета в ответе. Ожидалось 0x83, получено {recvPacketId}");
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
                        throw new Exception($"Обрезанный ответ: {bytesRead} из {contentLen}");
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
                            throw new Exception("Некорректная длина ответа с числом с плавающей точкой.");
                        }
                        float floatValue = BitConverter.ToSingle(response, 0);
                        data = floatValue;
                    }
                    else
                    {
                        // Неизвестный тип ответа, возвращаем сырые данные
                        data = response;
                    }

                    client.Close();
                    return (responseType, data);
                }
            }
        }
    }

    /// <summary>
    /// Запрашивает статус сервера.
    /// </summary>
    /// <param name="address">IP-адрес или DNS сервера DreamDaemon.</param>
    /// <param name="port">Порт сервера DreamDaemon.</param>
    /// <returns>Словарь с данными статуса сервера.</returns>
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
    /// Запрашивает количество игроков на сервере.
    /// </summary>
    /// <param name="address">IP-адрес или DNS сервера DreamDaemon.</param>
    /// <param name="port">Порт сервера DreamDaemon.</param>
    /// <returns>Количество игроков в виде строки.</returns>
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
    /// Асинхронно читает заданное количество байт из потока.
    /// </summary>
    private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            int bytesRead = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cts.Token);
            if (bytesRead == 0)
            {
                break; // Конец потока
            }
            totalRead += bytesRead;
        }
        return totalRead;
    }

    /// <summary>
    /// Создает строку запроса из словаря аргументов.
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
    /// Парсит строку запроса в словарь.
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
