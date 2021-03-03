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
            MyImage test = new MyImage(".\\sharp.bmp");
            MyImage g = test.RotationV2(66, false);
            g.From_Image_To_File(".\\resize.bmp");
            Console.ReadLine();
        }
    }
}
