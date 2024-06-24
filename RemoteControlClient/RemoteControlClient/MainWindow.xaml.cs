using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace RemoteControlClient
{
    public partial class MainWindow : Window
    {
        NetworkStream networkStream;

        public MainWindow()
        {
            InitializeComponent();
            Thread clientThread = new Thread(ConnectToServer);
            clientThread.Start();
        }

        void ConnectToServer()
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 5000);
                networkStream = client.GetStream();
                Thread receiveScreenThread = new Thread(ReceiveScreen);
                receiveScreenThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        void ReceiveScreen()
        {
            while (true)
            {
                try
                {
                    byte[] bufferLength = new byte[4];
                    networkStream.Read(bufferLength, 0, 4);
                    int length = BitConverter.ToInt32(bufferLength, 0);

                    byte[] buffer = new byte[length];
                    int bytesRead = 0;
                    while (bytesRead < length)
                    {
                        bytesRead += networkStream.Read(buffer, bytesRead, length - bytesRead);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        using (var ms = new MemoryStream(buffer))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            RemoteScreen.Source = bitmap;
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    break;
                }
            }
        }
    }
}