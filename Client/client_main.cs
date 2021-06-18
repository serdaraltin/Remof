using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
namespace Client
{
    public partial class client_main : Form
    {
        public client_main()
        {
            InitializeComponent();
        }

        private void btn_baglan_Click(object sender, EventArgs e)
        {
           Ekran ekran = new Ekran(textBox1.Text);
            ekran.Show();
        }
    }
}
