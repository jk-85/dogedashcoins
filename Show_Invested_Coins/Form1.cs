using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Net.Sockets;

namespace Show_Invested_Coins
{
    public partial class Form1 : Form
    {
        private string data;
        public int easy_coins, normal_coins, elite_coins;
        public bool automode;
        public bool useLocalStorage;
        private bool gameMenuScreenshotDone;
        public string userpass;
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static string RandomString(int length)
        {
            Random random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Boolean testRandomString(string str)
        {
            int valcount = 0;

            if (str.Length != 10) return false;

            for(int i=0;i<10;i++)
            {
                for(int j=0;j<chars.Length;j++)
                {
                    if(chars[j] == str[i])
                    {
                        valcount++;
                        break;
                    }
                }
            }

            if (valcount == 10)
                return true;
            else
                return false;
        }

        private int readFile(string _filename, string what_to_change)
        {
            string filename =  data + _filename;

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
                    if (what_to_change == "textbox")
                        textBox1.Text = line;
                    if (what_to_change == "checkbox")
                    {
                        if (line.Split('|')[0] == "False") {
                            checkBox1.CheckState = CheckState.Unchecked;
                            checkBox2.Checked = false;
                        }
                        else
                        {
                            checkBox1.CheckState = CheckState.Checked;
                            checkBox1.Checked = true;
                        }

                        if (line.Split('|')[1] == "False")
                        {
                            checkBox2.CheckState = CheckState.Unchecked;
                            checkBox2.Checked = false;
                        }
                        else
                        {
                            checkBox2.CheckState = CheckState.Checked;
                            checkBox2.Checked = true;
                        }

                        /*if (line.ToLower() == "false")
                        {
                            checkBox2.CheckState = CheckState.Unchecked;
                            checkBox2.Checked = false;
                        }
                        else
                        {
                            checkBox2.CheckState = CheckState.Checked;
                            checkBox2.Checked = true;
                        }*/

                    }
                    if (what_to_change == "coinvalues") {
                        string[] values = line.Split(",");
                        easy_coins = Int32.Parse(values[0]);
                        normal_coins = Int32.Parse(values[1]);
                        elite_coins = Int32.Parse(values[2]);
                    }
                    if(what_to_change == "userpass")
                    {
                        userpass = line;
                    }
                    // Read the next line
                    line = sr.ReadLine();
                }

                if(easy_coins == 0) { easy_coins = 300; }
                if (normal_coins == 0) { normal_coins = 500; }
                if (elite_coins == 0) { elite_coins = 700; }

                sr.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("File "+_filename+ "could not be read!");
                if (_filename == "dd_uselocal")
                {
                    // dd_local not there
                    return -2;
                }
                else
                {
                    return -1;
                }
            }
                return 0;
        }

        public void writeFile(string filename, string content)
        {
            filename = data + filename;
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

        public void deleteFile(string filename)
        {
            File.Delete(data + filename);
        }

        public Form1()
        {
            InitializeComponent();

            data = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString() + @"\DogeDashCoins\";
            Directory.CreateDirectory(data);
            automode = false;
            gameMenuScreenshotDone = false;

            // TODO HERE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (readFile("dd_uselocal", "") == -2 && readFile("dd_account", "") == 0) // dd_uselocal not there but dd_account there
            {
                if (!checkServer())
                {
                    using (DialogCenteringService centeringService = new DialogCenteringService(this)) {
                        MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. We will use your local storage for saving your data as long the blockage isn't removed!");
                        useLocalStorage = true;
                        writeFile("dd_uselocal", "");
                        deleteFile("dd_account");
                    }
                }
                else // Server is on, but both files exist, remove all local dd_files except dd_account and use dd_account
                {
                    useLocalStorage = false;
                    deleteFile("dd_uselocal");
                    deleteFile("dd_coins");
                    deleteFile("dd_config");
                    deleteFile("dd_values");
                }
            }
            else
            {
                if (readFile("dd_uselocal", "") == -1 && readFile("dd_account", "") == -1) {  // If nothing exists
                    // File exists, use only local storage
                    useLocalStorage = true;
                    loadOfflineValuesIntoWindow();
                }
            }

            if (!useLocalStorage) {
                // create user pass (account) and file automatically at the very first run, if not needed later, delete that account and use
                // that one the user inputs in the options dialog
                loadOnlineValuesIntoWindow();
            }
            else
            {
                // If local storage is set, user only can use server, if he actively checks the box in the options dialog!
            }
        }

        public void loadOnlineValuesIntoWindow()
        {
            if (readFile("dd_account", "") == -1)
            {
                // Create new account
                userpass = RandomString(10);
                writeFile("dd_account", userpass);
            }
            else
            {
                // File (account) already exists, read userpass
                readFile("dd_account", "userpass");
            }

            // TODO: Read coins and coin values in one server connection at once:
            readFromServer("coins");
            readFromServer("config");  // Main-Window checkbox-states
        }

        public void loadOfflineValuesIntoWindow()
        {
            // load values from drive
            readFile("dd_coins", "textbox");
            readFile("dd_config", "checkbox");
            readFile("dd_values", "coinvalues");
            label4.Text = "+" + easy_coins;
            label5.Text = "+" + normal_coins;
            label6.Text = "+" + elite_coins;
        }

        public async void writeToServer(string command, string sendStr)
        {
            if (!useLocalStorage)
            {
                const int PORT_NO = 7075;
                const string SERVER_IP = "serviceserver.ddnsfree.com";

                string textToSend = userpass + "_" + command + "_write_" + sendStr;

                // create a TCPClient object at the IP and port no.
                try
                {
                    TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = client.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                    // send
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                    client.Close();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode.ToString() == "ConnectionRefused")
                    {
                        // Server is down
                        MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. We will use your local storage for saving your data as long the blockage isn't removed!");
                        useLocalStorage = true;
                        writeFile("dd_uselocal", "");
                    }
                }
            }
        }

