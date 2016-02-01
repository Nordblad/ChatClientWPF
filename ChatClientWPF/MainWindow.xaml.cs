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
            Byte[] msg = Encoding.ASCII.GetBytes(inputBox.Text);
            stream.Write(msg, 0, msg.Length);
            inputBox.Text = "";
        }

        private void addMsg (string userName, string msg)
        {
            //if (InvokeRequired)
            //{
            //    //behöver inte riktigt förstå
            //    this.Invoke(new Action<string, string>(addMsg), new object[] { userName, msg });
            //    return;
            //}
            Application.Current.Dispatcher.BeginInvoke(new Action(() => this.chatLines.Add(new ChatLine { UserName = userName, Message = msg })));

            //chatLines.Add(new ChatLine { UserName = userName, Message = msg });
            Application.Current.Dispatcher.BeginInvoke(new Action(() => chatScroll.ScrollToBottom()));
        }

        //FRÅN LEKTIONEN===========================
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                client = new TcpClient("127.0.0.1", 5000);
                stream = client.GetStream();
                statusLabel.Foreground = Brushes.LawnGreen;
                statusLabel.Content = "Connected";
                connectBtn.Content = "Disconnect";
                Thread waitThread = new Thread(new ThreadStart(WaitForResponse));
                waitThread.Start();

            }
            else
            {
                stream.Close();
                client.Close();
                statusLabel.Foreground = Brushes.DarkRed;
                statusLabel.Content = "Not connected";
                connectBtn.Content = "Connect";
            }
        }
        private void WaitForResponse()
        {
            byte[] recievedMessage = new byte[256];
            while (true)
            {
                stream.Read(recievedMessage, 0, recievedMessage.Length);
                addMsg("test", Encoding.Unicode.GetString(recievedMessage).TrimEnd('\0'));
                //SetResponseMessage(Encoding.ASCII.GetString(recievedMessage));
            }
        }
    }
}
