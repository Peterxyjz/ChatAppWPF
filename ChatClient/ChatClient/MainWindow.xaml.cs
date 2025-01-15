using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private TcpClient _client = null!;
        private StreamWriter _writer = null!;
        private StreamReader _reader = null!;
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 8081;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                // Establish the connection to the server
                _client = new TcpClient(ServerIp, ServerPort);
                NetworkStream stream = _client.GetStream();
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                _reader = new StreamReader(stream, Encoding.UTF8);

                // Start a background thread to receive messages
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.AppendText($"Failed to connect to server: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    string? message = _reader.ReadLine();
                    if (message == null) break;

                    // Update the ChatBox with the received message
                    Dispatcher.Invoke(() => ChatBox.Text += $"{message}\n");
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    MessageBox.AppendText($"Error receiving messages: {ex.Message}")
                );
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    // Send the message to the server
                    _writer.WriteLine(message);

                    // Clear the input box after sending
                    MessageBox.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.AppendText($"Error sending message: {ex.Message}");
                }
            }
        }
    }
}
