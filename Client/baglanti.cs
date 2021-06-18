using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
namespace Client
{
    public partial class Ekran : Form
    {
        Socket resimAl = null;
        byte[] dizi = new byte[9999999];
        string karsiIP="";
        public Ekran(string IP)
        {
            karsiIP = IP;
            InitializeComponent();
        }

        private void baglanti_pc_Load(object sender, EventArgs e)
        {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(host);
            ilkGonderme(ip.AddressList[0].ToString());
            resimAl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            resimAl.Bind(new IPEndPoint(IPAddress.Parse(ip.AddressList[0].ToString()), 1453));
            resimAl.Listen(1);
            resimAl.BeginAccept(new AsyncCallback(Baglandinda), null);            
        }
        void Baglandinda(IAsyncResult iar)
        {
            Socket soket = resimAl.EndAccept(iar);
            soket.BeginReceive(dizi, 0, dizi.Length, SocketFlags.None, new AsyncCallback(veriGeldiginde), soket);
            resimAl.BeginAccept(new AsyncCallback(Baglandinda), null);
        }
        void veriGeldiginde(IAsyncResult iar)
        {
            Socket soket = (Socket)iar.AsyncState;
            int uzunluk = soket.EndReceive(iar);
            byte[] veri = new byte[uzunluk];
            Array.Copy(dizi, veri, veri.Length);
            if (uzunluk < 50)
            {
                string gelen = Encoding.UTF8.GetString(veri);
                string genislik = gelen;
                genislik = genislik.Substring(0, genislik.IndexOf(":"));
                int width =int.Parse(genislik);
                string yukseklik = gelen;
                yukseklik = yukseklik.Remove(0, yukseklik.IndexOf(":") + 1);
                int heigth = int.Parse(yukseklik);
                this.Size = new Size(width+16, heigth+38);
             
            }
            else
            {
                MemoryStream ms = new MemoryStream(veri);
                Image resim = Bitmap.FromStream(ms);
                pictureBox1.BackgroundImage = resim;
            }
        }
        void ilkGonderme(string ip)
        {
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(IPAddress.Parse(karsiIP), 1453);
            byte[] gonderilecek = Encoding.UTF8.GetBytes(ip+"|AC");
            soket.Send(gonderilecek);
            soket.Close();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            byte[] gonderilecek = null;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                gonderilecek = Encoding.UTF8.GetBytes("Left:MouseDown");
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                gonderilecek = Encoding.UTF8.GetBytes("Right:MouseDown");
            else
                return;
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(IPAddress.Parse(karsiIP), 1453);
            soket.Send(gonderilecek);
            soket.Close();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(IPAddress.Parse(karsiIP), 1453);
            byte[] gonderilecek = Encoding.UTF8.GetBytes(x.ToString() + ":" + y.ToString() + "|MouseMove");
            soket.Send(gonderilecek);
            soket.Close();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            byte[] gonderilecek = null;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                gonderilecek = Encoding.UTF8.GetBytes("Left:MouseUp");
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                gonderilecek = Encoding.UTF8.GetBytes("Right:MouseUp");
            else
                return;
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(IPAddress.Parse(karsiIP), 1453);
            soket.Send(gonderilecek);
            soket.Close();
        }

        private void Ekran_KeyUp(object sender, KeyEventArgs e)
        {
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(IPAddress.Parse(karsiIP), 1453);
            byte[] gonderilecek = Encoding.UTF8.GetBytes(e.KeyValue.ToString()+":Key");
            soket.Send(gonderilecek);
            soket.Close();
        }
    }
}
