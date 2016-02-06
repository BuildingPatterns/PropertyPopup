using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PropertyPopup
{
    public partial class Form1 : Form
    {
        CadDraw draw;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            draw = new CadDraw(pictureBox1);
            draw.CreateSomeObjects(12);
            draw.Popupmenu = contextMenuStrip1;
            draw.PopupOpening();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            draw.PopupOpening();
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            draw.PopupClosing(); 
        }
    }
}
