using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    private static TcpListener _server;
    private static Dictionary<IPAddress, string> _clientData = new Dictionary<IPAddress, string>();
    private const int Port = 8081;

    static void Main()
    {
        // Địa chỉ IP cụ thể của server
        string serverIp = "192.168.206.142"; // Thay đổi IP theo nhu cầu
        IPAddress ipAddress = IPAddress.Parse(serverIp);

        _server = new TcpListener(ipAddress, Port);
        _server.Start();
        Console.WriteLine($"Server started on {serverIp}:{Port}");

        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            IPEndPoint? remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

            if (remoteEndPoint != null)
            {
                Console.WriteLine($"New connection from {remoteEndPoint.Address}");
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        IPEndPoint? remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        string clientName = string.Empty;

        // Tạo hoặc mở file để lưu tin nhắn
        string logFilePath = "server_log.txt";
        using StreamWriter logWriter = new StreamWriter(logFilePath, append: true, Encoding.UTF8);

        try
        {
            if (remoteEndPoint != null)
            {
                IPAddress clientIp = remoteEndPoint.Address;

                // Kiểm tra nếu IP đã tồn tại
                if (_clientData.ContainsKey(clientIp))
                {
                    clientName = _clientData[clientIp];
                    Console.WriteLine($"Returning client {clientName} ({clientIp}) connected.");
                    writer.WriteLine($"Welcome back, {clientName}!");
                }
                else
                {
                    // Nhận tên mới từ client
                    writer.WriteLine("Enter your name:");
                    clientName = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(clientName))
                        throw new Exception("Client name is empty.");

                    _clientData[clientIp] = clientName;
                    Console.WriteLine($"New client {clientName} ({clientIp}) connected.");
                    writer.WriteLine($"Welcome, {clientName}!");
                }
            }

            while (true)
            {
                string message = reader.ReadLine();
                if (message == null) break;

                Console.WriteLine($"Received from {clientName}: {message}");

                // Lưu tin nhắn vào file
                logWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {clientName}: {message}");
                logWriter.Flush();

                // Gửi tin nhắn tới các client khác
                BroadcastMessage($"{clientName}: {message}", client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client {clientName} disconnected: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static void BroadcastMessage(string message, TcpClient sender)
    {
        foreach (var clientIp in _clientData.Keys)
        {
            try
            {
                TcpClient? client = _clientData.ContainsKey(clientIp) ? sender : null;
                if (client != null)
                {
                    StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    writer.WriteLine(message);
                }
            }
            catch
            {
                Console.WriteLine("Failed to send message to a client.");
            }
        }
    }
}
