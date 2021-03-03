using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace TD_3
{
    class Program
    {
        static void Main(string[] args)
        {
            MyImage test = new MyImage(".\\fox.bmp");
            test.ToBW();
            test.From_Image_To_File(".\\bw.bmp");
            Console.ReadLine();
        }
    }
}
