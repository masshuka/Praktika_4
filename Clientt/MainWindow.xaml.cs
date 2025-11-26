using Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Clientt
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPAddress ipAddress;
        private int port;
        private int userId = -1;
        private Stack<string> directoryStack = new Stack<string>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(txtIpAddress.Text, out ipAddress) && int.TryParse(txtPort.Text, out port))
            {
                string login = txtLogin.Text;
                string password = txtPassword.Password;
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    var response = SendCommand($"connect {login} {password}");
                    if (response?.Command == "autorization")
                    {
                        userId = int.Parse(response.Data);
                        MessageBox.Show("Подключение успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadDirectories();
                    }
                    else
                    {
                        MessageBox.Show(response?.Data ?? "Ошибка авторизации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите корректный IP и порт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public Socket Connecting(IPAddress ipAddress, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
                return socket;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (socket != null && !socket.Connected)
                {
                    socket.Close();
                }
            }
            return null;
        }
        private void LoadDirectories()
        {
            try
            {
                var response = SendCommand("cd");
                if (response?.Command == "cd")
                {
                    var directories = JsonConvert.DeserializeObject<string[]>(response.Data);
                    lstDirectories.Items.Clear();

                    if (directoryStack.Count > 0)
                    {
                        lstDirectories.Items.Add("Назад");
                    }

                    foreach (var dir in directories)
                    {
                        lstDirectories.Items.Add(dir);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить список директорий.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                SendFileToServer(filePath);
            }
        }
    }
}
