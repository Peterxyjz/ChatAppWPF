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
                // Yêu cầu nhập tên
                string clientName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter your name:",
                    "Name Required",
                    "Client");

                if (string.IsNullOrWhiteSpace(clientName))
                {
                    MessageBox.Show("Name cannot be empty!");
                    Close();
                    return;
                }

                // Kết nối tới server
                _client = new TcpClient(ServerIp, ServerPort);
                NetworkStream stream = _client.GetStream();
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                _reader = new StreamReader(stream, Encoding.UTF8);

                // Gửi tên tới server
                _writer.WriteLine(clientName);

                // Bắt đầu luồng nhận tin nhắn
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}");
                Close();
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

                    // Cập nhật tin nhắn vào ChatBox
                    Dispatcher.Invoke(() => ChatBox.Text += $"{message}\n");
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    ChatBox.Text += $"Error receiving messages: {ex.Message}\n"
                );
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = InputMessage.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    // Gửi tin nhắn tới server
                    _writer.WriteLine(message);

                    // Xóa hộp nhập sau khi gửi
                    InputMessage.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    ChatBox.Text += $"Error sending message: {ex.Message}\n";
                }
            }
        }
    }
}
