using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Show_Invested_Coins
{
    public partial class Form2 : Form
    {
        public Form1 p;

        public Form2(Form1 parent)
        {
            p = parent;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            p.ChangeTextBoxText = "0";
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
