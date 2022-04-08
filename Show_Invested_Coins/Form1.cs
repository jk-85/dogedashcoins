using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Show_Invested_Coins
{
    public partial class Form1 : Form
    {
        private void readFile(string filename, string what_to_change)
        {
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(filename);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the line to console window
                    //Console.WriteLine(line);
                    if (what_to_change == "textbox")
                        textBox1.Text = line;
                    if (what_to_change == "checkbox")
                    {
                        if (line.ToLower() == "false")
                        {
                            checkBox2.CheckState = System.Windows.Forms.CheckState.Unchecked;
                            checkBox2.Checked = false;
                        }
                        else
                        {
                            checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
                            checkBox2.Checked = true;
                        }
                    }
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("File could not be read!");
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }

        private void writeFile(string filename, string content)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(filename);
                //Write a line of text
                sw.WriteLine(content);
                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("File could not be written!");
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }
        public Form1()
        {
            InitializeComponent();

            // some changes after:
            readFile("dd_coins", "textbox");
            readFile("dd_config", "checkbox");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + 500).ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*string message = "Do you really want to clear spent coins?";
            string title = "Question";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                textBox1.Text = "0";
            }*/

            if (checkBox2.CheckState == System.Windows.Forms.CheckState.Checked)
            {
                Form2 form2 = new Form2(this);
                form2.TopMost = true;
                form2.ShowDialog();
            }
            else
            {
                ChangeTextBoxText = "0";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            writeFile("dd_coins", textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + 700).ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + 300).ToString();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.TopMost = true;
            form3.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://play.dogedash.com");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (TopMost)
            {
                TopMost = false;
            }
            else
            {
                TopMost = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            writeFile("dd_config", checkBox2.Checked.ToString());
        }
    }
}
