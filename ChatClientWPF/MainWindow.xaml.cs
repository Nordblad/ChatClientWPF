using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<ChatLine> chatLines = new ObservableCollection<ChatLine>();
        TcpClient client = null;
        NetworkStream stream = null;
        //bool connected = false;
        Thread waitThread = null;

        public MainWindow()
        {
            InitializeComponent();
            //Fejkmeddelanden för design
            //chatLines.Add(new ChatLine { UserName = "Knacke", Message = "Lorem upsum dolor sit amet!" });
            //chatLines.Add(new ChatLine { UserName = "Egot", Message = "Lorem upsum dolor sit amet! Längre meddelande!" });
            //chatLines.Add(new ChatLine { UserName = "Mamma", Message = "Kort meddelande" });
            //messageList.Items.Add(chatLines);
            serverPicker.ItemsSource = ReadServerListFromFile();
            serverPicker.SelectedIndex = 0;
            messageList.ItemsSource = chatLines;
        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            //addMsg("Knacke", inputBox.Text);
            Byte[] msg = Encoding.Unicode.GetBytes(inputBox.Text);
            stream.Write(msg, 0, msg.Length);
            inputBox.Text = "";
            inputBox.Focus();
        }

        private void addMsg (string userName, string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => chatLines.Add(new ChatLine { UserName = userName, Message = msg })));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => chatScroll.ScrollToBottom()));
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                ChatServer server = (ChatServer)serverPicker.SelectedItem;
                try
                {
                    client = new TcpClient(server.Ip, server.Port);
                }
                catch
                {
                    MessageBox.Show("Could not connect to " + server.Ip + ":" + server.Port);
                    return;
                }

                stream = client.GetStream();
                UiToggleConnected(true);
                //Skicka username först
                Byte[] msg = Encoding.Unicode.GetBytes(nameBox.Text);
                stream.Write(msg, 0, msg.Length);
                waitThread = new Thread(new ThreadStart(WaitForResponse));
                //connected = true;
                waitThread.Start();

            }
            else
            {
                waitThread.Abort();
                stream.Close();
                client.Close();
                client = null;
                UiToggleConnected(false);
            }
        }
        private void UiToggleConnected (bool connected)
        {
            statusLabel.Foreground = connected ? Brushes.LawnGreen : Brushes.DarkRed;
            statusLabel.Content = connected ? "Connected" : "Offline";
            connectBtn.Content = connected ? "Disconnect" : "Connect";
            inputBox.IsEnabled = connected;
            sendBtn.IsEnabled = connected;
            nameBox.IsEnabled = !connected;
            serverPicker.IsEnabled = !connected;
            SetValue(TitleProperty, connected ? "Chat" : "Chat - Offline");
        }
        private void WaitForResponse()
        {
            while (true)
            {
                byte[] recievedMessage = new byte[512];
                stream.Read(recievedMessage, 0, recievedMessage.Length);
                //Delar upp i användarnamn och meddelande. Inte helt vackert kanske? Lägg till koder för annat..
                string[] messageSplit = Encoding.Unicode.GetString(recievedMessage).TrimEnd('\0').Split('$');
                addMsg(messageSplit[0], messageSplit[1]);
            }
        }

        private List<ChatServer> ReadServerListFromFile ()
        {
            var ServerList = new List<ChatServer>();
            string line;
            string[] lineParts;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader("ServerList.txt"); //"ServerList.txt");

            while ((line = file.ReadLine()) != null)
            {
                lineParts = line.Split(';');
                ServerList.Add(new ChatServer { Name = lineParts[0], Ip = lineParts[1], Port = int.Parse(lineParts[2]) });
            }

            file.Close();
            return ServerList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
            {
                waitThread.Abort();
                client.Close();
            }
        }
    }
}
