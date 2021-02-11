using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;

namespace TD_2
{
    class MyImage
    {
        #region PRIVATE


        private string type;
        private int size, offset, height, width, depth;
        private RGB[,] data;



        #endregion

        #region PUBLIC

        public MyImage(string myfile)
        {
            byte[] file = File.ReadAllBytes(myfile);

            byte[] header = new byte[14];
            //Métadonnées du fichier
            for (int i = 0; i < 14; i++)
            {
                header[i] = file[i];
            }
            Console.WriteLine(header[0]);
            Console.WriteLine(header[1]);
            if (header[0] != 66 || header[1] != 77)
            {
                throw new ArgumentException("Le fichier fournit n'est pas un Bitmap");
            }
            Console.WriteLine("C'est un bitmap");
            size = 0;
            for (int index = 0; index < 4; index++)
            {
                size += file[index + 2] * (int)Math.Pow(256, index);
            }

            offset = 0;
            for (int index = 0; index < 4; index++)
            {
                offset += file[index + 10] * (int)Math.Pow(256, index);
            }
            Console.WriteLine($"size : {size}");
            Console.WriteLine($"offset : {offset}");

            width = 0;
            for (int index = 0; index < 4; index++)
            {
                width += file[index + 18] * (int)Math.Pow(256, index);
            }

            height = 0;
            for (int index = 0; index < 4; index++)
            {
                height += file[index + 22] * (int)Math.Pow(256, index);
            }

            depth = 0;
            for (int index = 0; index < 3; index++)
            {
                depth += file[index + 28] * (int)Math.Pow(256, index);
            }

            Console.WriteLine($"width : {width}");
            Console.WriteLine($"height : {height}");
            Console.WriteLine($"depth : {depth}");



            //L'image elle-même
            data = new RGB[height, width];
            Console.WriteLine("bytes image:");
            int widthpp = 4 * (((width * (depth / 8)) + 4 / 2) / 4);
            //int widthpp = (int)((width * (depth / 8)) / 4) * 4;

            for (int i = offset; i < file.Length; i = i + widthpp)
            {
                for (int j = i; j < i + width * (depth / 8); j += depth / 8)
                {
                    int avg = (file[j] + file[j + 1] + file[j + 2]) / (depth / 8);
                    if(avg < file[j])
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    else if(avg < file[j + 1])
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if(avg < file[j + 2])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }else if(avg < 128)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write((file[j] + file[j + 1] + file[j + 2]) / (depth / 8) + " ");
                    if(depth / 8 == 3)
                    {
                        //RGB
                        //data[i / (width * (depth / 8)), j-i] = new RGB(file[j], file[j+1], file[j+2]);
                    }


                    //data[i/width, j-i]
                    //data[i/width, j-i] = file[offset+j];

                }
                Console.WriteLine();
            }
            /*Console.WriteLine("data :");
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j<width; j++)
                {
                    Console.WriteLine(data[i, j] + " ");
                }
                Console.WriteLine();
            }*/

        }

        #endregion
    }
}