        public bool checkServer()
        {
            const int PORT_NO = 7075;
            const string SERVER_IP = "serviceserver.ddnsfree.com";

            string textToSend = "test";

            // create a TCPClient object at the IP and port no.
            try
            {
                TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                NetworkStream nwStream = client.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                // send
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                // read "ok"
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                client.Close();

                MessageBox.Show(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                if (Encoding.ASCII.GetString(bytesToRead, 0, bytesRead) == "ok") {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (SocketException ex)
            {
                /*if (ex.SocketErrorCode.ToString() == "ConnectionRefused")
                {
                    // Server is down
                    MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. We will use your local storage for saving your data as long the blockage isn't removed!");
                    useLocalStorage = true;
                    writeFile("dd_uselocal", "");
                }*/
                //MessageBox.Show(ex.Message);
                //MessageBox.Show(ex.NativeErrorCode.ToString());
                return false;
            }

        }


        public async void readFromServer(string command)
        {
            if (!useLocalStorage)
            {
                const int PORT_NO = 7075;
                const string SERVER_IP = "serviceserver.ddnsfree.com";

                string textToSend = userpass + "_" + command + "_read";

                // create a TCPClient object at the IP and port no.
                try
                {
                    TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = client.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

                    // send
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);


                    // read
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);

                    if (command == "coins") {
                        textBox1.Text = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[0];
                        easy_coins = Int32.Parse(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[1].Split(',')[0]);
                        normal_coins = Int32.Parse(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[1].Split(',')[1]);
                        elite_coins = Int32.Parse(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[1].Split(',')[2]);
                        label4.Text = "+" + easy_coins.ToString(); label5.Text = "+" + normal_coins.ToString(); label6.Text = "+" + elite_coins.ToString();
                    }
                    if(command == "config") {
                        checkBox1.CheckState = ((Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[0]) == "True") ? CheckState.Checked : CheckState.Unchecked;
                        checkBox2.CheckState = ((Encoding.ASCII.GetString(bytesToRead, 0, bytesRead).Split('|')[1]) == "True") ? CheckState.Checked : CheckState.Unchecked;
                    }
                    client.Close();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode.ToString() == "ConnectionRefused")
                    {
                        // Server is down
                        MessageBox.Show("Warning, the server is down, or your firewall blocks access to it. We will use your local storage for saving your data. If you want online storage again, please enable it in the options!", "DogeDashCoinCalculator");
                        useLocalStorage = true;
                        writeFile("dd_uselocal", "");
                    }
                }
            }
        }

        public bool testServer()
        {
            const int PORT_NO = 7075;
            const string SERVER_IP = "serviceserver.ddnsfree.com";

            TcpClient tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(SERVER_IP, PORT_NO);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + normal_coins).ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button7.Visible = button8.Visible = button9.Visible = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (checkBox2.CheckState == CheckState.Checked)
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
            if (useLocalStorage)
            {
                writeFile("dd_coins", textBox1.Text);
            }
            else
            {
                writeToServer("coins", textBox1.Text);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + elite_coins).ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = (Int32.Parse(textBox1.Text) + easy_coins).ToString();
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
            string data = checkBox1.Checked.ToString() + "|" + checkBox2.Checked.ToString();

            if (TopMost)
            {
                TopMost = false;
            }
            else
            {
                TopMost = true;
            }

            if (useLocalStorage)
            {
                writeFile("dd_config", data);
            }
            else
            {
                writeToServer("config", data);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://poocoin.app/tokens/0x7ae5709c585ccfb3e61ff312ec632c21a5f03f70");
        }

        private async void HookManager_MouseDown(object sender, MouseEventArgs e)
        {
            //Debug.Print(e.X.ToString() + " " + e.Y.ToString());
            Task<int> numPictures = getGameButtons(e);
            int result = await numPictures;

            if(result > 0)
            {
                Debug.Print("numPictures =" + result);
            }
        }

