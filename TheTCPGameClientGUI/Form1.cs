using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheTCPGameClientGUI
{
    public partial class Form1 : Form
    {
        PictureBox[][] GUI_items = null;
        public Form1()
        {
            InitializeComponent();
            GUI_items = new PictureBox[3][];
            for (int i = 0; i < 3; ++i) 
            {
                GUI_items[i] = new PictureBox[3];
            }

            GUI_items[0][0] = pictureBox1;
            GUI_items[0][1] = pictureBox2;
            GUI_items[0][2] = pictureBox3;
            GUI_items[1][0] = pictureBox4;
            GUI_items[1][1] = pictureBox5;
            GUI_items[1][2] = pictureBox6;
            GUI_items[2][0] = pictureBox7;
            GUI_items[2][1] = pictureBox8;
            GUI_items[2][2] = pictureBox9;

            for (int i = 0; i < 3; ++i) 
            {
            
                for (int j = 0; j < 3; ++j) 
                {
                    GUI_items[i][j].Dock = DockStyle.Fill;
                    GUI_items[i][j].SizeMode = PictureBoxSizeMode.Zoom;
                    GUI_items[i][j].Image = il_itemsImages.Images[2];
                    GUI_items[i][j].Tag = new Tuple<int, int>(i, j);
                    GUI_items[i][j].Click += (object sender, EventArgs e) => {
                        Tuple<int, int> t = ((sender as PictureBox).Tag as Tuple<int, int>);
                        MessageBox.Show(t.Item1 + " " + t.Item2);
                        
                        (sender as PictureBox).Image = il_itemsImages.Images[0]; };
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
