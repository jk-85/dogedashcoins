// Erstellen mit:
// dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true --self-contained

// TODO: https://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows

// Text-Probleme mit VS 17.4.1 gehabt in Form6.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Show_Invested_Coins
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            Application.Run(form);
        }
    }
}