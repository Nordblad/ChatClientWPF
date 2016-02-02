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

            chatLines.Add(new ChatLine { UserName = "Knacke", Message = "Lorem upsum dolor sit amet!" });
            chatLines.Add(new ChatLine { UserName = "Egot", Message = "Lorem upsum dolor sit amet! Längre meddelande!" });
            chatLines.Add(new ChatLine { UserName = "Mamma", Message = "Kort meddelande" });
            //messageList.Items.Add(chatLines);
            messageList.ItemsSource = chatLines;
        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {
            //addMsg("Knacke", inputBox.Text);
            Byte[] msg = Encoding.Unicode.GetBytes(inputBox.Text);
            stream.Write(msg, 0, msg.Length);
            inputBox.Text = "";
        }

        private void addMsg (string userName, string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => this.chatLines.Add(new ChatLine { UserName = userName, Message = msg })));
            Application.Current.Dispatcher.BeginInvoke(new Action(() => chatScroll.ScrollToBottom()));
        }

        //FRÅN LEKTIONEN===========================
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                
                client = new TcpClient("127.0.0.1", 5000);
                stream = client.GetStream();
                UiToggleConnected(true);
                //Skicka username först
                Byte[] msg = Encoding.Unicode.GetBytes("Knecke");
                stream.Write(msg, 0, msg.Length);
                waitThread = new Thread(new ThreadStart(WaitForResponse));
                //connected = true;
                waitThread.Start();

            }
            else
            {
                //connected = false;
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
        }
        private void WaitForResponse()
        {
            while (true)
            {
                byte[] recievedMessage = new byte[256];
                stream.Read(recievedMessage, 0, recievedMessage.Length);
                //Delar upp i användarnamn och meddelande. Inte helt vackert kanske? Lägg till koder för annat..
                string[] messageSplit = Encoding.Unicode.GetString(recievedMessage).TrimEnd('\0').Split('$');
                addMsg(messageSplit[0], messageSplit[1]);
                //SetResponseMessage(Encoding.ASCII.GetString(recievedMessage));
            }
        }
    }
}
