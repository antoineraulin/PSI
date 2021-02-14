using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace TD_2
{
    class Program
    {
        static void Main(string[] args)
        {
            MyImage test = new MyImage(".\\fox.bmp");
            Bgr[,] rotation = test.Rotation(30);
            
            
            //test.From_Image_To_File(".\\sharp_out.bmp");
            Console.ReadLine();
        }
    }
}