        private async void test()
        {
            const int count = 512;
            int counter = 0;
            var text = new StringBuilder(count);
            await Task.Delay(2000); // wait, let user switch to browser and then start

            string exe = (GetActiveProcess().MainModule.FileName).Split(@"\").Last().ToLower();

            HookManager.MouseDown -= HookManager_MouseDown; // disable temporarily
            IntPtr handle = GetForegroundWindow();
            int length = GetWindowText(handle, text, count);
            while (length > 0)
            {
                var image = ScreenCapture.CaptureActiveWindow(handle, counter++);
                image.Save("ingame" + counter + ".png", ImageFormat.Png);
                await Task.Delay(1000);
            }
        }

        // Will be called continuously as long HookManager is not disabled!!
        private async Task<int> getGameButtons(MouseEventArgs e)
        {
            //await Task.Delay(1500); // Game is loading...
            const int count = 512;
            // Max number of screenshots to take (in hope there's the main screen we need).
            const int max_pics = 14;
            var text = new StringBuilder(count);
            bool ok = false;
            int counter = 0;

            if(automode)
            {
                Debug.Print("in event...");
                //if (gameMenuScreenshotDone)
                //    return;

                string exe = (GetActiveProcess().MainModule.FileName).Split(@"\").Last().ToLower();
                IntPtr handle = GetForegroundWindow();
                if (GetWindowText(handle, text, count) > 0)
                {
                    ok = false;
                    switch (exe)
                    {
                        case "iexplore.exe":
                        case "firefox.exe":
                        case "edge.exe":
                        case "opera.exe":
                        case "brave.exe":
                        case "vivaldi.exe":
                        case "duckduckgo.exe":
                        case "safari.exe":
                        case "chrome.exe":
                        case "chromium.exe":
                            ok = true;
                            break;
                        default:
                            ok = false;
                            break;
                    }
                }

                string title = text.ToString().ToLower();
                //if(ok && (title == "dogedash") && !title.Contains("game portal") && !title.Contains("coin calculator") && !title.Contains("token tracker"))
                if(ok && (title == "dogedash - mozilla firefox") ||
                    (title == "dogedash - brave") ||
                    (title == "dogedash - google chrome") ||
                    (title == "dogedash - microsoft edge") ||
                    (title == "dogedash - opera") ||
                    (title == "dogedash - safari")) {
                    ok = false;
                    // We are at the main screen now
                    // System.ArgumentException can happen if user clicks away from browser-window
                    /*MemoryStream mem1 = new MemoryStream();
                    MemoryStream mem2 = new MemoryStream();

                    image.Save(mem1, ImageFormat.Png);
                    mem1.WriteTo(mem2);
                    mem1.Dispose();
                    await Task.Delay(500);
                    mem2.Dispose();*/

                    HookManager.MouseDown -= HookManager_MouseDown;
                    // get screenshot several times because of unknown game loading time
                    if (gameMenuScreenshotDone == false) { // important
                        for (int i = 0; i < max_pics; i++)
                        {
                            var image = ScreenCapture.CaptureActiveWindow(handle, counter++);
                            // Hope to catch one picture with the main-menu
                            image.Save("mainscreen" + i + ".png", ImageFormat.Png);
                            await Task.Delay(1000);
                        }
                        gameMenuScreenshotDone = true;

                        return max_pics; // important also
                    }
                }
               //await Task.Delay(1000);
            } return 0;
        }

        public Process GetActiveProcess()
        {
            IntPtr handle = GetForegroundWindow();
            uint pID;

            GetWindowThreadProcessId(handle, out pID);

            return Process.GetProcessById((Int32)pID);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Debug.Print("automode: "+ automode);
            // TODO (Switch button type to permanent on / permanent off button and add or subtract handler: += or -=)
            if (automode == false) {
                HookManager.MouseDown += HookManager_MouseDown;
                automode = true;
            }

            //Bitmap img = new("button_pressed.png");
            // < >
            
            /*
            for (int y = img.Height; y >=0; y--)
            { 
                
            }
            
            

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                }
            }*/
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //Debug.Print("automode_" + automode);
            if (automode == true)
            {
                HookManager.MouseDown -= HookManager_MouseDown;
                automode = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            string data = checkBox1.Checked.ToString() + "|" + checkBox2.Checked.ToString();

            if (useLocalStorage)
            {
                writeFile("dd_config", data);
            }
            else
            {
                writeToServer("config", data);
            }
        }

        public void writeCheckBoxStatesToFile()
        {
            string data = checkBox1.Checked.ToString() + "|" + checkBox2.Checked.ToString();
            writeFile("dd_config", data);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            test();
        }


        private void button6_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4(this);
            form4.TopMost = true;
            form4.ShowDialog();
        }
    }
}
