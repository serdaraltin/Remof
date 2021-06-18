using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
namespace Server
{

    public partial class server_main : Form
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData,UIntPtr dwExtraInfo);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,UIntPtr dwExtraInfo);
        [Flags]
        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            XDOWN = 0x00000080,
            XUP = 0x00000100
        }
        Socket mouse_klavye_dinleme = null;
        byte[] dizi = new byte[200];
        delegate void ResimGonderHandler();
        string karsi_ip = null;
        public server_main()
        {
            InitializeComponent();
        }
        public static string GetIP()
        {

            string externalIP;
            externalIP = (new System.Net.WebClient()).DownloadString("http://checkip.dyndns.org/");
            externalIP = (new System.Text.RegularExpressions.Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
            return externalIP;

        }

        private void server_main_Load(object sender, EventArgs e)
        {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(host);
            lbl_ip.Text = ip.AddressList[0].ToString();
            mouse_klavye_dinleme = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mouse_klavye_dinleme.Bind(new IPEndPoint(IPAddress.Parse(ip.AddressList[0].ToString()), 1453));
            mouse_klavye_dinleme.Listen(1);
            mouse_klavye_dinleme.BeginAccept(new AsyncCallback(baglandi), null);

            lbl_ip.Text = ip.AddressList[0].ToString();
        }
        void baglandi(IAsyncResult iar)
        {
            Socket soket=mouse_klavye_dinleme.EndAccept(iar);
            soket.BeginReceive(dizi, 0, dizi.Length, SocketFlags.None, new AsyncCallback(verigirisi), soket);
            mouse_klavye_dinleme.BeginAccept(new AsyncCallback(baglandi), null);
        }
        void verigirisi(IAsyncResult iar)
        {
            Socket soket = (Socket)iar.AsyncState;
            int gelenveriuzunluk=soket.EndReceive(iar);
            byte[] veri = new byte[gelenveriuzunluk];
            Array.Copy(dizi, veri, gelenveriuzunluk);
            string islenecek = Encoding.UTF8.GetString(veri);
            if (islenecek.Contains("AC"))
            {
                karsi_ip = islenecek.Substring(0, islenecek.IndexOf("|"));
                string gonderilecek = Screen.PrimaryScreen.Bounds.Width.ToString()+ ":" + Screen.PrimaryScreen.Bounds.Height.ToString();
                ekranCozunurlukGonder(gonderilecek);
                ResimGonderHandler resimgonder = new ResimGonderHandler(Resimgonder);
                resimgonder.BeginInvoke(new AsyncCallback(islemsonlandi), null);
            }
            else
            {
                if (islenecek.Contains("MouseMove"))
                {
                    string xkordinat = islenecek;
                    xkordinat = xkordinat.Substring(0, xkordinat.IndexOf(":"));
                    string ykordinat = islenecek;
                    ykordinat = ykordinat.Remove(0, ykordinat.IndexOf(":") + 1);
                    ykordinat = ykordinat.Substring(0, ykordinat.IndexOf("|"));
                    int x = int.Parse(xkordinat);
                    int y = int.Parse(ykordinat);
                    Cursor.Position = new Point(x, y);
                }
                else if (islenecek.Contains("MouseDown"))
                {
                    string tus = islenecek;
                     tus = tus.Substring(0, tus.IndexOf(":"));
                     if (tus == "Left")
                     {
                         mouse_event(0x00000002, 0, 0, 0, UIntPtr.Zero);
                     }
                     else
                         mouse_event(0x00000008, 0, 0, 0, UIntPtr.Zero);
                }
                else if (islenecek.Contains("MouseUp"))
                {
                    string tus = islenecek;
                    tus = tus.Substring(0, tus.IndexOf(":"));
                    if (tus == "Left")
                    {
                        mouse_event(0x00000004, 0, 0, 0, UIntPtr.Zero);
                    }
                    else
                        mouse_event(0x00000010, 0, 0, 0, UIntPtr.Zero);
                }
                else if(islenecek.Contains("Key"))
                {
                    string basılantus = islenecek;
                    basılantus = basılantus.Substring(0, basılantus.IndexOf(":"));
                    byte keycode = byte.Parse(basılantus);
                    //keybd_event(keycode, 0x45, 0x1, UIntPtr.Zero);
                    keybd_event(keycode, 0x45, 0x1, UIntPtr.Zero);
                    keybd_event(keycode, 0x45, 0x2, UIntPtr.Zero);
                }
            }
        }

        void ekranCozunurlukGonder(string gonderilecek)
        {
            Socket baglan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            baglan.Connect(IPAddress.Parse(karsi_ip), 1453);
            byte[] coz = Encoding.UTF7.GetBytes(gonderilecek);
            baglan.Send(coz, 0, coz.Length, SocketFlags.None);
            baglan.Close();
        }
        void Resimgonder()
        {
            while(true)
            {
                Socket baglan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                baglan.Connect(IPAddress.Parse(karsi_ip), 1453);
                byte[] ekran = EkranGoruntusu();
                baglan.Send(ekran, 0, ekran.Length, SocketFlags.None);
                baglan.Close();
            }
        }
        byte[] EkranGoruntusu()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics grf = Graphics.FromImage(bmp);
            grf.CopyFromScreen(0, 0, 0, 0, new Size(bmp.Width, bmp.Height));
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.GetBuffer();
        }
        void islemsonlandi(IAsyncResult iar)
        {

        }

  
    }
}
