using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Globalization;

namespace Show_Invested_Coins
{
    public partial class Form6 : Form
    {
        public Form1 p;
        private int index = 0;
        private Color standardcolor;
        private bool lbconvertflag = false;   // Used for old leaderboard-format when user selects old board
        PrivateFontCollection pfc = new PrivateFontCollection();   // Important, only declare here, could be collected via garbage collection inside function!

        public Form6(Form1 parent)
        {
            InitializeComponent();
            p = parent;
            pfc.AddFontFile("ARCO.ttf");
        }
        private void loadLatestLB(string command)
        {
            if(!p.checkServer())
            {
                MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. Could also be a server-fault. Please try again later. Leaderboard-data could not be retrieved!");
                return;
            }

            try
            {
                if (command != "NULL")
                {
                    p.readFromServer(command);
                    //MessageBox.Show(p.leaderboard);
                }

                if (p.leaderboard == "NEWDAY") {
                    // Leaderboard is empty because new day starts
                    if (p.leaderboard == "NEWDAY")
                    {
                        MessageBox.Show("There is no leaderboard-data at the moment because it seems it's a new day and a new leaderboard just started!");
                    }
                    else
                    {
                        MessageBox.Show("Other");
                    }
                    return;
                }

                int place = 0;
                foreach (string item in p.leaderboard.Split("#"))
                {
                    if (item.Contains("rewardpool="))
                    {
                        if (lbconvertflag)
                        {
                            label2.Text = (Int32.Parse(item.Split("=")[1]) / 100).ToString();
                        }
                        else
                        {
                            label2.Text = item.Split("=")[1];
                        }
                        continue;
                    }
                    if (item.Contains("updatedAt="))
                    {
                        label3.Text = ((item.Split("=")[1]).Insert(10, "\r\n")).Remove(12, 1);  // New line before "T"
                        continue;
                    }
                    ListViewItem item1 = new ListViewItem((++place).ToString());
                    item1.SubItems.Add((item.Split(";")[0]));   // Username
                    item1.SubItems.Add((item.Split(";")[1]));   // Game-Token

                    if (lbconvertflag)
                    {
                        item1.SubItems.Add((Int32.Parse((item.Split(";")[2]))/100).ToString());   // Reward-Token
                    }
                    else
                    {
                        item1.SubItems.Add((item.Split(";")[2]));   // Reward-Token
                    }
                    listView1.Items.Add(item1);
                }
                standardcolor = listView1.Items[0].BackColor;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Leaderboard could not be read from server. Please try again later. "+e.Data.ToString());
                //MessageBox.Show(p.leaderboard);
            }
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            // Copy in Debug-Folder if not there!
            label1.Font = new Font(pfc.Families[0], 14, FontStyle.Regular);
            label2.Font = new Font(pfc.Families[0], 14, FontStyle.Regular);
            label4.Font = new Font(pfc.Families[0], 14, FontStyle.Regular);
            label3.Font = new Font(pfc.Families[0], 14, FontStyle.Regular);
            label6.Font = new Font(pfc.Families[0], 14, FontStyle.Regular);
            label8.Font = new Font(pfc.Families[0], 9.75f, FontStyle.Regular);
            label13.Font = new Font(pfc.Families[0], 10, FontStyle.Regular);
            label14.Font = new Font(pfc.Families[0], 10, FontStyle.Regular);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            
            listView1.Items.Clear();
            comboBox1.SelectedIndex = -1;
            lbconvertflag = false; // important
            pictureBox2.Visible = true;
            pictureBox4.Visible = false;

            if (!p.checkServer())
            {
                MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. Could also be a server-fault. Please try again later. Leaderboard-data could not be retrieved!");
                return;
            }

            p.readFromServer("username");
            loadLatestLB("leaderboard");
            p.readFromServer("leaderboardlist");
            p.readFromServer("uniqueplayers");
            p.readFromServer("participated");
            //MessageBox.Show(p.leaderboardlist);
            comboBox1.Items.Clear();

            foreach (string item in p.leaderboardlist.Split("#")) {
                if(item.Length > 0)
                    comboBox1.Items.Add(item);
            }

        }

