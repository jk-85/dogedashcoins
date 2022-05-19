using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Show_Invested_Coins
{
    internal class SaveImages
    {
        public static void ThreadProc(Bitmap Image)
        {
            MemoryStream mem1 = new MemoryStream();
            MemoryStream mem2 = new MemoryStream();

            Image.Save(mem1, ImageFormat.Png);
            mem1.WriteTo(mem2);
            mem1.Dispose();
            Thread.Sleep(500);
            mem2.Dispose();
        }

        public SaveImages(Bitmap image)
        {
           Thread t = new Thread(() => ThreadProc(image));
            t.Start();
            //Console.WriteLine("Main thread: Call Join(), to wait until ThreadProc ends.");
            //t.Join();
        }
    }
}
