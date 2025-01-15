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
    private static List<TcpClient> _clients = new List<TcpClient>();
    private const int Port = 8081;

    static void Main()
    {
        _server = new TcpListener(IPAddress.Any, Port);
        _server.Start();
        Console.WriteLine($"Server started on port {Port}");

        while (true)
        {
            TcpClient client = _server.AcceptTcpClient();
            Console.WriteLine("New client connected.");
            _clients.Add(client);

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);

        try
        {
            while (true)
            {
                string message = reader.ReadLine();
                if (message == null) break;
                Console.WriteLine($"Received: {message}");
                BroadcastMessage(message, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client disconnected: {ex.Message}");
        }
        finally
        {
            _clients.Remove(client);
            client.Close();
        }
    }

    private static void BroadcastMessage(string message, TcpClient sender)
    {
        foreach (var client in _clients)
        {
            if (client == sender) continue;

            try
            {
                StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(message);
            }
            catch
            {
                Console.WriteLine("Failed to send message to a client.");
            }
        }
    }
}
