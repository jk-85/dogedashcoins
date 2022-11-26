using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Show_Invested_Coins
{
    public partial class Form4 : Form
    {
        public Form1 p;
        public Form4(Form1 parent)
        {
            InitializeComponent();
            p = parent;
            this.AcceptButton = button1;
            int e = p.easy_coins;
            int n = p.normal_coins;
            int el = p.elite_coins;

            textBox1.Text = e.ToString();
            textBox2.Text = n.ToString();
            textBox3.Text = el.ToString();
            textBox4.Text = p.userpass;

            if (p.useLocalStorage)
            {
                checkBox1.Checked = false;
                textBox4.Enabled = false;
            }
            else
            {
                checkBox1.Checked = true;
                textBox4.Enabled = true;
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkCoins();
        }

        private void checkCoins()
        {
            // Validate, if error: Set standard coins for playing level                                                                 
            try
            {
                p.easy_coins = Int32.Parse(textBox1.Text);
            }
            catch (FormatException)
            {
                p.easy_coins = 3;
                textBox1.Text = "3";
            }

            try
            {
                p.normal_coins = Int32.Parse(textBox2.Text);
            }
            catch (FormatException)
            {
                p.normal_coins = 5;
                textBox2.Text = "5";
            }

            try
            {
                p.elite_coins = Int32.Parse(textBox3.Text);
            }
            catch (FormatException)
            {
                p.elite_coins = 7;
                textBox3.Text = "7";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            checkCoins();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Unchecked)
            {
                p.useLocalStorage = true;
                p.writeFile("dd_uselocal", "");
                p.deleteFile("dd_account");
            }
            else
            {
                p.useLocalStorage = false;
                p.deleteFile("dd_uselocal");
                p.userpass = textBox4.Text;
            }

            if (checkBox1.Checked) {
                if (Form1.testRandomString(textBox4.Text))
                {
                    p.userpass = textBox4.Text;
                    // Write userpass in local file to access it after restart of coin calculator
                    // TODO: Validate textBox4 !!!!!!!!!!!!!!!!
                    p.writeFile("dd_account", textBox4.Text);
                }
                else
                {
                    using (DialogCenteringService centeringService = new DialogCenteringService(this)) // center message box
                    {
                        MessageBox.Show("Your password has incorrect format. Please use only alpha-numeric letters and an exact length of 10!", "Attention");
                        return;
                    }
                }
            }

            // TODO: If user enters other account, do NOT use coin-values in these three fields and load the correct values from server!
            p.label4.Text = "+" + Int32.Parse(textBox1.Text).ToString();
            p.label5.Text = "+" + Int32.Parse(textBox2.Text).ToString();
            p.label6.Text = "+" + Int32.Parse(textBox3.Text).ToString();

            if (p.useLocalStorage) {
                p.writeFile("dd_values", p.easy_coins + "," + p.normal_coins + "," + p.elite_coins);
                p.loadOfflineValuesIntoWindow();
                // Copy actual checkbox-states for offline remembering
                p.writeCheckBoxStatesToFile();
            }
            else
            {
                p.writeToServer("values", Int32.Parse(textBox1.Text).ToString() +
                    "," + Int32.Parse(textBox2.Text).ToString() +
                    "," + Int32.Parse(textBox3.Text).ToString());
                p.writeToServer("config", p.checkBox1.Checked + "|" + p.checkBox2.Checked);
                p.loadOnlineValuesIntoWindow();
            }
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(p.userpass);
            Form5 form5 = new Form5();
            form5.TopMost = true;
            form5.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.CheckState == CheckState.Unchecked)
            {
                textBox4.Enabled = false;
                label6.Visible = false;
            }
            else
            {
                textBox4.Enabled = true;
                textBox4.Text = Form1.RandomString(10);
                label6.Visible = true;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            checkCoins();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
