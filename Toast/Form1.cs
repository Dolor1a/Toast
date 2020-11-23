using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Toast
{
    public partial class Form1 : Form
    {
        private byte[] result = new byte[1024 * 1024];
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Dictionary<string, Socket> ClientInformation = new Dictionary<string, Socket> { };
        NotifyIcon notifyIcon1 = new NotifyIcon();
        public Form1()
        {
            InitializeComponent();
            //SetBalloonTip();
            //this.ShowInTaskbar = false;
            //this.ShowInTaskbar = false;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Minimized;
            //this.Location = new System.Drawing.Point(-1000, -1000);
            //this.Size = new System.Drawing.Size(0, 0);
            //this.Visible = false;
            //this.Show();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }
        private void SetBalloonTip()
        {
            NotifyIcon notifyIcon1 = new NotifyIcon();
            notifyIcon1.Icon = SystemIcons.Exclamation; ;
            notifyIcon1.BalloonTipTitle = "Balloon Tip Title";
            notifyIcon1.BalloonTipText = "Balloon Tip Text.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(500);


        }

        void Form1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)//启动服务
        {
            //SetBalloonTip();
            try
            {
                int Port = Convert.ToInt32(9999);
                IPAddress IP = IPAddress.Parse((string)"192.168.5.69");
                ServerSocket.Bind(new IPEndPoint(IP, Port));
                ServerSocket.Listen(10);
                richTextBox2.Text += "启动监听成功！\r\n";
                richTextBox2.Text += "监听本地" + ServerSocket.LocalEndPoint.ToString() + "成功\r\n";
                Thread ThreadListen = new Thread(ListenConnection);
                ThreadListen.IsBackground = true;
                ThreadListen.Start();
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                richTextBox2.Text += "监听异常！！！\r\n";
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close();
            }
        }
        Socket ConnectionSocket = null;
        private void ListenConnection()
        {           
            while (true)
            {
                try
                {
                    ConnectionSocket = ServerSocket.Accept();
                }
                catch (Exception ex)
                {
                    richTextBox2.Text += "监听套接字异常" + ex.Message;
                    //Console.WriteLine("监听套接字异常{0}", ex.Message);
                    break;
                }
                //获取客户端端口号和IP
                IPAddress ClientIP = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Address;
                int ClientPort = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Port;
                string SendMessage = "本地IP:" + ClientIP +
                    ",本地端口:" + ClientPort.ToString();
                ConnectionSocket.Send(Encoding.UTF8.GetBytes(SendMessage));
                string remotePoint = ConnectionSocket.RemoteEndPoint.ToString();
                richTextBox2.Text += "成功与客户端" + remotePoint + "建立连接\r\n";
                richTextBox3.Text += DateTime.Now + ":" + remotePoint + "\r\n";
                ClientInformation.Add(remotePoint, ConnectionSocket);
                Thread thread = new Thread(ReceiveMessage);
                thread.IsBackground = true;
                thread.Start(ConnectionSocket);
            }
        }
        public void Send()
        {
            //获取客户端端口号和IP
            IPAddress ClientIP = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Address;
            int ClientPort = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Port;
            string SendMessage = "本地IP:" + ClientIP +
                ",本地端口:" + ClientPort.ToString();
            ConnectionSocket.Send(Encoding.UTF8.GetBytes(SendMessage));
        }
        private void ReceiveMessage(Object SocketClient)
        {
            Socket ReceiveSocket = (Socket)SocketClient;
            while (true)
            {
                byte[] result = new byte[1024 * 1024];
                try
                {
                    IPAddress ClientIP = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Address;
                    int ClientPort = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Port;
                    int ReceiveLength = ReceiveSocket.Receive(result);
                    string str = ReceiveSocket.RemoteEndPoint.ToString();
                    string ReceiveMessage = Encoding.UTF8.GetString(result, 0, ReceiveLength);
                    richTextBox1.Text += "接收客户端:" + ReceiveSocket.RemoteEndPoint.ToString() +
                    "时间：" + DateTime.Now.ToString() + "\r\n" + "消息：" + ReceiveMessage + "\r\n";
                    //Thread ThreadListen = new Thread(Send);
                    //ThreadListen.IsBackground = true;
                    //ThreadListen.Start();
                    //if (ClientInformation.Count == 1) continue;//只有一个客户端
                    List<string> test = new List<string>(ClientInformation.Keys);
                    for (int i = 0; i < ClientInformation.Count; i++)
                    {
                        Socket socket = ClientInformation[test[i]];
                        string s = ReceiveSocket.RemoteEndPoint.ToString();
                        //if (test[i] != s)
                        {
                            richTextBox1.Text += DateTime.Now + "\r\n" + "客户端" + str + "向客户端" + test[i] + "发送消息：" + ReceiveMessage;
                            string ReceiveMessage1 = DateTime.Now + "\r\n" + "客户端" + str + "向您发送消息：" + ReceiveMessage;
                            socket.Send(Encoding.UTF8.GetBytes(ReceiveMessage1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    richTextBox2.Text += "监听出现异常！\r\n";
                    richTextBox2.Text += "客户端" + ReceiveSocket.RemoteEndPoint + "已经连接中断" + "\r\n" +
                    ex.Message + "\r\n" + ex.StackTrace + "\r\n";
                    string s = ReceiveSocket.RemoteEndPoint.ToString();
                    ClientInformation.Remove(s);
                    ReceiveSocket.Shutdown(SocketShutdown.Both);
                    ReceiveSocket.Close();
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread ThreadListen = new Thread(Send);
            ThreadListen.IsBackground = true;
            ThreadListen.Start();
        }
    }
}