        private string getPrice()
        {
            try
            {
                // Get actual HELLO token price
                WebClient client = new WebClient();

                client.Headers.Add("User-Agent: Other");
                client.Headers.Add("Accept: text/html");
                client.Headers.Add("Accept-Encoding: gzip, deflate, br");

                var responseStream = new GZipStream(client.OpenRead("https://coinmarketcap.com/currencies/hello-labs/"), CompressionMode.Decompress);
                var reader = new StreamReader(responseStream);
                string downloadString = reader.ReadToEnd();

                string price = "";
                int index = downloadString.IndexOf("<div class=\"priceValue \"><span>") + 32;
                bool run = true;
                for (int i = index; run; i++)
                {
                    if (downloadString[i] != '<')
                    {
                        price += downloadString[i];
                    }
                    else
                        run = false;
                }
                //MessageBox.Show(price.ToString());
                return price;
            }
            catch(Exception) {
                return "0.00";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            comboBox1.SelectedIndex = -1;
            lbconvertflag = false; // important
            pictureBox2.Visible = true;
            pictureBox4.Visible = false;
            loadLatestLB("leaderboard");
            //MessageBox.Show(p.leaderboard);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // VERY important, SelectedIndex was changed programmtically,
            // so don't call this function on the empty element!!!

            if (comboBox1.SelectedIndex != -1)
            {
                pictureBox2.Visible = false;
                pictureBox4.Visible = false;
                string day = (string)comboBox1.SelectedItem;
                //MessageBox.Show(day);
                p.writeToServer("getleaderboard", day);
                // From 19_11_2022: rewardpool and rewards in HELLO-Format instead of DD
                int d = Int32.Parse(day[0].ToString() + day[1].ToString());
                int m = Int32.Parse(day[3].ToString() + day[4].ToString());
                int y = Int32.Parse(day[6].ToString() + day[7].ToString() + day[8].ToString() + day[9].ToString());
                if((m == 10 && y == 2022) || (m == 11 && d < 19 && y == 2022))
                {
                    lbconvertflag = true;
                    //MessageBox.Show("old");
                }
                else
                {
                    lbconvertflag = false;  // Set previously used (?) flag to false
                }

                listView1.Items.Clear();
                loadLatestLB("NULL");
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items[index].BackColor = standardcolor;
            }
            catch { }

            if (!p.checkServer())
            {
                MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. Could also be a server-fault. Please try again later. Leaderboard-data could not be retrieved!");
                return;
            }

            textBox3.Text = "";  // Empty

            if (textBox4.Text == "")
                return;

            index = -1;
            foreach (string item in p.leaderboard.Split("#"))
            {
                index++;
                if(textBox4.Text == item.Split(";")[0])  // Username found in the leaderboard
                {
                    if (lbconvertflag)
                    {
                        textBox3.Text = ((Int32.Parse(item.Split(";")[2]))/100 - Int32.Parse(p.ChangeTextBoxText)).ToString();
                    }
                    else
                    {
                        textBox3.Text = (Int32.Parse(item.Split(";")[2]) - Int32.Parse(p.ChangeTextBoxText)).ToString();
                    }
                    // Write username to file
                    p.writeToServer("username", textBox4.Text);
                    listView1.Items[index].BackColor = Color.LightGreen;
                    listView1.Items[index].Selected = true;
                    listView1.Items[index].Focused = true;
                    listView1.EnsureVisible(index);
                    break;
                }
            }

            if (textBox3.Text == "")
            {
                textBox1.Text = "";
                return;
            }

            label17.Text = (label15.Text = await Task.Run(() => getPrice())) + " $";
            label17.Visible = true;

            if(label17.Text == "0.00 $")
            {
                label18.Visible = true;
            }
            else
            {
                label18.Visible = false;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                this.button4_Click(null, null);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label15_TextChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("changed");
            if (label15.Text.Length > 0) {
                textBox1.Text = (Int32.Parse(textBox3.Text) * float.Parse(label15.Text, CultureInfo.InvariantCulture.NumberFormat)).ToString();
                label15.Text = "";
            }
            //button4.Text = "Calculate";
            //button4.Font = new Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            // .Show((Int32.Parse(textBox3.Text) * float.Parse(textBox1.Text, CultureInfo.InvariantCulture.NumberFormat)).ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            comboBox1.SelectedIndex = -1;
            lbconvertflag = false; // important
            pictureBox2.Visible = false;
            pictureBox4.Visible = true;
            loadLatestLB("getleaderboardactual");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form6_Activated(object sender, EventArgs e)
        {

        }

        private void button5_Click_2(object sender, EventArgs e)
        {
            MessageBox.Show("More interesting features will be released in version 1.0.4 here!");
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
